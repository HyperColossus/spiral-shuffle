import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class GiftRedemptionTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GiftRedemptionService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/GiftRedemptionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_FULFILLPROMOCODE", svc)
        self.assertIn("MSG_REQUEST_GIFTS", svc)
        self.assertIn("MSG_REDEEM_GIFT", svc)
        self.assertIn("MSG_DELETE_GIFT", svc)
        self.assertIn("MSG_NOTIFY_GIFT", svc)

if __name__ == "__main__":
    unittest.main()
