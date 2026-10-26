import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetElixirSetBonus(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetElixirAndSetBonusService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetElixirAndSetBonusService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETELIXIRBUYREQUEST", svc)
        self.assertIn("MSG_PETELIXIROPEN", svc)
        self.assertIn("MSG_NEWTITLE", svc)
        self.assertIn("MSG_NOTIFYITEMFORSETBONUS", svc)
        self.assertIn("MSG_NOTIFYSCHOOLFOCUS", svc)

if __name__ == "__main__":
    unittest.main()
