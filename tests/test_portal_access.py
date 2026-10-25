"""Offline checks for grants, trusted-source HTTP flow and restricted UFW changes."""
import importlib.util
import json
import os
from pathlib import Path
import sqlite3
import tempfile
import threading
import unittest
from unittest import mock
from urllib.error import HTTPError
from urllib.request import Request, urlopen

import portal_api as api
from portal_access import AccessStore, LEASE_SECONDS, public_ipv4

spec = importlib.util.spec_from_file_location("access_firewall", Path(__file__).resolve().parents[1] / "portal/access_firewall.py")
fw = importlib.util.module_from_spec(spec)
spec.loader.exec_module(fw)


class GrantTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.store = AccessStore(Path(self.temp.name))

    def test_grant_replaces_ip_and_extends_expiry_without_access_keys(self):
        self.store.grant("new_player", "8.8.8.8", now=1000)
        self.store.grant("new_player", "1.1.1.1", now=2000)
        with self.store.connect() as db:
            self.assertEqual(db.execute("SELECT username, ip, expires, active FROM access_grants").fetchall(),
                             [("new_player", "1.1.1.1", 2000 + LEASE_SECONDS, 1)])
        self.assertFalse(hasattr(self.store, "refresh"))

    def test_legacy_schema_and_other_accounts_are_preserved(self):
        with self.store.connect() as db:
            db.execute("INSERT INTO access_grants VALUES (?, ?, ?, ?, ?, ?)",
                       ("old-key-hash", "new_player", "8.8.8.8", 10000, 10000, 1))
        self.store.grant("other_player", "8.8.8.8", now=1000)
        self.store.grant("new_player", "1.1.1.1", now=2000)
        with self.store.connect() as db:
            rows=db.execute("SELECT username, ip FROM access_grants ORDER BY username").fetchall()
            self.assertEqual(rows, [("new_player", "1.1.1.1"), ("other_player", "8.8.8.8")])
            self.assertEqual(db.execute("SELECT count(*) FROM access_grants WHERE key_hash = ?", ("old-key-hash",)).fetchone()[0], 0)

    def test_nonpublic_and_nonipv4_addresses_never_create_grants(self):
        for value in ("127.0.0.1", "192.168.1.1", "100.96.206.48", "224.0.0.1", "255.255.255.255", "::1", "2606:4700:4700::1111", "0.0.0.0/0", "8.8.8.8; reboot"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                public_ipv4(value)

    def test_export_snapshot_excludes_expired_and_rejects_excessive_expiry(self):
        self.assertEqual(fw.desired_ips([["8.8.8.8", 101], ["1.1.1.1", 100]], 100), {"8.8.8.8"})
        for rows in ([["0.0.0.0/0", 101]], [["8.8.8.8", 100 + LEASE_SECONDS + 61]], [["8.8.8.8", True]]):
            with self.assertRaises(ValueError):
                fw.desired_ips(rows, 100)

    def test_shared_address_remains_until_last_grant_moves(self):
        self.store.grant("first_player", "8.8.8.8", now=1000)
        self.store.grant("second_player", "8.8.8.8", now=1000)
        self.store.grant("first_player", "1.1.1.1", now=1001)
        with self.store.connect() as db:
            rows = [list(row) for row in db.execute("SELECT ip, expires FROM access_grants WHERE active = 1")]
        self.assertEqual(fw.desired_ips(rows, 1001), {"8.8.8.8", "1.1.1.1"})

    def test_ip_grants_require_public_mode(self):
        with mock.patch.dict(os.environ, {"SPIRAL_IP_ACCESS": "1", "SPIRAL_PORTAL_PUBLIC": "0"}):
            with self.assertRaisesRegex(ValueError, "public mode"):
                api.Settings()


class FirewallTests(unittest.TestCase):
    def test_reconcile_changes_only_its_profile_and_preserves_shared_ip(self):
        existing = "\n".join([
            "ufw allow OpenSSH", "ufw allow from 100.96.206.48 to any port 12379 proto tcp",
            "ufw " + " ".join(fw.rule("8.8.8.8")),
            "ufw " + " ".join(fw.rule("9.9.9.9")),
        ])
        command = mock.Mock(side_effect=lambda *args: (
            "Status: active\nDefault: deny (incoming), allow (outgoing)\n" if args == ("status", "verbose")
            else existing if args == ("show", "added") else ""))
        fw.reconcile({"9.9.9.9", "1.1.1.1"}, command)
        self.assertEqual(command.call_args_list[2:], [
            mock.call("--force", "delete", *fw.rule("8.8.8.8")), mock.call(*fw.rule("1.1.1.1"))])
        # Second run has no mutations; shared active addresses are kept once.
        command.reset_mock()
        command.side_effect = lambda *args: (
            "Status: active\nDefault: deny (incoming)" if args == ("status", "verbose")
            else "ufw " + " ".join(fw.rule("9.9.9.9")))
        fw.reconcile({"9.9.9.9"}, command)
        self.assertEqual(command.call_count, 2)

    def test_unmanaged_profile_or_inactive_firewall_aborts_before_mutation(self):
        for status, added in [
            ("Status: inactive", ""),
            ("Status: active\nDefault: allow (incoming)", ""),
            ("Status: active\nDefault: deny (incoming)", "ufw allow from 8.8.8.8 to any app SpiralShuffleRegistered"),
        ]:
            command = mock.Mock(side_effect=lambda *args: status if args == ("status", "verbose") else added)
            with self.assertRaises(ValueError):
                fw.reconcile({"1.1.1.1"}, command)
            self.assertLessEqual(command.call_count, 2)

    def test_profile_contains_only_required_tcp_ports(self):
        profile = Path(__file__).resolve().parents[1] / "portal/spiral-shuffle-registered"
        self.assertEqual(profile.read_text(), fw.PROFILE)
        self.assertIn("ports=12010,12343:12348,12510,12379/tcp", fw.PROFILE)


class AccessHttpTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.store = AccessStore(Path(self.temp.name))
        api.Handler.settings = type("Fake", (), dict(
            public=True, access_enabled=True, access_store=self.store,
            origin="https://spiral.hypercoloss.us", ip_salt="test", bridge_token="a" * 64))()
        self.server = api.ThreadingHTTPServer(("127.0.0.1", 0), api.Handler)
        self.thread = threading.Thread(target=self.server.serve_forever, daemon=True)
        self.thread.start()
        self.addCleanup(self.close_server)
        for name, value in (("check_rate_limit", True), ("verify_turnstile", True),
                            ("create_game_account", (201, "Created."))):
            patch = mock.patch.object(api, name, return_value=value)
            setattr(self, name, patch.start())
            self.addCleanup(patch.stop)

    def close_server(self):
        self.server.shutdown()
        self.server.server_close()
        self.thread.join(timeout=2)

    def post(self, path, body, ip="8.8.8.8", origin="https://spiral.hypercoloss.us"):
        headers = {"Content-Type": "application/json", "Origin": origin}
        if ip is not None:
            headers["X-Real-IP"] = ip
        request = Request(f"http://127.0.0.1:{self.server.server_port}{path}",
                          data=json.dumps(body).encode(), headers=headers)
        try:
            response = urlopen(request, timeout=3)
        except HTTPError as error:
            response = error
        with response:
            return response.status, json.load(response)

    def signup(self):
        return self.post("/register", dict(username="new_player", password="Example-Private-Password", turnstileToken="proof"))

    def test_create_or_verify_credentials_uses_same_form_and_proxy_ip(self):
        status, result = self.signup()
        self.assertEqual(status, 201)
        self.assertNotIn("password", result)
        self.assertNotIn("accessKey", result)
        self.create_game_account.return_value = (200, "Verified.")
        status, _ = self.post("/register", {"username":"new_player", "password":"Example-Private-Password", "ip":"9.9.9.9", "turnstileToken":"proof"}, ip="1.1.1.1")
        self.assertEqual(status, 200)
        with self.store.connect() as db:
            self.assertEqual(db.execute("SELECT ip, active FROM access_grants").fetchall(), [("1.1.1.1", 1)])
        self.create_game_account.assert_called_with(api.Handler.settings, "new_player", "Example-Private-Password", "1.1.1.1")

    def test_wrong_password_or_backend_failure_cannot_grant(self):
        for code in (403, 503):
            self.create_game_account.return_value = (code, "Unavailable.")
            self.assertEqual(self.signup()[0], code)
        with self.store.connect() as db:
            self.assertEqual(db.execute("SELECT count(*) FROM access_grants").fetchone()[0], 0)

    def test_wrong_origin_missing_proxy_captcha_and_rate_limit_cannot_grant(self):
        self.assertEqual(self.post("/register", {}, ip=None)[0], 403)
        self.assertEqual(self.post("/register", {}, origin="https://evil.example")[0], 403)
        self.verify_turnstile.return_value = False
        self.assertEqual(self.signup()[0], 403)
        self.verify_turnstile.return_value = True
        self.check_rate_limit.return_value = False
        self.assertEqual(self.signup()[0], 429)
        self.create_game_account.assert_not_called()

    def test_storage_failure_does_not_create_account(self):
        with mock.patch.object(self.store, "check_ready", side_effect=sqlite3.OperationalError):
            self.assertEqual(self.signup()[0], 503)
        self.create_game_account.assert_not_called()

    def test_save_failure_can_be_retried_with_existing_credentials(self):
        with mock.patch.object(self.store, "grant", side_effect=sqlite3.OperationalError):
            status, result = self.signup()
        self.assertEqual(status, 503)
        self.assertNotIn("accessKey", result)
        self.create_game_account.return_value=(200, "Verified.")
        self.assertEqual(self.signup()[0], 200)
        with self.store.connect() as db:
            self.assertEqual(db.execute("SELECT count(*) FROM access_grants WHERE active = 1").fetchone()[0], 1)

    def test_legacy_access_keys_are_not_accepted(self):
        self.assertEqual(self.post("/access/refresh", {"accessKey":"x"*43})[0], 404)
        self.assertEqual(self.post("/register", {"accessKey":"x"*43})[0], 400)
        self.create_game_account.assert_not_called()

    def test_refresh_requires_correct_credentials_and_captcha(self):
        self.signup()
        self.create_game_account.return_value=(403, "Wrong password.")
        self.assertEqual(self.post("/register", {"username":"new_player", "password":"wrong-password", "turnstileToken":"proof"}, ip="1.1.1.1")[0], 403)
        self.verify_turnstile.return_value=False
        self.check_rate_limit.reset_mock()
        self.assertEqual(self.signup()[0], 403)
        self.check_rate_limit.assert_called_once_with("8.8.8.8", "test")
        with self.store.connect() as db:
            self.assertEqual(db.execute("SELECT ip FROM access_grants").fetchone()[0], "8.8.8.8")

    def test_existing_short_password_is_forwarded_for_native_verification(self):
        self.create_game_account.return_value=(200, "Verified.")
        status, _ = self.post("/register", {"username":"old_player", "password":"legacy", "turnstileToken":"proof"})
        self.assertEqual(status, 200)

    def test_account_attempt_budget_is_enforced_after_captcha(self):
        self.check_rate_limit.side_effect=[True, False]
        self.assertEqual(self.signup()[0], 429)
        self.create_game_account.assert_not_called()


if __name__ == "__main__":
    unittest.main()
