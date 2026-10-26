import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCombatTimers(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CombatTimerService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CombatTimerService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMBATMOVESELECTION", svc)
        self.assertIn("MSG_SETPLANNINGPHASETIMER", svc)
        self.assertIn("MSG_SETDUELTIMER", svc)
        self.assertIn("MSG_SETSTATUS", svc)
        self.assertIn("MSG_UPDATEAFTERCOMBATDANCE", svc)

if __name__ == "__main__":
    unittest.main()
