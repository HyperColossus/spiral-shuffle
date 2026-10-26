import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCatchOfDayLuck(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CatchOfDayAndLuckService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CatchOfDayAndLuckService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CATCHOFTHEDAYOPEN", svc)
        self.assertIn("MSG_CSRADDFISH", svc)
        self.assertIn("MSG_CSRTRASHFISH", svc)
        self.assertIn("MSG_BENEFICIALLUCKUPDATE", svc)

if __name__ == "__main__":
    unittest.main()
