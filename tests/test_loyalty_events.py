import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class LoyaltyEventsTests(unittest.TestCase):
    def test_loyalty_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LoyaltyService)", factory)

    def test_loyalty_store_and_crown_rewards(self):
        svc = (SOURCE / "Game/Services/LoyaltyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LOYALTYSTORELISTREQUEST", svc)
        self.assertIn("MSG_SHOPLOYALTYLIST", svc)
        self.assertIn("MSG_LOYALTYCLAIMLOYALTYTOKENS", svc)
        self.assertIn("MSG_LOYALTYCLAIMLOYALTYTOKENSCONFIRM", svc)
        self.assertIn("MSG_REQUESTCROWNSREWARDSEVENTS", svc)
        self.assertIn("MSG_ADDCROWNSREWARDSEVENT", svc)
        self.assertIn("MSG_LOYALTYSTORE_PURCHASE_LOCK_REQUEST", svc)
        self.assertIn("MSG_LOYALTYSTORE_PURCHASE_REQUEST", svc)
        self.assertIn("MSG_REQUESTEXPLICITLOYALTYSTOREUPDATE", svc)

    def test_whirlyburly_kiosk(self):
        svc = (SOURCE / "Game/Services/LoyaltyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_WHIRLYBURLYOPENKIOSK", svc)

if __name__ == "__main__":
    unittest.main()
