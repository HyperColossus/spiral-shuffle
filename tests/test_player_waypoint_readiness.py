import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPlayerWaypointReadiness(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PlayerWaypointAndReadinessService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PlayerWaypointAndReadinessService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PLAYERARRIVED", svc)
        self.assertIn("MSG_PLAYERREADYACK", svc)
        self.assertIn("MSG_PLAYERBLACKBALLEDALERT", svc)
        self.assertIn("MSG_PIIUPDATEEMOTESRADIALMENU", svc)
        self.assertIn("MSG_PROMOTEPROVISIONALPLAYER", svc)
        self.assertIn("MSG_PVPCONFIRM", svc)

if __name__ == "__main__":
    unittest.main()
