"""Linux-only, isolated server-side build for ljste/spiral-shuffle.

Place at the root of a fork of the original repository. Does not contain,
download, or redistribute commercial Wizard101 client assets.
"""
from __future__ import annotations

import argparse
import ipaddress
import json
import os
import re
import secrets
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

# Reuse the exact pinned upstream source build and game-asset generation
# from the original Windows setup, without running its Windows-only setup.
import setup as original
from linux_runtime import repair_runtimeconfig, repair_deps

ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / ".runtime"
SERVER = RUNTIME / "server"
REVISION = original.REVISION
REQUIRED_REVIVE_FILES = (
    "Imlight/Imlight.Director.dll",
    "Imlight/Imlight.Director.deps.json",
    "Imlight/Imlight.Director.runtimeconfig.json",
    "Imlight/Imlight.CoreLib.dll",
    "Imlight/Imcodec.ObjectProperty.dll",
    "Imlight/libzstd.linux.x64.so",
    f"Aurorium/data/{REVISION}/Data/GameData/Root.wad",
    f"Aurorium/data/{REVISION}/LatestFileList.bin",
    f"Aurorium/data/{REVISION}/LatestFileList.xml",
)
CONFIG_PORTS = (12010, 12343, 12344, 12345, 12346, 12347, 12510, 8090, 13009, 12379)


def check_prerequisites(revive: Path, dotnet: str | None) -> list[str]:
    """Return actionable diagnostics without making any changes."""
    missing = [str(revive / entry) for entry in REQUIRED_REVIVE_FILES if not (revive / entry).is_file()]
    for category in ("QuestTemplates", "NpcSpellInventory"):
        if not (revive / "Imlight/spiraldb-cache" / category).is_dir():
            missing.append(str(revive / "Imlight/spiraldb-cache" / category))
    if not dotnet:
        missing.append("dotnet executable (.NET 10 SDK required)")
    else:
        try:
            p = subprocess.run([dotnet, "--list-sdks"], text=True, capture_output=True, check=True, timeout=20)
            if not re.search(r"(?m)^10\.", p.stdout):
                missing.append(".NET 10 SDK (dotnet --list-sdks)")
        except (OSError, subprocess.CalledProcessError, subprocess.TimeoutExpired):
            missing.append("working .NET 10 SDK")
    return missing


def stage_server(revive: Path) -> None:
    """Copy only installed Linux Imlight assemblies, metadata, and RavenDB."""
    source = revive / "Imlight"
    manifest = json.loads((source / "Imlight.Director.deps.json").read_text(encoding="utf-8"))
    runtime_target = manifest.get("runtimeTarget", {}).get("name", "")
    if "/linux-" not in runtime_target:
        raise ValueError(f"Expected Linux Imlight release, got runtime target: {runtime_target!r}")
    SERVER.mkdir(parents=True, exist_ok=True)
    for file in source.iterdir():
        # RavenDB/Sparrow loads native libraries (not just managed DLLs).
        # Copy versioned .so files too, but no Windows .exe launchers.
        native_so = file.suffix.lower() == ".so" or bool(re.search(r"\\.so(?:\\.\\d+)+$", file.name))
        if file.is_file() and (file.suffix.lower() in {".dll", ".json"} or native_so):
            shutil.copy2(file, SERVER / file.name)
    database = source / "RavenDBServer"
    if not database.is_dir():
        raise FileNotFoundError("Linux Imlight install is missing Imlight/RavenDBServer; check the release layout")
    shutil.copytree(database, SERVER / "RavenDBServer", dirs_exist_ok=True,
                    ignore=shutil.ignore_patterns("*.log", "*.pfx", "*.pem", "*.key"))


