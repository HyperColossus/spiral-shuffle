import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingPlacementPhysics(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingPlacementAndPhysicsService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingPlacementAndPhysicsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PICKUPALL", svc)
        self.assertIn("MSG_PICKUPOBJECT", svc)
        self.assertIn("MSG_PLACEOBJECT", svc)
        self.assertIn("MSG_PHYSICS_TORQUE", svc)
        self.assertIn("MSG_PING_RSP", svc)
        self.assertIn("MSG_PLAYTRAINSOUND", svc)

if __name__ == "__main__":
    unittest.main()
