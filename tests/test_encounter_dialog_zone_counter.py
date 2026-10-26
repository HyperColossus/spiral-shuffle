import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestEncounterDialogZoneCounter(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(EncounterDialogAndZoneCounterService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/EncounterDialogAndZoneCounterService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DISPOSABLEDUNGEONNOOWNER", svc)
        self.assertIn("MSG_DISPLAYZONECOUNTERS", svc)
        self.assertIn("MSG_ENCOUNTERDIALOG", svc)
        self.assertIn("MSG_ELIXIRSTATECHANGE", svc)
        self.assertIn("MSG_DISMISS_SUMMON", svc)
        self.assertIn("MSG_DISCONNECT_AFK", svc)

if __name__ == "__main__":
    unittest.main()
