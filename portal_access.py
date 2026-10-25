"""Private IP grants, written only after native game credentials are accepted."""
from __future__ import annotations

from contextlib import contextmanager
import hashlib
import ipaddress
from pathlib import Path
import re
import sqlite3
import time

LEASE_SECONDS = 30 * 86400


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
            # Keep the installed schema and existing grants. Legacy key columns
            # are internal identifiers now; no access keys are accepted.
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

    def check_ready(self):
        with self.connect() as db:
            db.execute("BEGIN IMMEDIATE")
            db.execute("UPDATE access_grants SET active = active WHERE 0")

    def grant(self, username: str, ip: str, now: int | None = None):
        if not isinstance(username, str) or not re.fullmatch(r"[a-z][a-z0-9_]{2,19}", username, re.ASCII):
            raise ValueError("Invalid account name")
        ip = public_ipv4(ip)
        now = int(time.time()) if now is None else now
        identifier = "credentials:" + hashlib.sha256(username.encode("ascii")).hexdigest()
        with self.connect() as db:
            db.execute("BEGIN IMMEDIATE")
            # Consolidate this verified account's old grants atomically, keeping
            # other accounts that share the previous address.
            db.execute("DELETE FROM access_grants WHERE username = ? AND key_hash <> ?", (username, identifier))
            db.execute("""INSERT INTO access_grants VALUES (?, ?, ?, ?, ?, 1)
                ON CONFLICT(key_hash) DO UPDATE SET ip = excluded.ip,
                    expires = excluded.expires, key_expires = excluded.key_expires, active = 1""",
                (identifier, username, ip, now + LEASE_SECONDS, now + LEASE_SECONDS))
