import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCrownRewardsEvents(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CrownRewardsEventService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CrownRewardsEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADDCROWNSREWARDSEVENT", svc)
        self.assertIn("MSG_CROWNSBUYREQUEST", svc)

if __name__ == "__main__":
    unittest.main()
