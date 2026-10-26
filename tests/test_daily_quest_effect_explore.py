import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestDailyQuestEffectExplore(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DailyQuestEffectAndExplorationService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/DailyQuestEffectAndExplorationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DAILYQUESTCSRDATA", svc)
        self.assertIn("MSG_DAILYQUESTEXPLORE", svc)
        self.assertIn("MSG_DAILYQUESTEFFECT", svc)
        self.assertIn("MSG_DECLINEQUEST", svc)
        self.assertIn("MSG_DEBUGQUESTDATA", svc)

if __name__ == "__main__":
    unittest.main()
