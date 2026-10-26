import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class TreasureCardPosterTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TreasureCardPosterService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/TreasureCardPosterService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADDTOTREASURECARDPOSTER", svc)
        self.assertIn("MSG_PATCHTREASURECARDPOSTER", svc)

if __name__ == "__main__":
    unittest.main()
