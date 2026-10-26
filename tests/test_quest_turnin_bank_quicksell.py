import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestQuestTurnInBankQuickSell(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(QuestTurnInAndBankQuickSellService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/QuestTurnInAndBankQuickSellService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_QUESTREADYTOTURNIN", svc)
        self.assertIn("MSG_QUESTDIALOG", svc)
        self.assertIn("MSG_QUICKSELLREQUESTBANK", svc)
        self.assertIn("MSG_PREVIEW_ISLAND", svc)
        self.assertIn("MSG_PRELEADERBOARD", svc)
        self.assertIn("MSG_PREMIUMCONTENT", svc)

if __name__ == "__main__":
    unittest.main()
