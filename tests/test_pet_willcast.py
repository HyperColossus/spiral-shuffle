import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetWillCast(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetWillCastAndHappinessService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetWillCastAndHappinessService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETWILLCAST", svc)
        self.assertIn("MSG_PETFEEDHAPPINESS", svc)
        self.assertIn("MSG_UPDATEOVERFLOWXP", svc)

if __name__ == "__main__":
    unittest.main()
