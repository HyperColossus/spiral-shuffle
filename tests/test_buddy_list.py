import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestBuddyList(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BuddyListService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/BuddyListService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SETINVISIBLETOFRIENDS", svc)
        self.assertIn("MSG_BUDDYDROP", svc)
        self.assertIn("MSG_BUDDYSTATUSUPDATE", svc)
        self.assertIn("MSG_BUDDYREQUESTACCEPTFWD", svc)
        self.assertIn("MSG_REQUESTMAXFRIENDS", svc)
        self.assertIn("MSG_DECORATORBUDDYREQUEST", svc)

if __name__ == "__main__":
    unittest.main()
