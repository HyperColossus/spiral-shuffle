import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class BattleCardsTests(unittest.TestCase):
    def test_battle_card_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BattleCardService)", factory)

    def test_battle_card_buy_and_shop(self):
        svc = (SOURCE / "Game/Services/BattleCardService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BATTLECARDBUY", svc)
        self.assertIn("MSG_BATTLECARDBUYCONFIRM", svc)
        self.assertIn("MSG_BATTLECARDSHOPLIST", svc)

    def test_polymorph_and_battlegrounds(self):
        svc = (SOURCE / "Game/Services/BattleCardService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BGSELECTPOLYMORPHREQUEST", svc)
        self.assertIn("MSG_BGSELECTPOLYMORPHRESPONSE", svc)
        self.assertIn("MSG_BGSELECTPOLYMORPHDONE", svc)
        self.assertIn("MSG_BGISPOLYMORPHUNLOCKEDREQUEST", svc)
        self.assertIn("MSG_BGISPOLYMORPHUNLOCKEDRESPONSE", svc)
        self.assertIn("MSG_BGPlayerStatsUpdate", svc)

if __name__ == "__main__":
    unittest.main()
