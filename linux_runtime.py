"""Repair the Linux Imlight runtime configuration without rebuilding assets.

The Revive101 Linux bundle can carry a self-contained runtimeconfig.json,
while Spiral Shuffle launches rebuilt framework-dependent DLLs via dotnet.
Only the fork's private .runtime/server is modified.
"""
from __future__ import annotations

import argparse
import json
import os
import shutil
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent
CONFIG = ROOT / ".runtime/server/Imlight.Director.runtimeconfig.json"
FRAMEWORK = {"name": "Microsoft.NETCore.App", "version": "10.0.0"}
DEP_NAME = "Imlight.Director.deps.json"
BUILT_DEPS = ROOT / "director-build/bin/Release/net10.0" / DEP_NAME


def bundled_runtime_dependencies(path: Path) -> tuple[str, ...]:
    """Return runtime-pack entries indicating an app-local .NET Core runtime."""
    data = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(data, dict):
        raise ValueError(f"Invalid dependency manifest: {path}")
    libraries = data.get("libraries", {})
    if not isinstance(libraries, dict):
        raise ValueError(f"Invalid dependency libraries: {path}")
    return tuple(sorted(key for key in libraries if
                        "Microsoft.NETCore.App.Runtime." in key
                        or "runtimepack.Microsoft.NETCore.App.Runtime." in key))


def repair_deps(installed: Path = CONFIG.parent / DEP_NAME,
                compiled: Path = BUILT_DEPS) -> bool:
    """Use matching framework-dependent build metadata instead of bundled-runtime metadata.

    The original release's self-contained .deps.json can still direct hostpolicy
    to seek libcoreclr.so in the app directory after runtimeconfig is repaired.
    Require a locally compiled .NET 10 dependency manifest, back up the
    installed file once, and never touch the original Revive101 installation.
    """
    if not compiled.is_file():
        raise FileNotFoundError(
            f"Rebuilt .NET dependency manifest not found: {compiled}. "
            "Do not overwrite with an unrelated deps.json."
        )
    original = json.loads(installed.read_text(encoding="utf-8"))
    replacement = json.loads(compiled.read_text(encoding="utf-8"))
    if not isinstance(original, dict) or not isinstance(replacement, dict):
        raise ValueError("Invalid .deps.json root object")
    for manifest, name in ((original, "installed"), (replacement, "compiled")):
        target = str(manifest.get("runtimeTarget", {}).get("name", ""))
        if "v10.0" not in target or ("/" in target and "linux-" not in target):
            raise ValueError(f"{name} manifest does not target .NET 10 Linux: {target}")
    if bundled_runtime_dependencies(compiled):
        raise ValueError("Compiled manifest still references bundled CoreCLR; refusing unsafe replacement")
    if not bundled_runtime_dependencies(installed):
        return False
    if not installed.with_name("Imlight.Director.dll").is_file():
        raise FileNotFoundError("Installed Imlight.Director.dll missing")
    backup = installed.with_name("Imlight.Director.deps.before-linux-fix.json")
    if not backup.exists():
        shutil.copy2(installed, backup)
    fd, tmp = tempfile.mkstemp(prefix=".deps-", suffix=".tmp", dir=installed.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as file:
            json.dump(replacement, file, indent=2)
            file.write("\\n")
            file.flush()
            os.fsync(file.fileno())
        os.chmod(tmp, 0o600)
        os.replace(tmp, installed)
    finally:
        if os.path.exists(tmp):
            os.unlink(tmp)
    return True



def has_framework(path: Path) -> bool:
    """True if the app selects an installed shared-framework family."""
    try:
        options = json.loads(path.read_text(encoding="utf-8"))["runtimeOptions"]
        frameworks = options.get("frameworks", [])
        if isinstance(options.get("framework"), dict):
            frameworks = [*frameworks, options["framework"]]
        return any(isinstance(entry, dict) and entry.get("name") == "Microsoft.NETCore.App"
                   and str(entry.get("version", "")).startswith("10.")
                   for entry in frameworks)
    except (OSError, ValueError, KeyError, TypeError):
        return False


def repair_runtimeconfig(path: Path = CONFIG) -> bool:
    """Add a .NET 10 runtime framework reference. Return True if modified.

    Preserves configProperties, and makes a one-time backup. Does not touch
    the original Revive101 distribution, the player database, or game assets.
    """
    if not path.with_name("Imlight.Director.dll").is_file():
        raise FileNotFoundError(f"Imlight.Director.dll is missing beside {path}")
    if has_framework(path):
        return False
    settings = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}
    if not isinstance(settings, dict):
        raise ValueError("Invalid runtimeconfig root object")
    options = settings.setdefault("runtimeOptions", {})
    if not isinstance(options, dict):
        raise ValueError("Invalid runtimeOptions in runtimeconfig")
    if options.get("frameworks") or options.get("framework"):
        raise ValueError("Existing framework definition requires manual review")
    options.pop("includedFrameworks", None)  # self-contained deployment marker
    options["tfm"] = "net10.0"
    options["framework"] = FRAMEWORK.copy()

    if path.exists():
        backup = path.with_name("Imlight.Director.runtimeconfig.before-linux-fix.json")
        if not backup.exists():
            shutil.copy2(path, backup)
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, name = tempfile.mkstemp(prefix=".runtimeconfig-", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as file:
            json.dump(settings, file, indent=2)
            file.write("\n")
            file.flush()
            os.fsync(file.fileno())
        os.chmod(name, 0o600)
        os.replace(name, path)
    finally:
        if os.path.exists(name):
            os.unlink(name)
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["check", "repair", "repair-deps"])
    args = parser.parse_args()
    if args.action == "check":
        ok = has_framework(CONFIG)
        print("Runtime configuration: OK" if ok else
              "Runtime configuration missing .NET 10 framework reference; run: python3 linux_runtime.py repair")
        manifest = CONFIG.parent / DEP_NAME
        if manifest.is_file():
            bundled = bundled_runtime_dependencies(manifest)
            print(f"Bundled-runtime dependency entries: {len(bundled)}")
            if bundled:
                print("Bundled CoreCLR references found; run: python3 linux_runtime.py repair-deps")
        else:
            print("Missing Imlight.Director.deps.json")
            ok = False
        return 0 if ok and manifest.is_file() and not bundled else 2
    if args.action == "repair-deps":
        changed = repair_deps()
        print("Updated dependency manifest to matching framework-dependent build." if changed
              else "Dependency manifest does not reference a bundled runtime; no replacement needed.")
        return 0
    changed = repair_runtimeconfig()
    print("Updated the Linux .NET 10 runtime configuration." if changed
          else "Linux .NET 10 runtime configuration is already correct.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
