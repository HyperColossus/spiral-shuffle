"""Private, expiring website network grants. Never executes firewall commands."""
from __future__ import annotations

import hashlib
from contextlib import contextmanager
import ipaddress
from pathlib import Path
import re
import secrets
import sqlite3
import time

LEASE_SECONDS = 30 * 86400
KEY_SECONDS = 90 * 86400
KEY_RULE = re.compile(r"^[A-Za-z0-9_-]{43}$", re.ASCII)


def public_ipv4(value: str) -> str:
    address = ipaddress.ip_address(value)
    if address.version != 4 or not address.is_global or address.is_multicast:
        raise ValueError("A public IPv4 connection is required for game access.")
    return str(address)


class AccessStore:
    def __init__(self, directory: Path):
        self.path = directory / "access.sqlite3"
        directory.mkdir(mode=0o700, parents=True, exist_ok=True)
        directory.chmod(0o700)
        with self.connect() as db:
            db.execute("""CREATE TABLE IF NOT EXISTS access_grants (
                key_hash TEXT PRIMARY KEY, username TEXT NOT NULL, ip TEXT NOT NULL,
                expires INTEGER NOT NULL, key_expires INTEGER NOT NULL,
                active INTEGER NOT NULL DEFAULT 0)""")
        self.path.chmod(0o600)

    @contextmanager
    def connect(self):
        db = sqlite3.connect(self.path, timeout=5)
        try:
            with db:
                yield db
        finally:
            db.close()

    def reserve(self, username: str, ip: str, now: int | None = None) -> str:
        if not isinstance(username, str) or not re.fullmatch(r"[a-z][a-z0-9_]{2,19}", username, re.ASCII):
            raise ValueError("Invalid account name")
        ip = public_ipv4(ip)
        now = int(time.time()) if now is None else now
        key = secrets.token_urlsafe(32)
        with self.connect() as db:
            db.execute("DELETE FROM access_grants WHERE key_expires <= ? AND expires <= ?", (now, now))
            db.execute("INSERT INTO access_grants VALUES (?, ?, ?, ?, ?, 0)",
                       (self.digest(key), username, ip, now + LEASE_SECONDS, now + KEY_SECONDS))
        return key

    @staticmethod
    def digest(key: str) -> str:
        return hashlib.sha256(key.encode("ascii")).hexdigest()

    def activate(self, key: str):
        with self.connect() as db:
            db.execute("UPDATE access_grants SET active = 1 WHERE key_hash = ?",
                       (self.digest(key),))

    def cancel(self, key: str):
        with self.connect() as db:
            db.execute("DELETE FROM access_grants WHERE key_hash = ? AND active = 0",
                       (self.digest(key),))

    def refresh(self, key: str, ip: str, now: int | None = None) -> bool:
        if not isinstance(key, str) or not KEY_RULE.fullmatch(key):
            return False
        ip = public_ipv4(ip)
        now = int(time.time()) if now is None else now
        with self.connect() as db:
            changed = db.execute("""UPDATE access_grants SET ip = ?, expires = ?
                WHERE key_hash = ? AND active = 1 AND key_expires > ?""",
                (ip, now + LEASE_SECONDS, self.digest(key), now)).rowcount
        return changed == 1
