import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCantripMagicClear(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CantripAndMagicClearService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CantripAndMagicClearService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICCLEAR", svc)
        self.assertIn("MSG_PATCHCASTLEMAGIC", svc)
        self.assertIn("MSG_CANTRIPSRESPONSEERROR", svc)
        self.assertIn("MSG_CANTRIPTUTORIALEFFECT", svc)

if __name__ == "__main__":
    unittest.main()
