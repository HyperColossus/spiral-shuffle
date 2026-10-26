import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestLoyaltySpiralSession(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LoyaltySpiralAndSessionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/LoyaltySpiralAndSessionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LOYALTYPROGRAMFORDAILYSPIRAL", svc)
        self.assertIn("MSG_JSBISFRIENDSWITHPLAYER", svc)
        self.assertIn("MSG_INVITE_RESPONSE", svc)
        self.assertIn("MSG_JOIN_SESSION", svc)
        self.assertIn("MSG_COMMANDRESULT", svc)

if __name__ == "__main__":
    unittest.main()
