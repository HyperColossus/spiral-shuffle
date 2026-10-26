import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPvPBrackets(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PvPTournamentBracketService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PvPTournamentBracketService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BRACKETREPORT", svc)
        self.assertIn("MSG_SUBOPTIMAL_BRACKET_RESPONSE", svc)
        self.assertIn("MSG_NEWTOURNEYREWARDS", svc)
        self.assertIn("MSG_FREETOURNEYCREDITINFO", svc)
        self.assertIn("MSG_PVPUPDATEINFO", svc)
        self.assertIn("MSG_PREPVPKIOSK", svc)
        self.assertIn("MSG_PVPCONSUMEPVPTOURNEYCURRENCY", svc)

if __name__ == "__main__":
    unittest.main()
