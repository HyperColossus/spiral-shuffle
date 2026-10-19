"""Check that installation keeps server builds paired and preserves client files."""
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import setup as installer


class ServerInstallTests(unittest.TestCase):
    def test_replaces_launcher_and_library_without_overwriting_runtime_or_save(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            server = root / ".runtime/server"
            server.mkdir(parents=True)
            output = root / "director-build/bin/Release/net10.0"
            output.mkdir(parents=True)
            for name in ("Imlight.CoreLib.dll", "Imlight.Director.dll"):
                (server / name).write_bytes(b"incompatible installed assembly")
                (output / name).write_bytes(b"matching rebuilt assembly")
            preserved = {"Imlight.Director.exe": b"installed app host",
                         "Imlight.Director.deps.json": b"installed dependency manifest",
                         "Imlight.Director.runtimeconfig.json": b"installed runtime configuration",
                         "Config/Imlight.ini": b"existing server settings", "Saves/player.db": b"saved wizard"}
            for name, data in preserved.items():
                path = server / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(data)
            with patch.object(installer, "ROOT", root), patch.object(installer, "SERVER", server), \
                    patch.object(installer, "run") as build:
                installer.build_server("dotnet")
            build.assert_called_once_with("dotnet", "build", "director-build/Imlight.Director.csproj",
                                          "-c", "Release", "-v", "quiet")
            for name in ("Imlight.CoreLib.dll", "Imlight.Director.dll"):
                self.assertEqual((server / name).read_bytes(), b"matching rebuilt assembly")
            for name, data in preserved.items():
                self.assertEqual((server / name).read_bytes(), data)


class ClientInstallTests(unittest.TestCase):
    def test_separate_client_uses_patched_root_without_copying_preferences(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            revive = root / "revive"
            original = revive / "Aurorium/data" / installer.REVISION / "Data/GameData/Root.wad"
            original.parent.mkdir(parents=True)
            original.write_bytes(b"original game archive")
            source = root / "game"
            (source / "Bin").mkdir(parents=True)
            (source / "Bin/WizardGraphicalClient.exe").write_bytes(b"client fixture")
            (source / "Bin/preferences.xml").write_text("private preference fixture", encoding="utf-8")
            (source / "Bin/previouscommands.xml").write_text("private chat fixture", encoding="utf-8")
            runtime = root / "runtime"
            patched = runtime / "patch" / installer.REVISION / "Data/GameData/Root.wad"
            patched.parent.mkdir(parents=True)
            patched.write_bytes(b"patched game archive")
            client = runtime / "client"
            with patch.object(installer, "RUNTIME", runtime), patch.object(installer, "CLIENT", client):
                installer.install_client(revive, source)
            self.assertEqual(original.read_bytes(), b"original game archive")
            self.assertEqual((client / "Data/GameData/Root.wad").read_bytes(), b"patched game archive")
            self.assertTrue((client / "Bin/WizardGraphicalClient.exe").is_file())
            self.assertFalse((client / "Bin/preferences.xml").exists())
            self.assertFalse((client / "Bin/previouscommands.xml").exists())


if __name__ == "__main__":
    unittest.main()
