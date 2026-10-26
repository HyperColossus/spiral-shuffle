import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestRealmAndBadge(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(RealmAndBadgeService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/RealmAndBadgeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_TRANSFER_REALMS", svc)
        self.assertIn("MSG_UPDATEREMEMBERLASTREALM", svc)
        self.assertIn("MSG_SETTESTREALMWATERMARK", svc)
        self.assertIn("MSG_REQUESTBADGELEADERBOARDFRIENDS", svc)
        self.assertIn("MSG_REQUESTBADGELEADERBOARDPAGE", svc)
        self.assertIn("MSG_REQUESTBADGELOOT", svc)
        self.assertIn("MSG_REQUESTBADGEREWARD", svc)

if __name__ == "__main__":
    unittest.main()
