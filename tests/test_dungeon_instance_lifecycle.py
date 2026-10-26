import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestDungeonInstanceLifecycle(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DungeonInstanceLifecycleService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/DungeonInstanceLifecycleService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CLEARRESUMEINSTANCE", svc)
        self.assertIn("MSG_COMPLETEINSTANCE", svc)
        self.assertIn("MSG_COMPLETEINSTANCE2", svc)
        self.assertIn("MSG_CLIENTZONED", svc)
        self.assertIn("MSG_CANACQUIREWORLDELIXIR", svc)
        self.assertIn("MSG_CLEARALLINTERACTIONS", svc)
        self.assertIn("MSG_COMPANIONEFFECTS", svc)
        self.assertIn("MSG_CONTROLVISIBLITY", svc)

if __name__ == "__main__":
    unittest.main()
