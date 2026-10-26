import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestBankExtensions(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BankExtensionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/BankExtensionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ITEMOVERFLOWTOBANK", svc)
        self.assertIn("MSG_AUCTIONREQUESTBANK", svc)
        self.assertIn("MSG_DELIVERYINVOICEREQUESTBANK", svc)
        self.assertIn("MSG_SHAREDBANKDELETEREAGENTORPETSNACK", svc)

if __name__ == "__main__":
    unittest.main()
