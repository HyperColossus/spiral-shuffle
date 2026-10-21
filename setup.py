"""Build a separate randomizer profile from an existing Revive101 install."""
import argparse
import hashlib
import json
import os
import re
import secrets
import shutil
import subprocess
import sys
import urllib.request
import zipfile
from pathlib import Path


ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / ".runtime"
BUILD = ROOT / ".build"
COMMIT = "b07987168fcfa2d123a5d78df90ca0c76405e427"
ARCHIVE_SHA256 = "d583b12f18549ccc0c5f1d39ab22c12b021bb00157430805f066643556a56c9e"
REVISION = "V_r806919.Wizard_1_610"
UPSTREAM = BUILD / "upstream" / f"Imlight-{COMMIT}"
SERVER = RUNTIME / "server"
CLIENT = RUNTIME / "client"


def run(*arguments: str) -> None:
    subprocess.run(arguments, cwd=ROOT, check=True)


def require_file(path: Path) -> None:
    if not path.is_file():
        raise FileNotFoundError(f"Required file missing: {path}")


def download_upstream() -> None:
    archive = BUILD / "upstream.zip"
    BUILD.mkdir(exist_ok=True)
    if not archive.exists():
        url = f"https://codeload.github.com/Revive101/Imlight/zip/{COMMIT}"
        with urllib.request.urlopen(url, timeout=120) as response, archive.open("wb") as target:
            shutil.copyfileobj(response, target)
    with archive.open("rb") as stream:
        if hashlib.file_digest(stream, "sha256").hexdigest() != ARCHIVE_SHA256:
            raise ValueError("Upstream archive checksum failed. Remove .build/upstream.zip and retry.")
    destination = (BUILD / "upstream").resolve()
    with zipfile.ZipFile(archive) as source:
        if any(not (destination / name).resolve().is_relative_to(destination) for name in source.namelist()):
            raise ValueError("Invalid upstream archive path.")
        source.extractall(destination)


def copy_server(revive: Path) -> None:
    SERVER.mkdir(parents=True, exist_ok=True)
    for file in (revive / "Imlight").iterdir():
        if file.is_file() and file.suffix.lower() in {".dll", ".exe", ".json"}:
            shutil.copy2(file, SERVER / file.name)
    shutil.copytree(revive / "Imlight/RavenDBServer", SERVER / "RavenDBServer", dirs_exist_ok=True,
                    ignore=shutil.ignore_patterns("*.log", "*.pfx", "*.pem", "*.key"))


def install_server_dependencies(output: Path) -> None:
    built = json.loads((output / "Imlight.Director.deps.json").read_text())
    packages = {name: info for name, info in built["libraries"].items() if info["type"] == "package"}
    versions = dict(name.rsplit("/", 1) for name in packages)
    built_target = built["targets"][built["runtimeTarget"]["name"]]
    package_entries = {name: built_target[name] for name in packages}
    assets: set[Path] = set()
    for entry in package_entries.values():
        for section in ("runtime", "native", "runtimeTargets"):
            for name in entry.get(section, {}):
                relative = Path(name) if section == "runtimeTargets" else Path(name).name
                assets.add(Path(relative))
    for relative in assets:
        require_file(output / relative)

    manifest_path = SERVER / "Imlight.Director.deps.json"
    installed = json.loads(manifest_path.read_text())
    for target in installed["targets"].values():
        for name in list(target):
            if name.rsplit("/", 1)[0] in versions:
                del target[name]
        for name, entry in target.items():
            dependencies = entry.get("dependencies", {})
            for package, version in versions.items():
                if package in dependencies or name.startswith("Imlight.CoreLib/"):
                    dependencies[package] = version
            if dependencies:
                entry["dependencies"] = dependencies
        target.update(package_entries)
    libraries = installed["libraries"]
    for name in list(libraries):
        if name.rsplit("/", 1)[0] in versions:
            del libraries[name]
    libraries.update(packages)

    for relative in sorted(assets):
        destination = SERVER / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(output / relative, destination)
    manifest_path.write_text(json.dumps(installed, indent=2), encoding="utf-8")


