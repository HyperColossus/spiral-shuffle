import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetDerbyMotionMorale(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetDerbyMotionAndMoraleService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetDerbyMotionAndMoraleService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETDERBYMODIFYSTAT", svc)
        self.assertIn("MSG_PETDERBYMORALE", svc)
        self.assertIn("MSG_PETDERBYPLAYERLEFT", svc)
        self.assertIn("MSG_PETDERBYSLOW", svc)
        self.assertIn("MSG_PETDERBYSWITCHLANEFAIL", svc)
        self.assertIn("MSG_PETGAMEDANCE", svc)

if __name__ == "__main__":
    unittest.main()
