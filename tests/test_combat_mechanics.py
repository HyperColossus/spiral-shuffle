import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCombatMechanics(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CombatMechanicsService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CombatMechanicsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_AGGRO", svc)
        self.assertIn("MSG_AISMESSAGE", svc)
        self.assertIn("MSG_COMBATADD", svc)
        self.assertIn("MSG_COMBATREMOVE", svc)
        self.assertIn("MSG_COMBATPAUSED", svc)
        self.assertIn("MSG_COMBATCHEAT", svc)
        self.assertIn("MSG_COMBATLOADED", svc)
        self.assertIn("MSG_COMBATMATCHRESULT", svc)
        self.assertIn("MSG_COMBATPHASE", svc)
        self.assertIn("MSG_COMBATPHASEFORSPECTATORS", svc)
        self.assertIn("MSG_COMBATPIPS", svc)
        self.assertIn("MSG_COMBATREVEALHANGING", svc)
        self.assertIn("MSG_COMBATSTATS", svc)
        self.assertIn("MSG_COMBATUPFIRST", svc)
        self.assertIn("MSG_COMBATVICTORY", svc)
        self.assertIn("MSG_SHOWCOMBATUI", svc)
        self.assertIn("MSG_UPDATECOMBATPARTICIPANT", svc)

if __name__ == "__main__":
    unittest.main()
