import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class DungeonSigilsTests(unittest.TestCase):
    def test_zone_service_sigil_lifecycle(self):
        zs = (SOURCE / "Game/Services/ZoneService.cs").read_text(encoding="utf-8")
        # Step on pad -> countdown timer starts
        self.assertIn("MSG_STARTSIGILENTRY", zs)
        self.assertIn("MSG_MINIGAMETIMERSTART", zs)
        # Step off pad -> countdown cancelled
        self.assertIn("MSG_LEAVESIGILTIMERWAITING", zs)
        self.assertIn("MSG_MINIGAMETIMEREND", zs)
        # Timer fires -> instance transfer
        self.assertIn("MSG_SIGILENTER", zs)
        self.assertIn("MSG_ZONETRANSFER", zs)
        self.assertIn("ResetInstance = true", zs)
        self.assertIn("IsPrivate = true", zs)

    def test_dungeon_quests_force_granted(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("TryGrantDungeonQuests", qs)
        self.assertIn("RemoveDungeonQuestsOutsideZone", qs)
        self.assertIn("IsZoneDungeonQuest", qs)
        self.assertIn("MSG_INSTANCEGOALCOMPLETED", qs)

    def test_sigil_inspection_and_slots(self):
        rs = (SOURCE / "Game/Services/RaidAndEventService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTSIGILINFO", rs)
        self.assertIn("MSG_SETSIGILSLOT", rs)
        self.assertIn("MSG_REQUESTCOMBATSIGILS", rs)
        self.assertIn("MSG_SUBMITCOMBATSIGILS", rs)

    def test_group_sigil_coordination(self):
        gs = (SOURCE / "Game/Services/GroupService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTSIGILSLOT", gs)
        self.assertIn("MSG_GotoPlayerConfirm", gs)
        self.assertIn("MSG_JOINRESERVEDDUEL", gs)

if __name__ == "__main__":
    unittest.main()
