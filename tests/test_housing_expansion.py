import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingExpansion(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingExpansionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingExpansionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ISLANDSPACEBUYREQUEST", svc)
        self.assertIn("MSG_UPDATEISLANDSPACE", svc)
        self.assertIn("MSG_UPDATEMAXIMUMHOUSINGITEMS", svc)

if __name__ == "__main__":
    unittest.main()
