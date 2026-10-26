import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class GuildRaidsTests(unittest.TestCase):
    def test_raid_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(RaidAndEventService)", factory)

    def test_raid_gate_unlocking_and_gates(self):
        rs = (SOURCE / "Game/Services/RaidAndEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RAIDGATEUNLOCKREQUEST", rs)
        self.assertIn("MSG_ZONEGATELIST", rs)

    def test_raid_team_registration(self):
        rs = (SOURCE / "Game/Services/RaidAndEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTJOINEVENTTEAM", rs)

    def test_raid_and_event_currencies(self):
        rs = (SOURCE / "Game/Services/RaidAndEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UPDATEEVENTCURRENCY1", rs)
        self.assertIn("MSG_UPDATEEVENTCURRENCY2", rs)

if __name__ == "__main__":
    unittest.main()