def account_credentials() -> dict[str, str]:
    """Persist a randomized login, never install the Windows demo password."""
    path = RUNTIME / "login.json"
    if path.exists():
        obj = json.loads(path.read_text(encoding="utf-8"))
        if not all(isinstance(obj.get(k), str) and obj[k] for k in ("username", "password", "session_salt")):
            raise ValueError("Invalid existing login.json. Refusing to regenerate credentials silently")
        if obj["password"] == "compsci":
            raise ValueError("Existing default password 'compsci' is insecure; change .runtime/login.json securely")
        return obj
    login = {"username": "admin", "password": secrets.token_urlsafe(28), "session_salt": secrets.token_hex(32)}
    path.parent.mkdir(parents=True, exist_ok=True)
    # Exclusive file creation avoids clobbering login credentials under concurrent setup.
    fd = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    with os.fdopen(fd, "w", encoding="utf-8") as out:
        json.dump(login, out, indent=2)
        out.write("\n")
    return login


def configure_server(revive: Path, game_ip: str) -> None:
    login = account_credentials()
    config = original.UPSTREAM / "src/Imlight.Director/Config"
    ini = (config / "Imlight.ini").read_text(encoding="utf-8-sig")
    values = {
        "GameRevision": REVISION.removeprefix("V_"),
        "LoginServerPort": "12010", "GameServerPort": "12343", "GameServerIP": game_ip,
        "GameServerCount": "6", "MaxGameServersAllowed": "6",
        "RealmNames": "Shuffle,Chaos,Nightmare,ShuffleCreatures,ChaosCreatures,NightmareCreatures",
        "PatchServerPort": "12510", "PatchServerInternalUrl": "http://127.0.0.1:12379",
        "EmbeddedDatabasePort": "8090", "EmbeddedDatabaseDataDirectory": str((RUNTIME / "saves").resolve()),
        "EmbeddedDatabaseUseFull": "false", "PlayerDatabaseUrl": "", "PlayerDatabaseCertificatePath": "",
        "SpiralDBLocalPath": str((revive / "Imlight/spiraldb-cache").resolve()),
        "SpiralDBAutoFetch": "false", "SpiralDBDisableRemote": "true", "LocalWadCachePath": "./cache",
        "RandomizeCreatures": "false", "RandomizeGateways": "false", "RandomBackflips": "false",
        "StartingLevel": "1", "TutorialDisabled": "false", "DefaultItems": "87256,284071,126983",
        "LogLevel": "INFO", "AdminAccountUsername": login["username"],
        "AdminAccountPassword": login["password"], "SessionKeyHashInput": login["session_salt"],
    }
    for key, value in values.items():
        ini = original.replace_setting(ini, key, value)
    dest = SERVER / "Config"
    dest.mkdir(parents=True, exist_ok=True)
    path = dest / "Imlight.ini"
    # Do not allow a window of world-readable credentials while updating.
    with open(path, "w", encoding="utf-8", opener=lambda p, flags: os.open(p, flags, 0o600)) as out:
        out.write(ini.replace("\u2014", ","))
    os.chmod(path, 0o600)
    akka = (config / "akka.conf").read_text(encoding="utf-8-sig")
    if akka.count("port = 12999") != 1:
        raise ValueError("Expected one Akka port = 12999 in pinned Imlight config")
    (dest / "akka.conf").write_text(akka.replace("port = 12999", "port = 13009").replace("\u2014", ","), encoding="utf-8")
    (RUNTIME / "saves").mkdir(parents=True, exist_ok=True)


def run(*parts: str) -> None:
    subprocess.run(parts, cwd=ROOT, check=True)