def build_server(dotnet: str) -> None:
    arguments = [dotnet, "build", "director-build/Imlight.Director.csproj", "-c", "Release", "-v", "quiet"]
    manifest = json.loads((SERVER / "Imlight.Director.deps.json").read_text())
    _, separator, runtime_id = manifest["runtimeTarget"]["name"].partition("/")
    if separator:
        arguments.extend(["-r", runtime_id, "--self-contained", "false", "-p:AppendRuntimeIdentifierToOutputPath=false"])
    run(*arguments)
    output = ROOT / "director-build/bin/Release/net10.0"
    for name in ("Imlight.CoreLib.dll", "Imlight.Director.dll"):
        require_file(output / name)
    install_server_dependencies(output)
    for name in ("Imlight.CoreLib.dll", "Imlight.Director.dll"):
        shutil.copy2(output / name, SERVER / name)


def replace_setting(config: str, name: str, value: str) -> str:
    content, count = re.subn(rf"(?m)^{re.escape(name)}\s*=.*$", lambda _: f"{name} = {value}", config)
    if count != 1:
        raise ValueError(f"Expected one upstream setting: {name}")
    return content


def configure_server(revive: Path) -> None:
    login_file = RUNTIME / "login.json"
    login = json.loads(login_file.read_text()) if login_file.exists() else {
        "session_salt": secrets.token_hex(32)
    }
    login.update(username="admin", password="compsci")
    config_directory = SERVER / "Config"
    config_directory.mkdir(exist_ok=True)
    template = UPSTREAM / "src/Imlight.Director/Config"
    config = (template / "Imlight.ini").read_text(encoding="utf-8-sig")
    settings = {
        "GameRevision": REVISION.removeprefix("V_"),
        "LoginServerPort": "12010", "GameServerPort": "12343", "GameServerIP": "127.0.0.1",
        "GameServerCount": "6", "MaxGameServersAllowed": "6",
        "RealmNames": "Shuffle,Chaos,Nightmare,ShuffleCreatures,ChaosCreatures,NightmareCreatures",
        "PatchServerPort": "12510", "PatchServerInternalUrl": "http://127.0.0.1:12379",
        "EmbeddedDatabasePort": "8090", "EmbeddedDatabaseDataDirectory": (RUNTIME / "saves").as_posix(),
        "EmbeddedDatabaseUseFull": "false", "PlayerDatabaseUrl": "", "PlayerDatabaseCertificatePath": "",
        "SpiralDBLocalPath": (revive / "Imlight/spiraldb-cache").as_posix(),
        "SpiralDBAutoFetch": "false", "SpiralDBDisableRemote": "true", "LocalWadCachePath": "./cache",
        "RandomizeCreatures": "false", "RandomizeGateways": "false", "RandomBackflips": "false",
        "StartingLevel": "1", "TutorialDisabled": "false", "DefaultItems": "87256,284071,126983", "LogLevel": "INFO",
        "AdminAccountUsername": login["username"], "AdminAccountPassword": login["password"],
        "SessionKeyHashInput": login["session_salt"],
    }
    for name, value in settings.items():
        config = replace_setting(config, name, value)
    (config_directory / "Imlight.ini").write_text(config.replace("\u2014", ","), encoding="utf-8")
    actor_config = (template / "akka.conf").read_text(encoding="utf-8-sig")
    actor_config = actor_config.replace("port = 12999", "port = 13009")
    (config_directory / "akka.conf").write_text(actor_config.replace("\u2014", ","), encoding="utf-8")
    login_file.write_text(json.dumps(login, indent=2), encoding="utf-8")
    (RUNTIME / "login.txt").write_text(
        f"Local Spiral Shuffle login\nUsername: {login['username']}\nPassword: {login['password']}\n",
        encoding="utf-8")


