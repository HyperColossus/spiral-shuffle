import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class CantripsTests(unittest.TestCase):
    def test_cantrips_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CantripService)", factory)

    def test_cantrips_xp_and_level_progression(self):
        cantrip_svc = (SOURCE / "Game/Services/CantripService.cs").read_text(encoding="utf-8")
        self.assertIn("stats.m_cantripXP += xpGained", cantrip_svc)
        self.assertIn("MSG_UPDATECANTRIPXP", cantrip_svc)
        self.assertIn("MSG_CANTRIPLEVELUP", cantrip_svc)

    def test_cantrips_sigil_and_window(self):
        cantrip_svc = (SOURCE / "Game/Services/CantripService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CANTRIPSIGILSPELL", cantrip_svc)
        self.assertIn("MSG_CANTRIPNOAGGRO", cantrip_svc)
        self.assertIn("MSG_OPENCANTRIPWINDOW", cantrip_svc)


if __name__ == "__main__":
    unittest.main()
