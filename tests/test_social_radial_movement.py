import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestSocialRadialMovement(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SocialRadialAndMovementService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/SocialRadialAndMovementService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RADIALZONECLUSTERQUICKCHAT", svc)
        self.assertIn("MSG_RADIALZONECLUSTERQUICKCHATEXT", svc)
        self.assertIn("MSG_RADIALQUICKCHATEXT", svc)
        self.assertIn("MSG_REQUESTRADIALFRIENDQUICKCHAT", svc)
        self.assertIn("MSG_REQUESTRADIALOWNERCHAT", svc)
        self.assertIn("MSG_ADDPLAYERTOBASICCHATCHANNEL", svc)
        self.assertIn("MSG_REMOVEPLAYERFROMBASICCHATCHANNEL", svc)
        self.assertIn("MSG_INVITEALLTOBASICCHATCHANNEL", svc)
        self.assertIn("MSG_PLAYERTELEPORTINGTOFRIEND", svc)
        self.assertIn("MSG_TELEPORTTOSTART", svc)
        self.assertIn("MSG_MOVECORRECTION", svc)
        self.assertIn("MSG_MB_MOVE", svc)
        self.assertIn("MSG_MB_MOVESTATE", svc)

if __name__ == "__main__":
    unittest.main()
