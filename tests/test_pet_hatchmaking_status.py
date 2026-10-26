import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetHatchmakingStatus(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetHatchmakingStatusService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetHatchmakingStatusService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GAINPETXP", svc)
        self.assertIn("MSG_GETSNACKLIST", svc)
        self.assertIn("MSG_HATCHMAKINGMESSAGE", svc)
        self.assertIn("MSG_HATCHMAKINGSCSRRESULTS", svc)
        self.assertIn("MSG_HATCHMAKINGSEARCHRESULTS", svc)
        self.assertIn("MSG_HATCHMAKINGSTATUS", svc)

if __name__ == "__main__":
    unittest.main()
