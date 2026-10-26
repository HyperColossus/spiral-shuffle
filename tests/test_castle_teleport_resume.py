import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCastleTeleportResume(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleTeleportAndResumeService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CastleTeleportAndResumeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSTELEPORTPLAYER", svc)
        self.assertIn("MSG_CASTLETOURSTELEPORTREJECTED", svc)
        self.assertIn("MSG_RESUMEINSTANCETELEPORTPLAYER", svc)
        self.assertIn("MSG_REQUESTHOUSETELEPORT", svc)
        self.assertIn("MSG_EXITCONFIRMTELEPORT", svc)
        self.assertIn("MSG_PETTELEPORT", svc)
        self.assertIn("MSG_TELEPORTEDTODELETEDLOT", svc)
        self.assertIn("MSG_EMPTYLOTCHECK", svc)
        self.assertIn("MSG_DILEVELUPELIXIRTELEPORT", svc)
        self.assertIn("MSG_PATCHBEFORETELEPORT", svc)
        self.assertIn("MSG_MB_TELEPORT", svc)

if __name__ == "__main__":
    unittest.main()
