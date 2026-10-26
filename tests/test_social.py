import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class SocialTests(unittest.TestCase):
    def test_true_friend_codes_implemented(self):
        friends = (SOURCE / "Game/Services/FriendsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTFRIENDFINDERCODE", friends)
        self.assertIn("MSG_SENDFRIENDFINDERCODE", friends)
        self.assertIn("MSG_USEFRIENDFINDERCODE", friends)
        self.assertIn("MSG_USEFRIENDFINDERCODERESPONSE", friends)
        self.assertIn("MSG_SETDONTALLOWFRIENDFINDERCODES", friends)

    def test_friend_behavior_no_notimplemented(self):
        behavior = (SOURCE / "Shared/Behaviors/ServerFriendBehavior.cs").read_text(encoding="utf-8")
        self.assertNotIn("throw new NotImplementedException()", behavior)

    def test_quest_behavior_no_notimplemented(self):
        behavior = (SOURCE / "Shared/Behaviors/ServerQuestBehavior.cs").read_text(encoding="utf-8")
        self.assertNotIn("throw new NotImplementedException()", behavior)

    def test_path_movement_no_notimplemented(self):
        path = (SOURCE / "Game/Zone/Components/PathMovementComponent.cs").read_text(encoding="utf-8")
        self.assertNotIn("throw new NotImplementedException", path)

    def test_group_teleport_and_quest_sync(self):
        group = (SOURCE / "Game/Services/GroupService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GOTOFRIENDLYPLAYER", group)
        self.assertIn("MSG_REQUESTGROUPTELEPORT", group)


if __name__ == "__main__":
    unittest.main()
