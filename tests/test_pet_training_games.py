import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetTrainingGames(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetTrainingGameService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetTrainingGameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETGAMEDROPOBJECT", svc)
        self.assertIn("MSG_PETGAMEDROPBONUS", svc)
        self.assertIn("MSG_PETMORPHSET", svc)
        self.assertIn("MSG_PETSNACKADD", svc)
        self.assertIn("MSG_PETSNACKREMOVE", svc)
        self.assertIn("MSG_PETTOMEPETADDED", svc)

if __name__ == "__main__":
    unittest.main()
