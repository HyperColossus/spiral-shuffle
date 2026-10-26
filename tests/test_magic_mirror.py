import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class MagicMirrorTests(unittest.TestCase):
    def test_magic_mirror_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(MagicMirrorService)", factory)

    def test_magic_mirror_handlers(self):
        svc = (SOURCE / "Game/Services/MagicMirrorService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MAGICMIRRORSHOPOPEN", svc)
        self.assertIn("MSG_MAGICMIRRORREQUESTCHANGE", svc)
        self.assertIn("MSG_MAGICMIRRORCHANGEPLAYER", svc)

if __name__ == "__main__":
    unittest.main()
