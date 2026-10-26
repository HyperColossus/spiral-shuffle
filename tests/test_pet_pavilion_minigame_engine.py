import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetPavilionMinigameEngine(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetPavilionMinigameEngineService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetPavilionMinigameEngineService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETGAMEINIT", svc)
        self.assertIn("MSG_PETGAMEJOIN", svc)
        self.assertIn("MSG_PETGAMEJOINRSP", svc)
        self.assertIn("MSG_PETGAMEJUMP", svc)
        self.assertIn("MSG_PETGAMEMAZE", svc)
        self.assertIn("MSG_PETGAMEREADY", svc)
        self.assertIn("MSG_PETGAMESTART", svc)
        self.assertIn("MSG_PETGAMEENDING", svc)
        self.assertIn("MSG_PETGAMEINDIVIDUALRESULTS", svc)
        self.assertIn("MSG_PETGAMEDATA", svc)

if __name__ == "__main__":
    unittest.main()
