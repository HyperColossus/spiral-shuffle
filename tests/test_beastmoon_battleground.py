import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestBeastmoonBattleground(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BeastmoonBattlegroundService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/BeastmoonBattlegroundService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BGOPENPOLYMORPHSELECT", svc)
        self.assertIn("MSG_BGPOIUpdate", svc)
        self.assertIn("MSG_BATTLEGROUNDSHOWCOMPASS", svc)
        self.assertIn("MSG_REQUESTBATTLEGROUNDSLOOT", svc)
        self.assertIn("MSG_SetBGPolymorphLevel", svc)
        self.assertIn("MSG_BGPlayerSync", svc)
        self.assertIn("MSG_BGPlayerSyncStatus", svc)
        self.assertIn("MSG_BGQueueStatus", svc)
        self.assertIn("MSG_BGResetStats", svc)
        self.assertIn("MSG_BGSigilProxyEvent", svc)

if __name__ == "__main__":
    unittest.main()
