import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPlatformPromotions(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PlatformAndPromotionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PlatformAndPromotionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UPDATECLIENTPLATFORM", svc)
        self.assertIn("MSG_PLATFORMSYNCPURCHASES", svc)
        self.assertIn("MSG_PlatformSyncPurchasesErr", svc)
        self.assertIn("MSG_SetPlatformFriends", svc)
        self.assertIn("MSG_SetPlatformIgnore", svc)
        self.assertIn("MSG_REQUEST_PROMOTIONS", svc)
        self.assertIn("MSG_RECEIVE_PROMOTIONS", svc)
        self.assertIn("MSG_WEBCHARACTERINFO", svc)
        self.assertIn("MSG_WEB_AUTHEN", svc)
        self.assertIn("MSG_WEB_VALIDATE", svc)
        self.assertIn("MSG_CSREDITCHARACTER", svc)
        self.assertIn("MSG_CSREDITCHARACTERFAILED", svc)
        self.assertIn("MSG_CSRSEARCHLIST", svc)
        self.assertIn("MSG_CSRFINISHEDHOUSE", svc)
        self.assertIn("MSG_MUTE", svc)
        self.assertIn("MSG_NOTMUTED", svc)
        self.assertIn("MSG_BAN_RSP", svc)

if __name__ == "__main__":
    unittest.main()
