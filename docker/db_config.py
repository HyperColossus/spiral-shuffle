"""Switch Spiral Shuffle to a separately imported Docker RavenDB, preserving original saves.

Never starts or stops processes. Must stop only Spiral Shuffle before switching.
Requires explicit --confirm-imported after manual RavenDB export/import verification.
"""
from __future__ import annotations
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen
from urllib.error import HTTPError, URLError
import argparse
import json
import re
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
CONFIG = ROOT / ".runtime/server/Config/Imlight.ini"
DB_URL = "http://127.0.0.1:18080"
DB_NAME = "Playerdata"
# The pinned Imlight GetCertificate() calls Path.GetFullPath even for empty paths.
# A deliberately absent path lets a localhost HTTP connection use no certificate.
NO_CERT = ROOT / ".runtime/server/Config/DO-NOT-CREATE-local-docker-certificate.pfx"

def replace_setting(source, key, value):
    pattern = r"(?m)^(" + re.escape(key) + r"\s*=\s*).*$"
    changed, count = re.subn(pattern, lambda m: m.group(1) + value, source)
    if count != 1:
        raise ValueError(f"Expected exactly one {key} setting")
    return changed

def get_setting(source, key):
    result = re.findall(r"(?m)^" + re.escape(key) + r"\s*=\s*(.*?)\s*$", source)
    if len(result) != 1:
        raise ValueError(f"Expected exactly one {key} setting")
    return result[0]

def check_destination():
    try:
        with urlopen(DB_URL + "/databases/" + DB_NAME + "/stats", timeout=6) as response:
            stats = json.load(response)
    except (HTTPError, URLError, OSError, TimeoutError, ValueError) as exc:
        raise RuntimeError("Cannot inspect Docker Playerdata database. Import it first.") from exc
    count = stats.get("CountOfDocuments")
    if not isinstance(count, int) or count < 1:
        raise RuntimeError("Destination Playerdata is empty or unreadable. Import accounts and saves first.")
    print(f"Docker Playerdata responds with {count} documents; verify account/wizard counts manually.")

def switch():
    sys.path.insert(0, str(ROOT))
    import linux_ctl
    if linux_ctl.owned_process("server") or linux_ctl.owned_process("patch"):
        raise RuntimeError("Stop Spiral Shuffle first with ./Stop-Linux.sh")
    if not CONFIG.is_file():
        raise FileNotFoundError(CONFIG)
    source = CONFIG.read_text()
    if get_setting(source, "PlayerDatabaseName") != DB_NAME:
        raise ValueError("This server uses another player database name: migration needs review.")
    old = get_setting(source, "PlayerDatabaseUrl")
    if old == DB_URL:
        print("Already configured for Docker. No changes.")
        return
    if old:
        raise ValueError("Already using a different external database; refusing switch.")
    if NO_CERT.exists():
        raise ValueError("Unexpected certificate sentinel file exists. Refusing switch.")
    check_destination()
    result = replace_setting(source, "PlayerDatabaseUrl", DB_URL)
    result = replace_setting(result, "PlayerDatabaseCertificatePath", str(NO_CERT.resolve()))
    tag = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    backup = CONFIG.with_name(f"Imlight.ini.before-docker.{tag}")
    shutil.copy2(CONFIG, backup)
    backup.chmod(0o600)
    temp = CONFIG.with_name(f"Imlight.ini.{tag}.tmp")
    try:
        temp.write_text(result)
        temp.chmod(0o600)
        temp.replace(CONFIG)
    finally:
        temp.unlink(missing_ok=True)
    print(f"Switched to {DB_URL} / {DB_NAME}; original save files untouched.")
    print(f"Rollback config backup (private): {backup}")
    print("Start ONLY Spiral Shuffle with ./Play-Linux.sh, then verify original logins.")

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("action", choices=("check", "switch"))
    p.add_argument("--confirm-imported", action="store_true")
    args = p.parse_args()
    try:
        if args.action == "check":
            check_destination()
        elif not args.confirm_imported:
            p.error("Use --confirm-imported ONLY after verifying the database import.")
        else:
            switch()
        return 0
    except (RuntimeError, OSError, ValueError) as exc:
        print(f"Refused: {exc}", file=sys.stderr)
        return 2

if __name__ == "__main__":
    raise SystemExit(main())
