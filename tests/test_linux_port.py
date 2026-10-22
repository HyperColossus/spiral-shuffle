"""Safe offline tests; run inside fork with `python3 -m unittest discover -s tests`."""
from __future__ import annotations

import importlib.util
import json
import os
import stat
import sys
import tempfile
import types
import unittest
from pathlib import Path
from unittest import mock


# setup_linux.py requires `setup.py` in the real fork. For tests of our Linux
# wrapper, supply a minimal stand-in for the Windows build module.
ORIGINAL = types.ModuleType("setup")
ORIGINAL.REVISION = "V_r806919.Wizard_1_610"
ORIGINAL.UPSTREAM = Path("/not-existing-in-tests")

def replace_setting(content, key, value):
    import re
    found = re.subn(rf"(?m)^{re.escape(key)}\s*=.*$", lambda m: f"{key} = {value}", content)
    if found[1] != 1:
        raise ValueError(key)
    return found[0]

ORIGINAL.replace_setting = replace_setting
sys.modules.setdefault("setup", ORIGINAL)
BASE = Path(__file__).resolve().parents[1]

spec = importlib.util.spec_from_file_location("linux_port_setup", BASE / "setup_linux.py")
installer = importlib.util.module_from_spec(spec)
spec.loader.exec_module(installer)
spec = importlib.util.spec_from_file_location("linux_port_ctl", BASE / "linux_ctl.py")
ctl = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ctl)


