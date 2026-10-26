import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestJewelUnlockTrade(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(JewelUnlockAndTradeService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/JewelUnlockAndTradeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_JEWELSOCKETUNLOCKCOST", svc)
        self.assertIn("MSG_ISLANDSPACESHOPOPEN", svc)
        self.assertIn("MSG_INVENTORYBEHAVIOR_TRADEITEM", svc)
        self.assertIn("MSG_ITEMIDENTIFIERINFO", svc)
        self.assertIn("MSG_ITEMLOCK", svc)

if __name__ == "__main__":
    unittest.main()
