"""Private Spiral Shuffle registration API, intended for HTTPS reverse proxy.

Only binds 127.0.0.1:12771 and only talks to Imlight's loopback-only bridge.
GitHub Pages is the public frontend; this server never serves game assets.
"""
from __future__ import annotations

import hashlib
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import ipaddress
import json
import os
from pathlib import Path
import re
import secrets
import sqlite3
import time
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urlsplit
from urllib.request import Request, urlopen

from portal_access import AccessStore, public_ipv4

ROOT = Path(__file__).resolve().parent
PRIVATE = ROOT / ".runtime" / "portal"
BRIDGE_URL = "http://127.0.0.1:12772/enter"
LISTEN_ADDRESS = ("127.0.0.1", 12771)
USER_RULE = re.compile(r"^[a-z][a-z0-9_]{2,19}$", re.ASCII)
PASS_RULE = re.compile(r"^[!-~]{12,32}$", re.ASCII)
LOGIN_PASS_RULE = re.compile(r"^[!-~]{1,32}$", re.ASCII)
RESERVED = frozenset(("admin", "administrator", "moderator", "support",
                      "system", "staff", "gamemaster"))


BRIDGE_REJECTIONS = {
    "username_format": (400, "Username must be 3–20 characters, start with a letter, and contain only letters, numbers or underscores."),
    "username_reserved": (400, "That username is reserved for server staff. Choose a different username."),
    "password_format": (400, "Passwords can contain at most 32 printable ASCII characters, with no spaces or other whitespace."),
    "new_password_requirements": (400, "This username is new. Choose a password with 12–32 characters, without spaces. Existing accounts should use their current game password."),
    "wrong_password": (403, "That username already exists, but the password is incorrect. Passwords are case-sensitive; check Caps Lock. To create a new account, choose a different username."),
    "account_locked": (403, "Your password is correct, but this account is locked. Contact the server owner to restore access."),
    "account_banned": (403, "Your password is correct, but this account is banned. Contact the server owner if you believe this is a mistake."),
    "network_blocked": (403, "This network is blocked by the game server. Contact the server owner for help."),
    "username_taken": (409, "That username already exists. Use its current password, or choose a different username for a new account."),
}


def credential_error(username: str, password: str) -> str | None:
    if not username:
        return "Enter a username."
    if not 3 <= len(username) <= 20:
        return "Username must be between 3 and 20 characters."
    if not re.match(r"^[a-z]", username, re.ASCII):
        return "Username must start with a letter."
    if not USER_RULE.fullmatch(username):
        return "Username can contain only letters, numbers and underscores."
    if username in RESERVED:
        return BRIDGE_REJECTIONS["username_reserved"][1]
    if not password:
        return "Enter your password."
    if len(password) > 32:
        return "Password cannot be longer than 32 characters."
    if not LOGIN_PASS_RULE.fullmatch(password):
        return "Password cannot contain spaces, whitespace or non-ASCII characters."
    return None


def local_signup_peers(value: str) -> frozenset[str]:
    """Explicit private connection peers only; never a public-IP substitution."""
    private_ranges = tuple(map(ipaddress.ip_network, ("10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16")))
    peers = set()
    for item in value.split(","):
        if not item.strip():
            continue
        address = ipaddress.ip_address(item.strip())
        if address.version != 4 or not any(address in network for network in private_ranges):
            raise ValueError("SPIRAL_LOCAL_SIGNUP_PEERS requires exact private LAN IPv4 addresses.")
        peers.add(str(address))
    return frozenset(peers)


