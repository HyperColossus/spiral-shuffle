"""Root-owned UFW synchronizer; SQLite is read in an unprivileged subprocess.

Install this file outside the website-writable checkout. No sudo access is
granted to the API. Only a dedicated UFW application profile is managed.
"""
from __future__ import annotations

import argparse
import ipaddress
import json
import os
from pathlib import Path
import shlex
import sqlite3
import subprocess
import sys
import time

APP = "SpiralShuffleRegistered"
TAG = "spiral-signup-v1"
PROFILE = "[SpiralShuffleRegistered]\ntitle=Spiral Shuffle registered players\ndescription=Signup-based game access\nports=12010,12343:12348,12510,12379/tcp\n"
MAX_ROWS = 10000
LEASE_SECONDS = 30 * 86400


def public_ipv4(value):
    ip = ipaddress.ip_address(value)
    if ip.version != 4 or not ip.is_global:
        raise ValueError("Invalid public IPv4 grant")
    return str(ip)


def export(database: Path):
    # This mode must be run as the game owner, never as root.
    if os.geteuid() == 0:
        raise ValueError("Database export must run unprivileged")
    db = sqlite3.connect(database.resolve().as_uri() + "?mode=ro", uri=True, timeout=5)
    try:
        db.execute("PRAGMA query_only=ON")
        rows = db.execute("""SELECT substr(ip, 1, 64),
            CASE WHEN typeof(expires) = 'integer' THEN expires ELSE NULL END
            FROM access_grants WHERE active = 1""").fetchmany(MAX_ROWS + 1)
        if len(rows) > MAX_ROWS:
            raise ValueError("Grant limit exceeded")
        desired_ips([list(row) for row in rows], int(time.time()))
        print(json.dumps(rows))
    finally:
        db.close()


def desired_ips(rows, now):
    if not isinstance(rows, list) or len(rows) > MAX_ROWS:
        raise ValueError("Invalid grant snapshot")
    wanted = set()
    for row in rows:
        if not isinstance(row, list) or len(row) != 2:
            raise ValueError("Invalid grant")
        ip, expires = row
        ip = public_ipv4(ip)
        if type(expires) is not int or expires > now + LEASE_SECONDS + 60:
            raise ValueError("Invalid expiry")
        if expires > now:
            wanted.add(ip)
    return wanted


def rule(ip):
    return ["allow", "from", public_ipv4(ip), "to", "any", "app", APP, "comment", TAG]


def managed_ips(output):
    found = set()
    for line in output.splitlines():
        tokens = shlex.split(line)
        if APP not in tokens:
            continue
        # Never adopt a manually maintained or unexpected rule for this profile.
        if len(tokens) != 10 or tokens[0] != "ufw":
            raise ValueError("Unexpected rule for reserved application profile")
        ip = public_ipv4(tokens[3])
        if tokens != ["ufw", *rule(ip)]:
            raise ValueError("Unmanaged rule uses reserved application profile")
        found.add(ip)
    return found


def run_ufw(*args):
    return subprocess.run(["/usr/sbin/ufw", *args], check=True, capture_output=True,
                          text=True, timeout=30, env={"PATH": "/usr/sbin:/usr/bin:/sbin:/bin", "LC_ALL": "C"}).stdout


def reconcile(wanted, ufw=run_ufw):
    # Revalidate before executing any mutation, including direct callers.
    wanted = {public_ipv4(ip) for ip in wanted}
    status = ufw("status", "verbose")
    if not status.startswith("Status: active") or "Default: deny (incoming)" not in status:
        raise ValueError("Requires active UFW with default-deny incoming traffic")
    current = managed_ips(ufw("show", "added"))
    for ip in sorted(current - wanted):
        ufw("--force", "delete", *rule(ip))
    for ip in sorted(wanted - current):
        ufw(*rule(ip))
    return len(wanted)


def sync(owner, database):
    import pwd
    if os.geteuid() != 0 or pwd.getpwnam(owner).pw_uid == 0:
        raise ValueError("Sync requires root and a non-root game owner")
    # The service must execute this root-owned copy, never checkout code as root.
    script = Path(__file__).resolve()
    for path in (script, *script.parents):
        info = path.stat()
        if info.st_uid != 0 or info.st_mode & 0o022:
            raise ValueError("Firewall executable and parents must be root-owned and not writable by others")
    profile = Path("/etc/ufw/applications.d/spiral-shuffle-registered")
    if profile.read_text() != PROFILE or profile.stat().st_uid != 0 or profile.stat().st_mode & 0o022:
        raise ValueError("Missing or modified root-owned UFW profile")
    snapshot = subprocess.run([
        "/usr/sbin/runuser", "--user", owner, "--", "/usr/bin/python3", "-I",
        str(script), "--export", str(database),
    ], check=True, capture_output=True, text=True, timeout=15,
       env={"PATH": "/usr/sbin:/usr/bin:/sbin:/bin", "LC_ALL": "C"})
    if len(snapshot.stdout) > 1000000:
        raise ValueError("Grant snapshot too large")
    wanted = desired_ips(json.loads(snapshot.stdout), int(time.time()))
    reconcile(wanted)
    print(f"Registered-player access synchronized: {len(wanted)} active addresses.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--export", type=Path)
    parser.add_argument("--owner")
    parser.add_argument("--database", type=Path)
    args = parser.parse_args()
    try:
        if args.export and not args.owner and not args.database:
            export(args.export)
        elif args.owner and args.database and not args.export:
            sync(args.owner, args.database)
        else:
            parser.error("Use --export PATH or --owner USER --database PATH")
    except Exception as error:
        # Never print database paths, IPs, raw subprocess output or credentials.
        print(f"Registered-player access sync failed ({type(error).__name__}); inspect installation and service health.", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
