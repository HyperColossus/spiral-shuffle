import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class AccessPassTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AccessPassService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/AccessPassService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ACCESSPASSINFOREQUEST", svc)
        self.assertIn("MSG_ACCESSPASSOFFER", svc)
        self.assertIn("MSG_ACCESSPASSBUYREQUEST", svc)
        self.assertIn("MSG_ACCESSPASSBUYCONFIRM", svc)
        self.assertIn("MSG_ACCESSPASSPURCHASE", svc)
        self.assertIn("MSG_GETTIMEDACCESSPASSES", svc)
        self.assertIn("MSG_TIMEDACCESSPASSES", svc)
        self.assertIn("MSG_ACCESSPASSDECLINED", svc)

if __name__ == "__main__":
    unittest.main()
