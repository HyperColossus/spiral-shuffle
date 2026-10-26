import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetHatchStatusFeeding(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetHatchStatusAndFeedingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetHatchStatusAndFeedingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETGAMEKIOSK", svc)
        self.assertIn("MSG_PETGAMESKIPFEED", svc)
        self.assertIn("MSG_PETGAMESNACKFEEDFAILED", svc)
        self.assertIn("MSG_PETHATCHJOINSTATUS", svc)
        self.assertIn("MSG_PETHATCHREADYSTATUS", svc)
        self.assertIn("MSG_PETMORPHCANAFFORD", svc)
        self.assertIn("MSG_PETMORPHREADY", svc)
        self.assertIn("MSG_PETPOWERWORLDFAILURE", svc)
        self.assertIn("MSG_PREVENTPLAYASPET", svc)

if __name__ == "__main__":
    unittest.main()
