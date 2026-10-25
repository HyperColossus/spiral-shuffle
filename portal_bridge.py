"""Install an optional local Imlight registration bridge WITHOUT rebuilding game assets.

Run 'init' while server runs; run 'install' only after ./Stop-Linux.sh.
This installer only changes the staged Director's startup source and DLL.
It never changes RavenDB saves, accounts, the game data, other processes or UFW.
"""
from __future__ import annotations

import argparse
import os
from pathlib import Path
import re
import secrets
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / ".runtime"
SERVER = RUNTIME / "server"
PRIVATE = RUNTIME / "portal"
SOURCE_DIR = ROOT / ".build/upstream/Imlight-b07987168fcfa2d123a5d78df90ca0c76405e427/src/Imlight.Director"
DIRECTOR_PROJECT = ROOT / "director-build/Imlight.Director.csproj"
ANCHOR = "        CreateEmbeddedDatabaseAccounts();\n\n        OnlinePlayerCollection.Clear();"
PATCHED = "        CreateEmbeddedDatabaseAccounts();\n        PortalRegistration.Start();\n\n        OnlinePlayerCollection.Clear();"


def init():
    PRIVATE.mkdir(mode=0o700, parents=True, exist_ok=True)
    PRIVATE.chmod(0o700)
    token = PRIVATE / "bridge-token"
    if not token.exists():
        fd = os.open(token, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
        with os.fdopen(fd, "w") as out:
            out.write(secrets.token_hex(32) + "\n")
    if not re.fullmatch(r"[a-fA-F0-9]{64}\s*", token.read_text()):
        raise ValueError("Invalid bridge token; bridge remains disabled.")
    token.chmod(0o600)
    env = PRIVATE / "portal.env"
    if not env.exists():
        fd = os.open(env, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
        with os.fdopen(fd, "w") as out:
            out.write(
                "# Load via: set -a; source .runtime/portal/portal.env; set +a\n"
                "SPIRAL_PORTAL_PUBLIC=0\n"
                "SPIRAL_IP_ACCESS=0\n"
                "SPIRAL_ALLOWED_ORIGIN=https://spiral.hypercoloss.us\n"
                "SPIRAL_TURNSTILE_SECRET=\n"
                "SPIRAL_GAME_HOST=\n"
            )
    env.chmod(0o600)
    print("Created/verified private .runtime/portal/bridge-token and portal.env.")
    print("No game process or database changed.")


def install():
    import linux_ctl
    if linux_ctl.owned_process("server") or linux_ctl.owned_process("patch"):
        raise RuntimeError("Spiral Shuffle is running. Stop only this game with ./Stop-Linux.sh first.")
    if not SOURCE_DIR.is_dir() or not DIRECTOR_PROJECT.is_file():
        raise FileNotFoundError("Existing .build source and director-build project required. Run initial Linux setup first.")
    active = SERVER / "Imlight.Director.dll"
    if not active.is_file():
        raise FileNotFoundError(f"Missing {active}")
    init()
    program = SOURCE_DIR / "Program.cs"
    original = program.read_text(encoding="utf-8-sig")
    if PATCHED not in original and original.count(ANCHOR) != 1:
        raise ValueError("Pinned Program.cs changed: refusing to modify an unexpected startup sequence.")
    previous_bridge = SOURCE_DIR / "PortalRegistration.cs"
    old_bridge = previous_bridge.read_bytes() if previous_bridge.exists() else None
    try:
        if PATCHED not in original:
            program.write_text(original.replace(ANCHOR, PATCHED), encoding="utf-8")
        shutil.copy2(ROOT / "PortalRegistration.cs", previous_bridge)
        print("Compiling Imlight registration bridge (game data is unchanged)...", flush=True)
        subprocess.run([
            "dotnet", "build", str(DIRECTOR_PROJECT), "-c", "Release", "-v", "minimal",
            "-r", "linux-x64", "--self-contained", "false",
            "-p:AppendRuntimeIdentifierToOutputPath=false"
        ], cwd=ROOT, check=True)
        compiled = ROOT / "director-build/bin/Release/net10.0/Imlight.Director.dll"
        if not compiled.is_file():
            raise FileNotFoundError(f"Compilation output missing: {compiled}")
    except BaseException:
        program.write_text(original, encoding="utf-8")
        if old_bridge is None:
            previous_bridge.unlink(missing_ok=True)
        else:
            previous_bridge.write_bytes(old_bridge)
        raise

    backup = SERVER / "Imlight.Director.before-portal.dll"
    if not backup.exists():
        shutil.copy2(active, backup)
    temp = active.with_suffix(".dll.portal-new")
    shutil.copy2(compiled, temp)
    os.replace(temp, active)
    print("Installed compiled Director bridge; game assets and player saves were not modified.")
    print("Start Spiral Shuffle and check curl http://127.0.0.1:12772/health.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=("init", "install"))
    args = parser.parse_args()
    try:
        (init if args.action == "init" else install)()
    except (OSError, RuntimeError, ValueError, subprocess.CalledProcessError) as error:
        print(f"Bridge setup failed: {error}", file=sys.stderr)
        raise SystemExit(2)
