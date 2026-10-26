import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetLeashLending(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetLeashAndLendingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetLeashAndLendingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LEASH", svc)
        self.assertIn("MSG_LEASHOFFSET", svc)
        self.assertIn("MSG_LENTPET", svc)
        self.assertIn("MSG_LOCKLEVEL", svc)
        self.assertIn("MSG_LOADSAVEOBJECTS", svc)

if __name__ == "__main__":
    unittest.main()
