import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestDungeonSocial(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DungeonInstanceAndSocialService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/DungeonInstanceAndSocialService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADDZONETIMER", svc)
        self.assertIn("MSG_REMOVEZONETIMER", svc)
        self.assertIn("MSG_UPDATEZONETIMER", svc)
        self.assertIn("MSG_ZONEEVENTTIMEREXPIRED", svc)
        self.assertIn("MSG_UPDATEZONECOUNTER", svc)
        self.assertIn("MSG_TELEPORT_TO_GAME_ZONE", svc)
        self.assertIn("MSG_INVITEFRIEND", svc)
        self.assertIn("MSG_SETFRIENDLYPLAYER", svc)
        self.assertIn("MSG_UPDATEFRIENDLYPLAYERWORLDS", svc)
        self.assertIn("MSG_UPDATEFRIENDLYWORLD", svc)
        self.assertIn("MSG_BUDDYREQUESTDENYFWD", svc)
        self.assertIn("MSG_CHECK_MAIL", svc)
        self.assertIn("MSG_MAIL_DATA", svc)
        self.assertIn("MSG_NEW_MAIL", svc)
        self.assertIn("MSG_RETRIEVE_MAIL", svc)
        self.assertIn("MSG_REDEEM_MAIL_GIFT", svc)

if __name__ == "__main__":
    unittest.main()
