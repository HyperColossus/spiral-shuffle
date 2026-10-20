"""Check dependency installation when Revive101 has no Git libraries."""
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import setup as installer


TARGET = ".NETCoreApp,Version=v10.0"
MANAGED = "LibGit2Sharp/0.32.0"
NATIVE = "LibGit2Sharp.NativeBinaries/2.0.324"
NATIVE_ASSET = "runtimes/win-x64/native/git2-test.dll"


def write_build_fixture(root: Path) -> Path:
    output = root / "director-build/bin/Release/net10.0"
    output.mkdir(parents=True)
    (output / Path(NATIVE_ASSET).name).write_bytes(b"native dependency fixture")
    (output / "LibGit2Sharp.dll").write_bytes(b"managed dependency fixture")
    dependencies = {
        "runtimeTarget": {"name": TARGET + "/win-x64"},
        "targets": {TARGET + "/win-x64": {
            MANAGED: {"dependencies": {"LibGit2Sharp.NativeBinaries": "2.0.324"},
                      "runtime": {"lib/net8.0/LibGit2Sharp.dll": {"assemblyVersion": "0.32.0.0"}}},
            NATIVE: {"native": {NATIVE_ASSET: {"fileVersion": "1.0.0.0"}}},
        }},
        "libraries": {MANAGED: {"type": "package"}, NATIVE: {"type": "package"}},
    }
    manifest = output / "Imlight.Director.deps.json"
    manifest.write_text(json.dumps(dependencies), encoding="utf-8")
    return output


def write_server_fixture(server: Path, git_version: str | None = None) -> Path:
    server.mkdir(parents=True)
    entries = {
        "Imlight.Director/1.0.0": {"dependencies": {"Imlight.CoreLib": "1.0.0"}},
        "Imlight.CoreLib/1.0.0": {"dependencies": {"Unrelated": "1.0.0"}},
        "Unrelated/1.0.0": {"runtime": {"Unrelated.dll": {}}},
    }
    libraries = {name: {"type": "project"} for name in entries}
    if git_version is not None:
        name = f"LibGit2Sharp/{git_version}"
        entries["Imlight.CoreLib/1.0.0"]["dependencies"]["LibGit2Sharp"] = git_version
        entries[name] = {"runtime": {"lib/net8.0/LibGit2Sharp.dll": {}}}
        libraries[name] = {"type": "package"}
    manifest = server / "Imlight.Director.deps.json"
    manifest.write_text(json.dumps({
        "runtimeTarget": {"name": TARGET + "/win-x64"},
        "targets": {TARGET + "/win-x64": entries}, "libraries": libraries,
    }), encoding="utf-8")
    return manifest


class ServerDependencyTests(unittest.TestCase):
    def test_missing_dependencies_are_installed_with_runtime_metadata(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            output = write_build_fixture(root)
            server = root / "server"
            manifest = write_server_fixture(server)
            original = json.loads(manifest.read_text())
            with patch.object(installer, "ROOT", root), patch.object(installer, "SERVER", server):
                installer.install_server_dependencies(output)
            installed = json.loads(manifest.read_text())
            target = installed["targets"][TARGET + "/win-x64"]
            self.assertEqual(target["Imlight.CoreLib/1.0.0"]["dependencies"]["LibGit2Sharp"], "0.32.0")
            self.assertEqual(target[MANAGED]["dependencies"]["LibGit2Sharp.NativeBinaries"], "2.0.324")
            self.assertIn(NATIVE_ASSET, target[NATIVE]["native"])
            self.assertEqual(target["Unrelated/1.0.0"], original["targets"][TARGET + "/win-x64"]["Unrelated/1.0.0"])
            self.assertEqual(installed["runtimeTarget"], original["runtimeTarget"])
            self.assertEqual((server / "LibGit2Sharp.dll").read_bytes(), b"managed dependency fixture")
            self.assertEqual((server / Path(NATIVE_ASSET).name).read_bytes(), b"native dependency fixture")

    def test_old_metadata_is_upgraded_and_setup_can_be_repeated(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            output = write_build_fixture(root)
            server = root / "server"
            manifest = write_server_fixture(server, "0.30.0")
            with patch.object(installer, "ROOT", root), patch.object(installer, "SERVER", server):
                installer.install_server_dependencies(output)
                first = manifest.read_bytes()
                installer.install_server_dependencies(output)
            self.assertEqual(manifest.read_bytes(), first)
            installed = json.loads(first)
            self.assertNotIn("LibGit2Sharp/0.30.0", installed["libraries"])
            for target in installed["targets"].values():
                self.assertNotIn("LibGit2Sharp/0.30.0", target)
                self.assertEqual(target["Imlight.CoreLib/1.0.0"]["dependencies"]["LibGit2Sharp"], "0.32.0")

    def test_missing_native_asset_fails_before_changing_the_server(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            output = write_build_fixture(root)
            (output / Path(NATIVE_ASSET).name).unlink()
            server = root / "server"
            manifest = write_server_fixture(server)
            original = manifest.read_bytes()
            with patch.object(installer, "ROOT", root), patch.object(installer, "SERVER", server):
                with self.assertRaises(FileNotFoundError):
                    installer.install_server_dependencies(output)
            self.assertEqual(manifest.read_bytes(), original)
            self.assertFalse((server / "LibGit2Sharp.dll").exists())


if __name__ == "__main__":
    unittest.main()
