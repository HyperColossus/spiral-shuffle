import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestDyeShop(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DyeShopService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/DyeShopService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_FULLDYEREQUEST", svc)
        self.assertIn("MSG_SETSKINTONEMESSAGE", svc)
        self.assertIn("MSG_PATCHREMOVETEXTUREREMAP", svc)

if __name__ == "__main__":
    unittest.main()
