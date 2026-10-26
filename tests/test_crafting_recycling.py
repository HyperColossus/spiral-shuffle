import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCraftingRecycling(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CraftingAndRecyclingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CraftingAndRecyclingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CRAFTINGSLOTADD", svc)
        self.assertIn("MSG_CRAFTINGSLOTREMOVE", svc)
        self.assertIn("MSG_CRAFTINGSLOTCOUNT", svc)
        self.assertIn("MSG_REQUESTRECYCLINGBIN", svc)

if __name__ == "__main__":
    unittest.main()
