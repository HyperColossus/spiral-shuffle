import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class CastleMagicTests(unittest.TestCase):
    def test_castle_magic_action_binding_handled(self):
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICCHANGEACTION", housing)
        self.assertIn("ActionNumber = message.ActionNumber", housing)
        self.assertIn("SpellTemplateID = message.SpellTemplateID", housing)

    def test_castle_magic_triggers_and_use(self):
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICREQUESTUSE", housing)
        self.assertIn("MSG_CASTLEMAGICUPDATESTATE", housing)
        self.assertIn("GlobalID = message.ItemGID", housing)

    def test_castle_magic_orb_handled(self):
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICORB", housing)
        self.assertIn("OrbFlags = message.OrbFlags", housing)

    def test_castle_magic_player_teleport(self):
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICREQUESTPLAYERTELEPORT", housing)
        self.assertIn("MSG_SERVERTELEPORT", housing)


if __name__ == "__main__":
    unittest.main()
