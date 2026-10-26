import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestTrainingPointRespec(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TrainingPointRespecAndRestoreService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/TrainingPointRespecAndRestoreService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RESPEC", svc)
        self.assertIn("MSG_RESPECCONFIRM", svc)
        self.assertIn("MSG_RESTORECHAR_RSP", svc)
        self.assertIn("MSG_REQUESTRESTORESTATE", svc)
        self.assertIn("MSG_REQUESTSENDAWAY", svc)

if __name__ == "__main__":
    unittest.main()
