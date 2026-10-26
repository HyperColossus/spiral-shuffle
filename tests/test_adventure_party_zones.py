import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestAdventurePartyZones(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdventurePartyZoneService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/AdventurePartyZoneService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PARTYREQUESTMEMBERZONES", svc)
        self.assertIn("MSG_PARTYZONEREQUEST", svc)
        self.assertIn("MSG_UPDATEADVENTUREPOWERCOOLDOWN", svc)
        self.assertIn("MSG_RECLAIMPARTYNAME", svc)

if __name__ == "__main__":
    unittest.main()
