"""Offline regression checks for the GitHub Pages account API."""
from __future__ import annotations

import json
import io
import os
from pathlib import Path
import tempfile
import threading
import unittest
from unittest import mock
from urllib.error import HTTPError
from urllib.request import Request, urlopen
from urllib.parse import parse_qs

import portal_api as api


class RegistrationApiTests(unittest.TestCase):
    def test_turnstile_stays_enforced_when_home_peer_omits_optional_remoteip(self):
        settings = type("Fake", (), {"public":True, "turnstile_secret":"test-secret", "host":"spiral.hypercoloss.us"})()
        for ip in (None, "8.8.8.8"):
            body=json.dumps({"success":True,"hostname":"spiral.hypercoloss.us","action":"register"}).encode()
            with mock.patch.object(api, "urlopen", return_value=io.BytesIO(body)) as verify:
                self.assertTrue(api.verify_turnstile(settings, "proof", ip))
                payload=parse_qs(verify.call_args.args[0].data.decode())
                self.assertEqual(payload["response"], ["proof"])
                self.assertEqual(payload.get("remoteip"), [ip] if ip else None)
        self.assertFalse(api.verify_turnstile(settings, "", None))

    def test_bridge_client_accepts_created_and_verified_accounts(self):
        settings = type("Fake", (), {"bridge_token": "a" * 64})()
        for status in (200, 201):
            response = mock.MagicMock()
            response.__enter__.return_value.status = status
            with mock.patch.object(api, "urlopen", return_value=response) as send:
                result, _ = api.create_game_account(settings, "player_one", "ExamplePassword123!", "8.8.8.8")
            self.assertEqual(result, status)
            request = send.call_args.args[0]
            self.assertEqual(request.full_url, "http://127.0.0.1:12772/enter")
            self.assertEqual(json.loads(request.data), {"Username":"player_one", "Password":"ExamplePassword123!", "ClientIp":"8.8.8.8"})

    def test_bridge_client_wrong_password_and_unavailable_versions_fail_closed(self):
        settings = type("Fake", (), {"bridge_token": "a" * 64})()
        for bridge_status, expected in ((403, 403), (404, 503), (500, 503)):
            with mock.patch.object(api, "urlopen", side_effect=HTTPError(api.BRIDGE_URL, bridge_status, "Error", {}, None)):
                status, _ = api.create_game_account(settings, "player_one", "ExamplePassword123!", "8.8.8.8")
                self.assertEqual(status, expected)

    def test_actionable_username_and_password_validation(self):
        cases = [
            ("", "valid-password", "Enter a username."),
            ("ab", "valid-password", "between 3 and 20"),
            ("1player", "valid-password", "start with a letter"),
            ("my-player", "valid-password", "only letters, numbers and underscores"),
            ("admin", "valid-password", "reserved"),
            ("player_one", "", "Enter your password."),
            ("player_one", "x" * 33, "longer than 32"),
            ("player_one", "has spaces", "cannot contain spaces"),
        ]
        for username, password, expected in cases:
            with self.subTest(username=username):
                self.assertIn(expected, api.credential_error(username, password))
        self.assertIsNone(api.credential_error("player_one", "legacy"))

    def test_bridge_reason_codes_produce_safe_specific_messages(self):
        settings = type("Fake", (), {"bridge_token": "a" * 64})()
        for reason, (status, message) in api.BRIDGE_REJECTIONS.items():
            body = io.BytesIO(json.dumps({"code":reason,"message":"PRIVATE INTERNAL DETAIL"}).encode())
            with mock.patch.object(api, "urlopen", side_effect=HTTPError(api.BRIDGE_URL,status,"Error",{},body)):
                actual, detail = api.create_game_account(settings,"player_one","ExamplePassword123!","8.8.8.8")
            self.assertEqual(actual, 403 if status == 409 else status)
            self.assertEqual(detail, message)
            self.assertNotIn("PRIVATE INTERNAL DETAIL", detail)

    def test_unknown_bridge_errors_do_not_expose_raw_details(self):
        settings = type("Fake", (), {"bridge_token":"a" * 64})()
        for code in ("bridge_forbidden", "unknown_private_error", ["wrong_password"]):
            body=io.BytesIO(json.dumps({"code":code,"message":"PRIVATE INTERNAL DETAIL"}).encode())
            with mock.patch.object(api,"urlopen",side_effect=HTTPError(api.BRIDGE_URL,403,"Error",{},body)):
                status, detail=api.create_game_account(settings,"player_one","ExamplePassword123!","8.8.8.8")
            self.assertEqual(status,503 if code == "bridge_forbidden" else 403)
            self.assertNotIn("PRIVATE INTERNAL DETAIL",detail)

    def test_username_and_password_policy(self):
        self.assertTrue(api.USER_RULE.fullmatch("player_one"))
        self.assertFalse(api.USER_RULE.fullmatch("Admin"))
        self.assertFalse(api.USER_RULE.fullmatch("123player"))
        self.assertFalse(api.USER_RULE.fullmatch("too_long_username_for_a_game"))
        self.assertTrue(api.PASS_RULE.fullmatch("mY-UniquePass_2026"))
        self.assertTrue(api.PASS_RULE.fullmatch("1"))
        self.assertFalse(api.PASS_RULE.fullmatch(""))
        self.assertFalse(api.PASS_RULE.fullmatch("x" * 33))
        self.assertFalse(api.PASS_RULE.fullmatch("with spaces 123"))
        self.assertIn("admin", api.RESERVED)

    def test_rate_limit_is_private_and_persistent(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(api, "PRIVATE", Path(folder)):
            for i in range(5):
                self.assertTrue(api.check_rate_limit("198.51.100.9", "private-salt", now=10_000 + i))
            self.assertFalse(api.check_rate_limit("198.51.100.9", "private-salt", now=10_010))
            self.assertTrue(api.check_rate_limit("198.51.100.10", "private-salt", now=10_010))
            records = (Path(folder) / "rate-limits.sqlite3").read_bytes()
            self.assertNotIn(b"198.51.100.9", records)

    def test_settings_fail_closed_without_private_bridge_token(self):
        with tempfile.TemporaryDirectory() as folder, mock.patch.object(api, "PRIVATE", Path(folder)):
            with mock.patch.dict(os.environ, {"SPIRAL_PORTAL_PUBLIC": "0"}, clear=True):
                with self.assertRaisesRegex(ValueError, "portal_bridge.py init/install"):
                    api.Settings()

    def test_settings_require_captcha_in_public_mode(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            private = root / ".runtime/portal"
            private.mkdir(parents=True)
            (private / "bridge-token").write_text("a" * 64)
            ini = root / ".runtime/server/Config/Imlight.ini"
            ini.parent.mkdir(parents=True)
            ini.write_text("AdminAccountPassword = AnExampleLongPrivatePassword\n")
            with mock.patch.object(api, "ROOT", root), mock.patch.object(api, "PRIVATE", private):
                with mock.patch.dict(os.environ, {
                    "SPIRAL_PORTAL_PUBLIC": "1",
                    "SPIRAL_ALLOWED_ORIGIN": "https://spiral.hypercoloss.us",
                    "SPIRAL_TURNSTILE_SECRET": "",
                }, clear=True):
                    with self.assertRaisesRegex(ValueError, "TURNSTILE_SECRET"):
                        api.Settings()

    def test_browser_preflight_allows_only_github_pages_origin(self):
        settings = type("Fake", (), {
            "origin": "https://spiral.hypercoloss.us", "public": False,
            "ip_salt": "demo", "bridge_token": "x" * 64,
        })()
        api.Handler.settings = settings
        server = api.ThreadingHTTPServer(("127.0.0.1", 0), api.Handler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            url = f"http://127.0.0.1:{server.server_port}/register"
            req = Request(url, method="OPTIONS", headers={
                "Origin": "https://spiral.hypercoloss.us",
                "Access-Control-Request-Headers": "content-type",
            })
            with urlopen(req, timeout=2) as resp:
                self.assertEqual(resp.status, 204)
                self.assertEqual(resp.headers["Access-Control-Allow-Origin"], settings.origin)
            bad = Request(url, method="OPTIONS", headers={
                "Origin": "https://attacker.example",
                "Access-Control-Request-Headers": "content-type",
            })
            with self.assertRaises(HTTPError) as ctx:
                urlopen(bad, timeout=2)
            self.assertEqual(ctx.exception.code, 403)
            self.assertIsNone(ctx.exception.headers.get("Access-Control-Allow-Origin"))
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)

    def test_registration_does_not_return_a_password(self):
        settings = type("Fake", (), {
            "origin": "https://spiral.hypercoloss.us", "public": False,
            "ip_salt": "demo", "bridge_token": "x" * 64,
        })()
        api.Handler.settings = settings
        server = api.ThreadingHTTPServer(("127.0.0.1", 0), api.Handler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            url = f"http://127.0.0.1:{server.server_port}/register"
            payload = json.dumps({
                "username": "test_player", "password": "EXAMPLE_long_PASSWORD_123",
                "turnstileToken": ""
            }).encode()
            with mock.patch.object(api, "check_rate_limit", return_value=True), \
                 mock.patch.object(api, "create_game_account", return_value=(201, "Account created.")):
                req = Request(url, method="POST", data=payload, headers={
                    "Origin": settings.origin, "Content-Type": "application/json",
                })
                with urlopen(req, timeout=2) as resp:
                    result = json.load(resp)
                    self.assertEqual(resp.status, 201)
                    self.assertEqual(result, {"message": "Account created."})
                    self.assertNotIn("password", str(result).lower())
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)


if __name__ == "__main__":
    unittest.main()
