import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class RaidsAndEventsTests(unittest.TestCase):
    def test_raid_event_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(RaidAndEventService)", factory)

    def test_raid_gate_and_events(self):
        svc = (SOURCE / "Game/Services/RaidAndEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RAIDGATEUNLOCKREQUEST", svc)
        self.assertIn("MSG_UPDATEEVENTCURRENCY1", svc)
        self.assertIn("MSG_UPDATEEVENTCURRENCY2", svc)
        self.assertIn("MSG_REQUESTJOINEVENTTEAM", svc)

if __name__ == "__main__":
    unittest.main()
