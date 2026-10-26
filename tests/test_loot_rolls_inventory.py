import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestLootRollsInventory(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LootRollAndInventoryService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/LootRollAndInventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ITEMDROP", svc)
        self.assertIn("MSG_TOURNAMENTLOOTROLLINFO", svc)
        self.assertIn("MSG_TOURNAMENTLOOTROLLRESPONSE", svc)
        self.assertIn("MSG_TOURNAMENTLOOTROLLRESULT", svc)
        self.assertIn("MSG_ADDITEMREQUEST", svc)
        self.assertIn("MSG_REMOVEITEMREQUEST", svc)

if __name__ == "__main__":
    unittest.main()
