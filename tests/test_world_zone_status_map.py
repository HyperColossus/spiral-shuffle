import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestWorldZoneStatusMap(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(WorldZoneStatusAndMapService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/WorldZoneStatusAndMapService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LEMURIASTATUS", svc)
        self.assertIn("MSG_LAVALOCKSTATUS", svc)
        self.assertIn("MSG_KHANDANCESTATUS", svc)
        self.assertIn("MSG_MAPREQUIREMENTS", svc)
        self.assertIn("MSG_LEAVESTATE", svc)

if __name__ == "__main__":
    unittest.main()