def install_client(revive: Path, client_source: Path) -> None:
    data = revive / "Aurorium/data" / REVISION / "Data"
    needed = sum(file.stat().st_size for file in data.rglob("*") if file.is_file() and
                 not (CLIENT / "Data" / file.relative_to(data)).exists())
    if shutil.disk_usage(RUNTIME).free < needed + 1024 ** 3:
        raise OSError("Not enough free disk space for the separate client.")
    excluded = shutil.ignore_patterns("*.log", "*.dmp", "*.pfx", "*.pem", "*.key", "config.xml",
                                     "preferences.xml", "previouscommands.xml", "state.dat", "queued_packages.txt")
    shutil.copytree(client_source / "Bin", CLIENT / "Bin", dirs_exist_ok=True, ignore=excluded)
    for source in data.rglob("*"):
        if source.is_file():
            target = CLIENT / "Data" / source.relative_to(data)
            if target.exists() and target.stat().st_size == source.stat().st_size:
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, target)
    patched = RUNTIME / "patch" / REVISION / "Data/GameData/Root.wad"
    shutil.copy2(patched, CLIENT / "Data/GameData/Root.wad")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--revive-path", type=Path, required=True)
    parser.add_argument("--client-path", type=Path,
                        default=Path(os.environ.get("PROGRAMDATA", "C:/ProgramData")) / "KingsIsle Entertainment/Wizard101")
    parser.add_argument("--seed", default="SPIRAL-101")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--build-only", action="store_true")
    args = parser.parse_args()
    if sys.version_info < (3, 12):
        parser.error("Python 3.12 or newer is required.")
    if not re.fullmatch(r"[A-Za-z0-9_-]{1,32}", args.seed):
        parser.error("Use 1-32 letters, numbers, underscores, or hyphens for the seed.")
    revive = args.revive_path.resolve()
    client = args.client_path.resolve()
    dotnet = shutil.which(args.dotnet)
    if dotnet is None:
        parser.error("Install the .NET 10 SDK first.")
    sdk_list = subprocess.check_output([dotnet, "--list-sdks"], text=True)
    if not re.search(r"(?m)^10\.", sdk_list):
        parser.error("The .NET 10 SDK is required.")
    for path in [revive / "Imlight/Imlight.Director.exe", revive / "Imlight/Imlight.Director.dll",
                 revive / "Imlight/Imlight.Director.deps.json", revive / "Imlight/Imlight.Director.runtimeconfig.json",
                 revive / "Imlight/Imlight.CoreLib.dll", revive / "Imlight/Imcodec.ObjectProperty.dll",
                 revive / "Aurorium/data" / REVISION / "Data/GameData/Root.wad",
                 revive / "Aurorium/data" / REVISION / "LatestFileList.bin",
                 revive / "Aurorium/data" / REVISION / "LatestFileList.xml",
                 client / "Bin/WizardGraphicalClient.exe"]:
        require_file(path)
    for name in ["NpcSpellInventory", "QuestTemplates"]:
        if not (revive / "Imlight/spiraldb-cache" / name).is_dir():
            parser.error(f"Revive101 is missing its world data: {name}")
    for directory in [RUNTIME, RUNTIME / "generated", RUNTIME / "native", RUNTIME / "patch"]:
        directory.mkdir(parents=True, exist_ok=True)
    settings = {"revive_path": str(revive), "client_path": str(client), "seed": args.seed,
                "python": sys.executable, "dotnet": dotnet}
    (RUNTIME / "settings.json").write_text(json.dumps(settings, indent=2), encoding="utf-8")
    download_upstream()
    copy_server(revive)
    configure_server(revive)
    print("Building the server and randomized assets...", flush=True)
    run(sys.executable, "prepare_source.py")
    build_server(dotnet)
    run(dotnet, "run", "--project", "asset-tool", "-c", "Release", "--", str(revive),
        str(RUNTIME / "generated"), args.seed)
    run(sys.executable, "extract_ui.py")
    run(sys.executable, "build_menu.py")
    run(sys.executable, "build_enemy_ui.py")
    run(sys.executable, "build_reward_ui.py")
    run(sys.executable, "pack_assets.py")
    shutil.copy2(RUNTIME / "generated/catalog.json", SERVER / "Config/Randomizer.json")
    run(sys.executable, "verify_assets.py")
    if args.build_only:
        print("Build verified. Run setup without --build-only to copy the client.")
        return
    print("Copying the separate game client...", flush=True)
    install_client(revive, client)
    print("Ready. Open Play.cmd and sign in with admin / compsci.")


if __name__ == "__main__":
    main()
