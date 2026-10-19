"""Check that installing the client preserves the supplied files and skips preferences."""
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import setup as installer


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
