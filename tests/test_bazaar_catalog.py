import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestBazaarCatalog(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BazaarCatalogService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/BazaarCatalogService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_AUCTIONHOUSEMOREACKNOWLEDGEMENT", svc)
        self.assertIn("MSG_AUCTIONHOUSECSRMESSAGE", svc)
        self.assertIn("MSG_UPDATEGOLD", svc)
        self.assertIn("MSG_AUCTIONRESPONSE", svc)

if __name__ == "__main__":
    unittest.main()
