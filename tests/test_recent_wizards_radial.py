import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestRecentWizardsRadial(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(RecentWizardsAndRadialChatService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/RecentWizardsAndRadialChatService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADDRECENTWIZARD", svc)
        self.assertIn("MSG_REQUESTRECENTWIZARDS", svc)
        self.assertIn("MSG_UPDATEDISABLECROSSPLAY", svc)
        self.assertIn("MSG_REQUESTRADIALADVENTURECHAT", svc)
        self.assertIn("MSG_REQUESTRADIALFRIENDCHAT", svc)
        self.assertIn("MSG_REQUESTRADIALTEAMUPCHAT", svc)
        self.assertIn("MSG_REQUESTZONECLUSTERQUICKCHAT", svc)

if __name__ == "__main__":
    unittest.main()
