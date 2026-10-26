import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestClientTelemetryDisplay(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ClientTelemetryAndDisplayService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ClientTelemetryAndDisplayService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LOGCLASSICMODE", svc)
        self.assertIn("MSG_LOGCLIENTRESOLUTION", svc)
        self.assertIn("MSG_LOGCSBATTLEREADYITEM", svc)
        self.assertIn("MSG_LOGPATCHCLIENTPATCHTIME", svc)
        self.assertIn("MSG_LOGVIDEOCAPABILITIES", svc)
        self.assertIn("MSG_CONNECTIONSTATS", svc)

if __name__ == "__main__":
    unittest.main()
