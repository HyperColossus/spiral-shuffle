import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestStorageInvoiceEssence(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(StorageInvoiceAndEssenceService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/StorageInvoiceAndEssenceService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DELIVERYINVOICETRANSFEROBJECTSFROMSTORAGETOBANK", svc)
        self.assertIn("MSG_DELETESETSWITHITEM", svc)
        self.assertIn("MSG_DISPLAYCOLLECTEDESSENCES", svc)
        self.assertIn("MSG_DISPLAYEQUIPMENTSLOWDOWN", svc)

if __name__ == "__main__":
    unittest.main()
