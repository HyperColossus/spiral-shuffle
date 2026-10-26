import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestArcadePhysics(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ArcadeAndPhysicsService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ArcadeAndPhysicsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SOBLOCKS_ADVANCEON", svc)
        self.assertIn("MSG_SOBLOCKS_ADVANCEOFF", svc)
        self.assertIn("MSG_SOBLOCKS_ATTACK", svc)
        self.assertIn("MSG_SOBLOCKS_COUNTDOWN", svc)
        self.assertIn("MSG_SOBLOCKS_FREEZEBLOCKS", svc)
        self.assertIn("MSG_SOBLOCKS_LEVELUP", svc)
        self.assertIn("MSG_SOBLOCKS_LOSS", svc)
        self.assertIn("MSG_SOBLOCKS_SENDROW", svc)
        self.assertIn("MSG_PHYSICS_FORCE", svc)
        self.assertIn("MSG_PHYSICS_FORCE_AT_POS", svc)
        self.assertIn("MSG_PHYSICS_GRAB", svc)
        self.assertIn("MSG_PHYSICS_RELEASE", svc)
        self.assertIn("MSG_PHYSICS_STATE", svc)

if __name__ == "__main__":
    unittest.main()
