import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPackOdds(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PackOddsService)", factory)

    def test_pack_odds_handlers(self):
        svc = (SOURCE / "Game/Services/PackOddsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTPACKODDS", svc)
        self.assertIn("MSG_CREATEBOOSTERDISTRIBUTION", svc)

if __name__ == "__main__":
    unittest.main()
