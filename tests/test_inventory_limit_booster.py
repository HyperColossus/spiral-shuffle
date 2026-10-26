import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestInventoryLimitBooster(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(InventoryLimitAndBoosterService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/InventoryLimitAndBoosterService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BANKCOUNT", svc)
        self.assertIn("MSG_ATESSENCELIMIT", svc)
        self.assertIn("MSG_ATHANORARRIVAL", svc)
        self.assertIn("MSG_BOOSTERDISTRIBUTIONRESULTS", svc)
        self.assertIn("MSG_BADGESEGMENT", svc)
        self.assertIn("MSG_BLACKBALL", svc)
        self.assertIn("MSG_ACCESSPASSREJECTED", svc)
        self.assertIn("MSG_ADDQUESTFINDER", svc)
        self.assertIn("MSG_ARENA_ERROR", svc)

if __name__ == "__main__":
    unittest.main()
