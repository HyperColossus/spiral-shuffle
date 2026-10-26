import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class SorceryStonesTests(unittest.TestCase):
    def test_sorcery_stones_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SorceryStonesService)", factory)

    def test_sorcery_stones_handlers(self):
        svc = (SOURCE / "Game/Services/SorceryStonesService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SOBLOCKS_SELECTGAME", svc)
        self.assertIn("MSG_SOBLOCKS_STARTSWAP", svc)
        self.assertIn("MSG_SOBLOCKS_REQUESTROW", svc)
        self.assertIn("MSG_SOBLOCKS_PAUSE", svc)
        self.assertIn("MSG_SOBLOCKS_RESETGAME", svc)
        self.assertIn("MSG_SOBLOCKS_ENDGAME", svc)

if __name__ == "__main__":
    unittest.main()
