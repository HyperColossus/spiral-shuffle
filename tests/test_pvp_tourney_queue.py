import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPvPTourneyQueue(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PvPTourneyQueueService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PvPTourneyQueueService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PLAYERHOSTABNLETOURNEYLIST", svc)
        self.assertIn("MSG_PVPTOURNEYHOSTINGCREDIT", svc)
        self.assertIn("MSG_PVPQUEUE", svc)
        self.assertIn("MSG_PVPINTENT", svc)
        self.assertIn("MSG_PVPREFUNDPVPTOURNEYCURRENCY", svc)
        self.assertIn("MSG_REQUESTPVPACTOR", svc)

if __name__ == "__main__":
    unittest.main()
