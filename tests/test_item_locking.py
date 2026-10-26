import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestItemLocking(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ItemLockService)", factory)

    def test_locking_protocol_handlers(self):
        svc = (SOURCE / "Game/Services/ItemLockService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTTOGGLELOCKITEM", svc)
        self.assertIn("MSG_REMOVEITEMLOCKS", svc)
        self.assertIn("MSG_JEWELSOCKETLOCKUPDATE", svc)
        self.assertIn("MSG_PUBLICJEWELSOCKETUPDATE", svc)

if __name__ == "__main__":
    unittest.main()