def build(revive: Path, dotnet: str, seed: str, game_ip: str) -> None:
    for folder in (RUNTIME, RUNTIME / "generated", RUNTIME / "native", RUNTIME / "patch"):
        folder.mkdir(parents=True, exist_ok=True)
    # Original profile_paths.py reads revive_path and seed from this file.
    settings = {"revive_path": str(revive), "client_path": "", "python": sys.executable,
                "dotnet": dotnet, "seed": seed, "platform": "linux", "game_host": game_ip}
    (RUNTIME / "settings.json").write_text(json.dumps(settings, indent=2) + "\n", encoding="utf-8")
    original.download_upstream()  # Verifies pinned source ZIP with SHA-256.
    stage_server(revive)
    configure_server(revive, game_ip)
    run(sys.executable, "prepare_source.py")
    # Original build_server reads the installed Linux deps.json target and
    # builds the matching CoreLib/Director assemblies and NuGet dependencies.
    original.build_server(dotnet)
    # Revive101's Linux runtimeconfig can describe self-contained deployment,
    # whereas this build is framework-dependent and runs via 'dotnet'.
    repair_runtimeconfig(SERVER / "Imlight.Director.runtimeconfig.json")
    # Pair the rebuilt DLL with its framework-dependent dependency manifest.
    repair_deps(SERVER / "Imlight.Director.deps.json",
                ROOT / "director-build/bin/Release/net10.0/Imlight.Director.deps.json")
    run(dotnet, "run", "--project", "asset-tool", "-c", "Release", "--", str(revive),
        str(RUNTIME / "generated"), seed)
    for filename in ("extract_ui.py", "build_menu.py", "build_enemy_ui.py", "build_reward_ui.py",
                     "pack_assets.py", "verify_assets.py"):
        run(sys.executable, filename)
    shutil.copy2(RUNTIME / "generated/catalog.json", SERVER / "Config/Randomizer.json")
    print("Linux server build and patch ready. No client installed and no services started.")
    print(f"Login credentials are saved privately at {RUNTIME / 'login.json'} (0600).")


def export_private_client_patch(destination: Path) -> None:
    """Optional personal-use client patch; contains proprietary user-owned assets."""
    base = RUNTIME / "patch" / REVISION
    needed = ("LatestFileList.bin", "LatestFileList.xml", "Data/GameData/Root.wad")
    for name in needed:
        if not (base / name).is_file():
            raise FileNotFoundError(f"Not generated: {base / name}")
    destination.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(destination, "w", compression=zipfile.ZIP_STORED) as output:
        for name in needed:
            output.write(base / name, arcname=f"{REVISION}/{name}")
    os.chmod(destination, 0o600)
    print(f"Personal client patch created at {destination}. Do not upload or distribute its proprietary game assets.")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--revive-path", type=Path, required=True,
                        help="folder containing a Linux Imlight/ installation and Aurorium/data/")
    parser.add_argument("--seed", default="SPIRAL-101")
    parser.add_argument("--game-ip", default="127.0.0.1",
                        help="network IP advertised to game clients; loopback by default")
    parser.add_argument("--allow-lan", action="store_true", help="explicitly opt into non-loopback game service")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--check-only", action="store_true", help="inspect prerequisites; do not modify anything")
    parser.add_argument("--export-client-patch", type=Path, help="optional local-only zip of patched user game assets")
    args = parser.parse_args(argv)
    if not sys.platform.startswith("linux"):
        parser.error("Use setup.py/Setup.cmd on Windows. This script is for Linux hosts only")
    if sys.version_info < (3, 12):
        parser.error("Python 3.12 or newer is required")
    if not re.fullmatch(r"[A-Za-z0-9_-]{1,32}", args.seed):
        parser.error("Seed must be 1-32 letters/numbers/underscore/hyphen")
    try:
        address = ipaddress.ip_address(args.game_ip)
    except ValueError:
        parser.error("--game-ip must be a valid IPv4 address (no hostnames or config text)")
    if address.version != 4 or address.is_multicast or address.is_unspecified:
        parser.error("--game-ip must be a concrete IPv4 address usable by a client")
    if not address.is_loopback and not args.allow_lan:
        parser.error("Non-loopback game IP requires --allow-lan (configure your firewall separately)")
    revive = args.revive_path.expanduser().resolve()
    dotnet = shutil.which(args.dotnet)
    errors = check_prerequisites(revive, dotnet)
    if errors:
        for item in errors:
            print("MISSING:", item, file=sys.stderr)
        return 2
    if args.check_only:
        print("Prerequisites found. No changes made.")
        return 0
    if os.geteuid() == 0:
        parser.error("Do not run the game server setup as root; use a dedicated unprivileged account")
    build(revive, dotnet, args.seed, args.game_ip)
    if args.export_client_patch:
        export_private_client_patch(args.export_client_patch.expanduser().resolve())
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