class Settings:
    def __init__(self):
        self.public = os.getenv("SPIRAL_PORTAL_PUBLIC", "0") == "1"
        self.access_enabled = os.getenv("SPIRAL_IP_ACCESS", "0") == "1"
        self.local_signup_peers = local_signup_peers(os.getenv("SPIRAL_LOCAL_SIGNUP_PEERS", ""))
        if self.access_enabled and not self.public:
            raise ValueError("IP access requires public mode and a trusted HTTPS proxy.")
        self.access_store = AccessStore(PRIVATE) if self.access_enabled else None
        self.origin = os.getenv("SPIRAL_ALLOWED_ORIGIN", "https://spiral.hypercoloss.us").rstrip("/")
        url = urlsplit(self.origin)
        if (url.scheme not in ("https", "http") or not url.hostname or
                url.path or url.query or url.fragment or url.username or url.password):
            raise ValueError("SPIRAL_ALLOWED_ORIGIN must be an exact browser origin without a path.")
        if self.public and (url.scheme != "https" or self.origin != "https://spiral.hypercoloss.us"):
            raise ValueError("Public mode requires the configured HTTPS GitHub Pages origin.")
        self.turnstile_secret = os.getenv("SPIRAL_TURNSTILE_SECRET", "")
        self.host = url.hostname
        if self.public and not self.turnstile_secret:
            raise ValueError("Public registration requires SPIRAL_TURNSTILE_SECRET.")
        ini = ROOT / ".runtime/server/Config/Imlight.ini"
        if self.public:
            if not ini.is_file():
                raise ValueError("Imlight config missing: cannot open public registration.")
            match = re.findall(r"(?m)^AdminAccountPassword\s*=\s*(.*?)\s*$", ini.read_text())
            if len(match) != 1 or len(match[0]) < 12 or match[0] in ("compsci", "123"):
                raise ValueError("Change temporary admin password in Imlight first.")
        token_path = PRIVATE / "bridge-token"
        if not token_path.is_file():
            raise ValueError("Run python3 portal_bridge.py init/install first.")
        self.bridge_token = token_path.read_text().strip()
        if not re.fullmatch(r"[0-9a-fA-F]{64}", self.bridge_token):
            raise ValueError("Invalid bridge token.")
        salt_path = PRIVATE / "ip-salt"
        if not salt_path.exists():
            fd = os.open(salt_path, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
            with os.fdopen(fd, "w") as stream:
                stream.write(secrets.token_hex(32) + "\n")
        salt_path.chmod(0o600)
        self.ip_salt = salt_path.read_text().strip()


def check_rate_limit(ip: str, salt: str, now: int | None = None) -> bool:
    """At most 5 attempts/hour or 15/day per salted IP fingerprint."""
    now = int(time.time()) if now is None else now
    ip_hash = hashlib.sha256((salt + ":" + ip).encode("utf-8")).hexdigest()
    connection = sqlite3.connect(PRIVATE / "rate-limits.sqlite3", timeout=5)
    try:
        connection.execute("CREATE TABLE IF NOT EXISTS attempts (ip_hash TEXT, at INTEGER)")
        connection.execute("CREATE INDEX IF NOT EXISTS attempts_by_ip ON attempts(ip_hash, at)")
        (PRIVATE / "rate-limits.sqlite3").chmod(0o600)
        connection.execute("BEGIN IMMEDIATE")
        connection.execute("DELETE FROM attempts WHERE at < ?", (now - 86400,))
        hourly = connection.execute(
            "SELECT count(*) FROM attempts WHERE ip_hash = ? AND at >= ?",
            (ip_hash, now - 3600)).fetchone()[0]
        daily = connection.execute(
            "SELECT count(*) FROM attempts WHERE ip_hash = ? AND at >= ?",
            (ip_hash, now - 86400)).fetchone()[0]
        if hourly >= 5 or daily >= 15:
            connection.commit()
            return False
        connection.execute("INSERT INTO attempts VALUES (?, ?)", (ip_hash, now))
        connection.commit()
        return True
    finally:
        connection.close()


def verify_turnstile(settings: Settings, token: str, ip: str | None) -> bool:
    if not settings.public:
        return True
    if not token or len(token) > 2048:
        return False
    fields = {
        "secret": settings.turnstile_secret,
        "response": token,
    }
    if ip is not None:
        fields["remoteip"] = ip
    payload = urlencode(fields).encode("utf-8")
    request = Request(
        "https://challenges.cloudflare.com/turnstile/v0/siteverify",
        data=payload, method="POST",
        headers={"Content-Type": "application/x-www-form-urlencoded"})
    try:
        with urlopen(request, timeout=8) as response:
            result = json.load(response)
        return (result.get("success") is True
                and result.get("hostname") == settings.host
                and result.get("action") == "register")
    except (URLError, ValueError, TimeoutError, OSError):
        return False


def create_game_account(settings: Settings, username: str, password: str, ip: str) -> tuple[int, str]:
    """Create a game account or verify existing credentials using Imlight."""
    body = json.dumps({"Username": username, "Password": password, "ClientIp": ip}).encode("utf-8")
    request = Request(
        BRIDGE_URL, data=body, method="POST",
        headers={"Content-Type": "application/json",
                 "X-Spiral-Token": settings.bridge_token})
    try:
        with urlopen(request, timeout=20) as response:
            if response.status == 201:
                return 201, "Account created. You can now sign in to Spiral Shuffle."
            if response.status == 200:
                return 200, "Game credentials verified. You can now sign in to Spiral Shuffle."
            return 503, "Registration temporarily unavailable."
    except HTTPError as exc:
        # Translate only documented reason codes, never arbitrary bridge text,
        # exceptions, account data or private authentication failures.
        try:
            detail = json.loads(exc.read(4096))
            reason = detail.get("code") if isinstance(detail, dict) else None
        except (ValueError, TypeError, OSError, AttributeError):
            reason = None
        known = BRIDGE_REJECTIONS.get(reason) if isinstance(reason, str) else None
        if known and known[0] == exc.code:
            return (403 if exc.code == 409 else exc.code), known[1]
        if reason in ("bridge_forbidden", "account_unavailable"):
            return 503, "The game account service is temporarily unavailable. Try again later."
        if exc.code in (401, 403, 409):
            return 403, "Unable to verify these game credentials. Check your username and password."
        if exc.code == 400:
            return 400, "New accounts need a 12–32 character password without spaces. If this is an existing account, use its current game password."
        return 503, "Registration temporarily unavailable."
    except (URLError, TimeoutError, OSError, ValueError):
        return 503, "Game account creation is unavailable right now."


class Handler(BaseHTTPRequestHandler):
    settings: Settings

    def log_message(self, fmt, *args):
        # Do not log request body, password, headers, IP or username.
        print("portal-api: request handled", flush=True)

    def json_reply(self, status: int, message: str, success: bool = False, **extra):
        payload = json.dumps({("message" if success else "error"): message, **extra}).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(payload)))
        self.send_header("Cache-Control", "no-store")
        self.send_header("X-Content-Type-Options", "nosniff")
        self.send_header("Referrer-Policy", "no-referrer")
        if self.headers.get("Origin") == self.settings.origin:
            self.send_header("Access-Control-Allow-Origin", self.settings.origin)
            self.send_header("Vary", "Origin")
        self.end_headers()
        self.wfile.write(payload)

    def do_GET(self):
        if self.path == "/health":
            self.json_reply(200, "Registration API is listening.", True)
        else:
            self.json_reply(404, "Not found.")

    def do_OPTIONS(self):
        if self.path != "/register" or self.headers.get("Origin") != self.settings.origin:
            self.json_reply(403, "Forbidden.")
            return
        if "content-type" not in self.headers.get("Access-Control-Request-Headers", "").lower():
            self.json_reply(403, "Forbidden.")
            return
        self.send_response(204)
        self.send_header("Access-Control-Allow-Origin", self.settings.origin)
        self.send_header("Vary", "Origin")
        self.send_header("Access-Control-Allow-Methods", "POST, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.send_header("Access-Control-Max-Age", "600")
        self.send_header("Content-Length", "0")
        self.end_headers()

    def client_ip(self) -> str | None:
        # When public, trust only the X-Real-IP that the LOCAL Caddy proxy
        # overwrites with {remote_host}. Listener itself is loopback-only.
        raw = (self.headers.get("X-Real-IP", "") if self.settings.public
               else self.client_address[0])
        try:
            return str(ipaddress.ip_address(raw))
        except ValueError:
            return None

    def do_POST(self):
        access_enabled = getattr(self.settings, "access_enabled", False)
        if self.path != "/register":
            self.json_reply(404, "Not found.")
            return
        if self.headers.get("Origin") != self.settings.origin:
            self.json_reply(403, "Invalid website origin.")
            return
        if self.headers.get("Content-Type", "").split(";")[0].strip().lower() != "application/json":
            self.json_reply(415, "Expected JSON registration.")
            return
        try:
            size = int(self.headers.get("Content-Length", "-1"))
        except ValueError:
            size = -1
        if size < 2 or size > 4096:
            self.json_reply(413, "Invalid request size.")
            return
        ip = self.client_ip()
        if ip is None:
            self.json_reply(403, "Registration proxy configuration invalid.")
            return
        local_peer = access_enabled and ip in getattr(self.settings, "local_signup_peers", frozenset())
        if access_enabled and not local_peer:
            try:
                ip = public_ipv4(ip)
            except ValueError:
                self.json_reply(400, "Use a public IPv4 connection to register or refresh game access.")
                return
        try:
            data = json.loads(self.rfile.read(size))
            if not isinstance(data, dict):
                raise ValueError("Expected object.")
            username = data.get("username", "")
            password = data.get("password", "")
            proof = data.get("turnstileToken", "")
            if not isinstance(username, str) or not isinstance(password, str) or not isinstance(proof, str):
                raise ValueError("Invalid field types.")
            username = username.strip().lower()
        except (ValueError, UnicodeError, TypeError):
            self.json_reply(400, "Invalid registration data.")
            return
        rejection = credential_error(username, password)
        if rejection:
            self.json_reply(400, rejection)
            return
        try:
            allowed = check_rate_limit(ip, self.settings.ip_salt)
        except sqlite3.Error:
            self.json_reply(503, "Registration temporarily unavailable.")
            return
        if not allowed:
            self.json_reply(429, "Too many attempts; try again later.")
            return
        if not verify_turnstile(self.settings, proof, None if local_peer else ip):
            self.json_reply(403, "Bot verification failed. Refresh and try again.")
            return
        try:
            # Charge the per-account budget only after bot verification, so an
            # unauthenticated script cannot exhaust another account's budget.
            account_allowed = check_rate_limit("account:" + username, self.settings.ip_salt)
        except sqlite3.Error:
            self.json_reply(503, "Game access temporarily unavailable.")
            return
        if not account_allowed:
            self.json_reply(429, "Too many attempts; try again later.")
            return
        if access_enabled and not local_peer:
            try:
                self.settings.access_store.check_ready()
            except (sqlite3.Error, OSError, ValueError):
                self.json_reply(503, "Game access storage temporarily unavailable. Try again later.")
                return
        status, message = create_game_account(self.settings, username, password, ip)
        if local_peer and status in (200, 201):
            # Private peers still require CAPTCHA and native game credentials.
            # Never insert a router/private address into the public allowlist or
            # overwrite an existing public grant for this account.
            message = "Game credentials accepted. Use these credentials in the launcher."
        elif access_enabled and status in (200, 201):
            try:
                self.settings.access_store.grant(username, ip)
            except (sqlite3.Error, OSError):
                self.json_reply(503, "Game credentials accepted, but network access could not be saved. Retry with the same username and password.")
                return
            message = "Game access queued. Allow up to one minute, then use this username and password in the launcher. New accounts are created automatically."
        self.json_reply(status, message, success=(status in (200, 201)))


def main():
    settings = Settings()
    Handler.settings = settings
    server = ThreadingHTTPServer(LISTEN_ADDRESS, Handler)
    server.daemon_threads = True
    print("Spiral Shuffle registration API: http://127.0.0.1:12771 (LOCAL ONLY)", flush=True)
    print("Public validation enforced:", settings.public, flush=True)
    server.serve_forever()


if __name__ == "__main__":
    main()
