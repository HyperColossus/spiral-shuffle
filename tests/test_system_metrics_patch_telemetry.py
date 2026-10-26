import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestSystemMetricsPatchTelemetry(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SystemMetricsAndPatchTelemetryService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/SystemMetricsAndPatchTelemetryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_NEWSYSSTATS", svc)
        self.assertIn("MSG_NEWTICKCNT", svc)
        self.assertIn("MSG_NEXT_VERSION", svc)
        self.assertIn("MSG_LATEST_FILE_LIST", svc)
        self.assertIn("MSG_PCS_CACHESEGREQSSUMMARY_REQUEST", svc)

if __name__ == "__main__":
    unittest.main()
