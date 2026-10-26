import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestAdvancedPetBreeding(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdvancedPetBreedingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/AdvancedPetBreedingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_HATCHMAKINGSREQUESTPREMIUMPETEXEMPTIONS", svc)
        self.assertIn("MSG_HATCHMAKINGSUPDATEHATCHDAYPET2", svc)
        self.assertIn("MSG_REQUESTINPERSONHATCHINGCHANGE", svc)
        self.assertIn("MSG_INPERSONHATCHINGCHECK", svc)
        self.assertIn("MSG_PETTOMESCANATTIC", svc)
        self.assertIn("MSG_REQUESTPETTOME", svc)
        self.assertIn("MSG_MONSTERMAGICUSETOME", svc)

if __name__ == "__main__":
    unittest.main()
