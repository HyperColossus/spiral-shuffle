import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestQuestNavigation(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(QuestNavigationService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/QuestNavigationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UPDATEUSERHIDDENQUESTS", svc)
        self.assertIn("MSG_REQUESTNEXTCLOSESTQUEST", svc)
        self.assertIn("MSG_REQUESTQUESTDIALOG", svc)
        self.assertIn("MSG_REQUESTFALLBACK", svc)

if __name__ == "__main__":
    unittest.main()
