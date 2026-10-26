import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class FishMarketTests(unittest.TestCase):
    def test_fish_market_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FishMarketService)", factory)

    def test_fish_market_handlers(self):
        svc = (SOURCE / "Game/Services/FishMarketService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SELLFISHOPEN", svc)
        self.assertIn("MSG_SELLFISHREQUEST", svc)
        self.assertIn("MSG_FISHTOURNAMENTLEADERBOARDREQUEST", svc)
        self.assertIn("MSG_REQUESTALLFISH", svc)
        self.assertIn("MSG_REQUESTFISHHISTORY", svc)

if __name__ == "__main__":
    unittest.main()
