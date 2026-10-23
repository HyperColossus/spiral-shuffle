"""Switch Spiral Shuffle to Docker RavenDB while keeping both old save directories.

Two explicit modes: --confirm-imported (migrate prior accounts) OR --start-fresh
(empty Docker database only). Never starts or stops processes.
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
    pattern = r"(?m)^" + re.escape(key) + r"[ \t]*=[ \t]*[^\r\n]*$"
    changed, count = re.subn(pattern, lambda _: f"{key} = {value}", source)
    if count != 1:
        raise ValueError(f"Expected exactly one {key} setting")
    return changed

def get_setting(source, key):
    result = re.findall(r"(?m)^" + re.escape(key) + r"[ \t]*=[ \t]*([^\r\n]*)$", source)
    if len(result) != 1:
        raise ValueError(f"Expected exactly one {key} setting")
    return result[0]

def check_destination(*, allow_empty=False):
    try:
        with urlopen(DB_URL + "/databases/" + DB_NAME + "/stats", timeout=6) as response:
            stats = json.load(response)
    except (HTTPError, URLError, OSError, TimeoutError, ValueError) as exc:
        raise RuntimeError("Cannot inspect Docker Playerdata. Create the Playerdata database in RavenDB Studio first.") from exc
    count = stats.get("CountOfDocuments")
    if type(count) is not int or count < 0:
        raise RuntimeError("Destination Playerdata document count is invalid.")
    if allow_empty:
        if count != 0:
            raise RuntimeError("Fresh-start refused: Docker Playerdata already contains documents. No data was changed.")
        print("Docker Playerdata exists and is empty: ready for a fresh account database.")
    else:
        if count < 1:
            raise RuntimeError("Destination Playerdata is empty. Import accounts and saves first, or use --start-fresh.")
        print(f"Docker Playerdata responds with {count} documents; verify account/wizard counts manually.")
    return count

def switch(*, start_fresh=False):
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
    if start_fresh:
        password = get_setting(source, "AdminAccountPassword")
        if len(password) < 16 or password.lower() in ("compsci", "123", "password", "admin"):
            raise ValueError("Set a strong AdminAccountPassword (16+ characters) in Imlight.ini before starting fresh.")
    check_destination(allow_empty=start_fresh)
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
    if start_fresh:
        print("Start only Spiral Shuffle with ./Play-Linux.sh; an initial admin account will be created in the empty database.")
    else:
        print("Start only Spiral Shuffle with ./Play-Linux.sh; verify existing accounts and characters.")

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("action", choices=("check", "switch"))
    modes = p.add_mutually_exclusive_group()
    modes.add_argument("--confirm-imported", action="store_true", help="Use a manually imported Playerdata database")
    modes.add_argument("--start-fresh", action="store_true", help="Use an EMPTY Playerdata database; keeps old saves untouched")
    args = p.parse_args()
    try:
        if args.action == "check":
            if args.confirm_imported:
                p.error("The --confirm-imported flag is only for switch")
            check_destination(allow_empty=args.start_fresh)
        elif args.start_fresh:
            switch(start_fresh=True)
        elif args.confirm_imported:
            switch()
        else:
            p.error("Choose --start-fresh for an empty database OR --confirm-imported for a migrated one.")
        return 0
    except (RuntimeError, OSError, ValueError) as exc:
        print(f"Refused: {exc}", file=sys.stderr)
        return 2

if __name__ == "__main__":
    raise SystemExit(main())
