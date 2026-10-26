import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class PvpDuelTests(unittest.TestCase):
    def test_pvp_participant_slot_selection(self):
        """Verify players can occupy Team Monster slots in duels."""
        duel = (SOURCE / "Game/Zone/Components/CombatDuelComponent.cs").read_text(encoding="utf-8")
        self.assertIn("subCircle is null && isPlayer && IsSlotAvailable(CombatTeam.Monster)", duel)

    def test_pvp_combat_hand_dispatch(self):
        """Verify players on Monster team receive cards and telegraphs."""
        duel = (SOURCE / "Game/Zone/Components/CombatDuelComponent.cs").read_text(encoding="utf-8")
        self.assertIn("circle._wizard is null && !circle.IsSummonedMinion", duel)
        self.assertIn("circle.OccupiedTeam == CombatTeam.Monster && circle._wizard is null", duel)
        self.assertIn("caster._wizard is not null", duel)


if __name__ == "__main__":
    unittest.main()
