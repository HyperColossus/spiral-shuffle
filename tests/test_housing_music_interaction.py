import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingMusicInteraction(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingMusicAndInteractionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingMusicAndInteractionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_INTERACTIVEMUSICERROR", svc)
        self.assertIn("MSG_INTERACTIVEMUSICINUSE", svc)
        self.assertIn("MSG_INTERACTIVEMUSICINUSE2", svc)
        self.assertIn("MSG_INTERACTABLELOCKEDMESSAGE", svc)
        self.assertIn("MSG_INTERACTOBJECT", svc)
        self.assertIn("MSG_INTERACTCOMPLETEGOAL", svc)
        self.assertIn("MSG_INTERACTUNDERWAYQUEST", svc)

if __name__ == "__main__":
    unittest.main()