class LinuxSetupTests(unittest.TestCase):
    def test_prerequisites_detect_missing_assemblies_and_tools(self):
        with tempfile.TemporaryDirectory() as folder:
            errors = installer.check_prerequisites(Path(folder), None)
            self.assertIn("dotnet executable (.NET 10 SDK required)", errors)
            self.assertTrue(any("Root.wad" in item for item in errors))
            self.assertTrue(any("QuestTemplates" in item for item in errors))

    def test_random_credentials_are_persistent_and_private(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "RUNTIME", Path(folder)):
            first = installer.account_credentials()
            second = installer.account_credentials()
            self.assertEqual(first, second)
            self.assertNotEqual(first["password"], "compsci")
            self.assertGreaterEqual(len(first["password"]), 25)
            self.assertEqual(stat.S_IMODE((Path(folder) / "login.json").stat().st_mode), 0o600)

    def test_refuses_old_unsafe_default_password(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "RUNTIME", Path(folder)):
            (Path(folder)/"login.json").write_text(json.dumps({
                "username":"admin","password":"compsci","session_salt":"test"}))
            with self.assertRaisesRegex(ValueError, "insecure"):
                installer.account_credentials()

    def test_stage_linux_deps_and_ignores_windows_apphost(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "SERVER", Path(folder)/"target"):
            source = Path(folder)/"revive/Imlight"
            source.mkdir(parents=True)
            (source/"Imlight.Director.deps.json").write_text(json.dumps({"runtimeTarget":{"name":".NETCoreApp,Version=v10.0/linux-x64"}}))
            (source/"Imlight.Director.dll").write_bytes(b"demo-linux-assembly")
            (source/"Imlight.Director.exe").write_bytes(b"windows-apphost")
            (source/"RavenDBServer").mkdir()
            (source/"RavenDBServer/server").write_text("private")
            installer.stage_server(Path(folder)/"revive")
            self.assertTrue((installer.SERVER/"Imlight.Director.dll").is_file())
            self.assertFalse((installer.SERVER/"Imlight.Director.exe").exists())
            self.assertTrue((installer.SERVER/"RavenDBServer/server").exists())

    def test_rejects_windows_runtime(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "SERVER", Path(folder)/"target"):
            source = Path(folder)/"revive/Imlight"
            source.mkdir(parents=True)
            (source/"Imlight.Director.deps.json").write_text(json.dumps({"runtimeTarget":{"name":".NETCoreApp,Version=v10.0/win-x64"}}))
            with self.assertRaisesRegex(ValueError, "Expected Linux"):
                installer.stage_server(Path(folder)/"revive")

    def test_generated_config_points_to_local_data_with_isolated_ports(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "RUNTIME", Path(folder)/"run"), \
              mock.patch.object(installer, "SERVER", Path(folder)/"run/server"), \
              mock.patch.object(ORIGINAL, "UPSTREAM", Path(folder)/"upstream"):
            config = ORIGINAL.UPSTREAM/"src/Imlight.Director/Config"
            config.mkdir(parents=True)
            keys = ("GameRevision", "LoginServerPort", "GameServerPort", "GameServerIP", "GameServerCount", "MaxGameServersAllowed",
                    "RealmNames", "PatchServerPort", "PatchServerInternalUrl", "EmbeddedDatabasePort", "EmbeddedDatabaseDataDirectory",
                    "EmbeddedDatabaseUseFull", "PlayerDatabaseUrl", "PlayerDatabaseCertificatePath", "SpiralDBLocalPath", "SpiralDBAutoFetch",
                    "SpiralDBDisableRemote", "LocalWadCachePath", "RandomizeCreatures", "RandomizeGateways", "RandomBackflips",
                    "StartingLevel", "TutorialDisabled", "DefaultItems", "LogLevel", "AdminAccountUsername", "AdminAccountPassword",
                    "SessionKeyHashInput")
            (config/"Imlight.ini").write_text("\n".join(k+" = placeholder" for k in keys))
            (config/"akka.conf").write_text("port = 12999")
            installer.configure_server(Path(folder)/"revive", "127.0.0.1")
            text = (installer.SERVER/"Config/Imlight.ini").read_text()
            self.assertIn("SpiralDBAutoFetch = false", text)
            self.assertIn("SpiralDBDisableRemote = true", text)
            self.assertIn("PatchServerPort = 12510", text)
            self.assertNotIn("compsci", text)
            self.assertEqual(stat.S_IMODE((installer.SERVER/"Config/Imlight.ini").stat().st_mode), 0o600)
            self.assertIn("13009", (installer.SERVER/"Config/akka.conf").read_text())

    def test_export_keeps_assets_private_and_local(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(installer, "RUNTIME", Path(folder)):
            base = Path(folder)/"patch"/installer.REVISION
            for name in ("LatestFileList.xml", "LatestFileList.bin", "Data/GameData/Root.wad"):
                (base/name).parent.mkdir(parents=True, exist_ok=True)
                (base/name).write_bytes(b"dummy-user-fixture")
            dest=Path(folder)/"private-assets.zip"
            installer.export_private_client_patch(dest)
            self.assertEqual(stat.S_IMODE(dest.stat().st_mode), 0o600)
            import zipfile
            with zipfile.ZipFile(dest) as z:
                self.assertEqual(len(z.namelist()), 3)
                self.assertTrue(all(x.startswith(installer.REVISION+"/") for x in z.namelist()))

    def test_non_loopback_requires_opt_in_even_in_check_mode(self):
        with self.assertRaises(SystemExit) as caught:
            installer.main(["--revive-path", "/tmp/nonexistent", "--game-ip", "192.0.2.23", "--check-only"])
        self.assertEqual(caught.exception.code, 2)


class LinuxControllerTests(unittest.TestCase):
    def test_stop_refuses_unowned_pid_file(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(ctl, "RUN", Path(folder)), \
             mock.patch.object(ctl, "owned_process", return_value=None):
            (Path(folder)/"server.pid").write_text("12345")
            with mock.patch.object(ctl.os, "kill") as kill:
                self.assertEqual(ctl.stop(), 2)
                kill.assert_not_called()

    def test_start_does_not_touch_other_services_without_build(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(ctl, "SERVER", Path(folder)):
            self.assertEqual(ctl.start(), 2)

    def test_status_reports_not_running(self):
        with mock.patch.object(ctl, "owned_process", return_value=None):
            self.assertEqual(ctl.status(), 0)


if __name__ == "__main__":
    unittest.main()
