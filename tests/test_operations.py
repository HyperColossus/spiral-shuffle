"""Offline deployment tests. No SSH, live systemd, sudo or game data required."""
import importlib.util
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest import mock
from urllib.request import Request, urlopen
import threading

import auto_deploy as deploy
import install_services as installer
import portal_api as api
import service_runtime as runtime

A = "a" * 40
B = "b" * 40


class GateTests(unittest.TestCase):
    def test_only_successful_latest_main_push_for_exact_revision_is_accepted(self):
        run = dict(head_sha=B, head_branch="main", event="push", path=deploy.WORKFLOW,
                   run_number=1, run_attempt=1, status="completed", conclusion="success")
        self.assertEqual(deploy.gate_result([run], B), "passed")
        for change in (dict(head_sha=A), dict(head_branch="feature"), dict(event="pull_request"), dict(path="other.yml")):
            self.assertEqual(deploy.gate_result([{**run, **change}], B), "pending")
        self.assertEqual(deploy.gate_result([run, {**run,"run_attempt":2,"status":"in_progress"}], B), "pending")
        self.assertEqual(deploy.gate_result([run, {**run,"run_attempt":2,"conclusion":"failure"}], B), "failed")

    def test_deployment_scope_does_not_rebuild_assets_or_change_privileged_components(self):
        self.assertEqual(deploy.deployment_kind(["docs/index.html"]), "website")
        self.assertEqual(deploy.deployment_kind(["portal_api.py","tests/test_portal_api.py"]), "api")
        self.assertEqual(deploy.deployment_kind(["PortalRegistration.cs","portal_api.py"]), "bridge")
        for path in ("pack_assets.py","prepare_source.py",".runtime/saves/players","docker/ravendb.compose.yml",
                     "portal/access_firewall.py","portal/spiral-game.service","auto_deploy.py","../portal_api.py"):
            with self.subTest(path=path), self.assertRaises(ValueError):
                deploy.deployment_kind([path])

    def test_origin_cannot_be_redirected_to_another_repository(self):
        deploy.validate_origin("https://github.com/HyperColossus/spiral-shuffle.git")
        with self.assertRaises(ValueError):
            deploy.validate_origin("https://github.com/someone/another-repo.git")


class TransactionTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        server = self.root / ".runtime/server"
        server.mkdir(parents=True)
        (server / "Imlight.Director.dll").write_bytes(b"old-director")
        (server / "Imlight.CoreLib.dll").write_bytes(b"working-entry-fix")
        self.director = server / "Imlight.Director.dll"
        self.core = server / "Imlight.CoreLib.dll"
        for name, value in (("ROOT",self.root),("PRIVATE",self.root/".runtime/deploy"),("STATE",self.root/".runtime/deploy/state.json")):
            patch=mock.patch.object(deploy,name,value)
            patch.start()
            self.addCleanup(patch.stop)
        self.git = self.patch("git")
        self.systemctl = self.patch("systemctl")
        self.checks = self.patch("local_checks")
        self.health = self.patch("health")
        self.command = self.patch("command")
        self.command.side_effect = lambda *args, **kwargs: self.director.write_bytes(b"new-director")

    def patch(self, name):
        patch=mock.patch.object(deploy,name)
        result=patch.start()
        self.addCleanup(patch.stop)
        return result

    def test_api_update_restarts_only_api_and_preserves_live_binaries(self):
        deploy.apply_candidate(A,B,"api")
        self.assertEqual(self.systemctl.call_args_list,[mock.call("stop",deploy.API_UNIT),mock.call("start",deploy.API_UNIT)])
        self.assertEqual(self.director.read_bytes(),b"old-director")
        self.assertEqual(self.core.read_bytes(),b"working-entry-fix")

    def test_bridge_update_restarts_game_and_api_but_keeps_corelib(self):
        deploy.apply_candidate(A,B,"bridge")
        self.assertEqual(self.director.read_bytes(),b"new-director")
        self.assertEqual(self.core.read_bytes(),b"working-entry-fix")
        self.assertEqual(self.systemctl.call_args_list,[mock.call("stop",deploy.API_UNIT),mock.call("stop",deploy.GAME_UNIT),
            mock.call("start",deploy.GAME_UNIT),mock.call("start",deploy.API_UNIT)])

    def test_health_failure_restores_previous_code_and_bridge_binary(self):
        self.health.side_effect=[RuntimeError("unhealthy"),None]
        with self.assertRaises(RuntimeError):
            deploy.apply_candidate(A,B,"bridge")
        self.assertEqual(self.director.read_bytes(),b"old-director")
        self.assertEqual(self.core.read_bytes(),b"working-entry-fix")
        self.git.assert_any_call("checkout","--detach",A)
        self.assertEqual(self.health.call_count,2)

    def test_failed_preflight_never_stops_running_services(self):
        self.checks.side_effect=RuntimeError("test failed")
        with self.assertRaises(RuntimeError):
            deploy.apply_candidate(A,B,"bridge")
        self.systemctl.assert_not_called()
        self.assertEqual(self.director.read_bytes(),b"old-director")

    def test_corelib_change_is_detected_and_rolled_back(self):
        def unexpected(*args,**kwargs):
            self.director.write_bytes(b"new-director")
            self.core.write_bytes(b"unexpected")
        self.command.side_effect=unexpected
        with self.assertRaises(RuntimeError):
            deploy.apply_candidate(A,B,"bridge")
        self.assertEqual(self.core.read_bytes(),b"working-entry-fix")
        self.assertEqual(self.director.read_bytes(),b"old-director")

    def test_failure_of_rollback_is_distinguished_from_successful_recovery(self):
        self.health.side_effect=RuntimeError("unhealthy")
        with self.assertRaises(deploy.RecoveryFailed):
            deploy.apply_candidate(A,B,"bridge")

    def test_website_update_does_not_restart_game_or_api(self):
        deploy.apply_candidate(A,B,"website")
        self.systemctl.assert_not_called()

    def test_interrupted_bridge_deployment_recovers_from_journal(self):
        backup=deploy.PRIVATE/"backups"/B
        backup.mkdir(parents=True)
        (backup/self.director.name).write_bytes(b"old-director")
        (backup/self.core.name).write_bytes(b"working-entry-fix")
        self.director.write_bytes(b"partial-new-director")
        self.git.side_effect=lambda *args: "" if args[0]=="status" else B if args[0]=="rev-parse" else ""
        state={"revision":A,"candidate":B,"status":"deploying","kind":"bridge"}
        deploy.recover_interrupted(state)
        self.assertEqual(self.director.read_bytes(),b"old-director")
        self.assertEqual(json.loads(deploy.STATE.read_text())["status"],"rolled_back")


class DeploymentDecisionTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name)
        self.state=self.root/"state.json"
        self.state.write_text(json.dumps({"revision":A,"status":"current"}))
        for name,value in (("PRIVATE",self.root),("STATE",self.state)):
            patch=mock.patch.object(deploy,name,value);patch.start();self.addCleanup(patch.stop)
        self.platform=mock.patch.object(deploy.sys,"platform","linux");self.platform.start();self.addCleanup(self.platform.stop)
        patch=mock.patch.object(deploy.os,"geteuid",return_value=1000,create=True);patch.start();self.addCleanup(patch.stop)

    def git(self,*args):
        if args==("remote","get-url","origin"):return "https://github.com/HyperColossus/spiral-shuffle.git"
        if args[0]=="status":return ""
        if args==("rev-parse","HEAD"):return A
        if args==("rev-parse","FETCH_HEAD"):return B
        if args[0]=="show":return '{"version":1,"scope":"portal-bridge"}'
        if args[0]=="diff":return "portal_api.py"
        return ""

    def test_failed_checks_never_modify_checkout_or_restart(self):
        with mock.patch.object(deploy,"git",side_effect=self.git),mock.patch.object(deploy,"check_gate",return_value="failed"),mock.patch.object(deploy,"apply_candidate") as apply:
            deploy.deploy()
        apply.assert_not_called()
        self.assertEqual(json.loads(self.state.read_text())["status"],"blocked_failed_checks")

    def test_successful_checks_are_required_before_candidate_is_applied(self):
        with mock.patch.object(deploy,"git",side_effect=self.git),mock.patch.object(deploy,"check_gate",return_value="passed"),mock.patch.object(deploy,"apply_candidate") as apply:
            deploy.deploy()
        apply.assert_called_once_with(A,B,"api")
        self.assertEqual(json.loads(self.state.read_text())["revision"],B)

    def test_old_main_without_service_contract_cannot_replace_current_test_server(self):
        def old_main(*args):
            if args[0]=="show":raise subprocess.CalledProcessError(1,["git"])
            return self.git(*args)
        with mock.patch.object(deploy,"git",side_effect=old_main),mock.patch.object(deploy,"apply_candidate") as apply:
            deploy.deploy()
        apply.assert_not_called()
        self.assertEqual(json.loads(self.state.read_text())["status"],"waiting_for_main")


class InstallationSafetyTests(unittest.TestCase):
    def test_missing_user_units_are_skipped_on_first_install(self):
        missing=type("Result",(),{"stdout":"not-found\n"})()
        with mock.patch.object(installer.subprocess,"run",return_value=missing),mock.patch.object(installer,"run") as run:
            installer.stop_existing_user_units()
        run.assert_not_called()

    def test_dead_supervised_pid_cleanup_never_removes_another_pidfile(self):
        with tempfile.TemporaryDirectory() as folder:
            root=Path(folder)
            paths={name:root/(name+".pid") for name in ("server","patch")}
            paths["server"].write_text("999999991")
            paths["patch"].write_text("123")
            ctl=type("Ctl",(),{"_pid_path":staticmethod(lambda name:paths[name])})()
            runtime.clean_dead_pidfiles(ctl,{"server":999999991,"patch":999999992})
            self.assertFalse(paths["server"].exists())
            self.assertEqual(paths["patch"].read_text(),"123")


class StatusPrivacyTests(unittest.TestCase):
    def test_status_endpoint_returns_only_public_revision_and_health_metadata(self):
        with tempfile.TemporaryDirectory() as folder,mock.patch.object(api,"ROOT",Path(folder)):
            path=Path(folder)/".runtime/deploy/state.json"
            path.parent.mkdir(parents=True)
            path.write_text(json.dumps({"revision":A,"status":"current","updated_at":10,"private":"DO NOT EXPOSE","failure_type":"private details"}))
            api.Handler.settings=type("Settings",(),{"origin":"https://spiral.hypercoloss.us"})()
            server=api.ThreadingHTTPServer(("127.0.0.1",0),api.Handler)
            thread=threading.Thread(target=server.serve_forever,daemon=True);thread.start()
            try:
                with urlopen(f"http://127.0.0.1:{server.server_port}/deployment",timeout=2) as response:
                    result=json.load(response)
                self.assertEqual(result["deployment"],{"revision":A,"status":"current","updated_at":10})
                self.assertNotIn("DO NOT EXPOSE",str(result))
            finally:
                server.shutdown();server.server_close();thread.join(timeout=2)


if __name__=="__main__":
    unittest.main()
