import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestFishTournamentBeneficial(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FishTournamentAndBeneficialService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/FishTournamentAndBeneficialService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ENDFISHINGCAST", svc)
        self.assertIn("MSG_ENTERBENEFICIALOBJECT", svc)
        self.assertIn("MSG_ENTERTOURNAMENTFISH", svc)
        self.assertIn("MSG_ENTERTOURNAMENTFISHRESULT", svc)
        self.assertIn("MSG_ENERGYBUYREQUEST", svc)

if __name__ == "__main__":
    unittest.main()
