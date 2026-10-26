import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class AdventurePartyTests(unittest.TestCase):
    def test_adventure_party_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdventurePartyService)", factory)

    def test_party_creation_and_joining(self):
        svc = (SOURCE / "Game/Services/AdventurePartyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTCREATEADVENTUREPARTY", svc)
        self.assertIn("MSG_REQUESTJOINADVENTUREPARTY", svc)
        self.assertIn("MSG_ADVENTUREPARTSETPURPOSE", svc)
        self.assertIn("MSG_ADVENTUREPARTYMESSAGE", svc)

    def test_pet_adventure_talents(self):
        svc = (SOURCE / "Game/Services/AdventurePartyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_USEPETADVENTUREPOWER", svc)
        self.assertIn("MSG_UPDATEADVENTUREPOWERCOOLDOWN", svc)


    def test_party_kiosk_and_management(self):
        svc = (SOURCE / "Game/Services/AdventurePartyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADVENTUREPARTYADDTOKIOSK", svc)
        self.assertIn("MSG_ADVENTUREPARTYASKTOJOIN", svc)
        self.assertIn("MSG_CHANGEPARTYNAME", svc)
        self.assertIn("MSG_CHANGEPARTYOWNER", svc)
        self.assertIn("MSG_REQUESTLEAVEADVENTUREPARTY", svc)
        self.assertIn("MSG_LEAVEADVENTUREPARTY", svc)

if __name__ == "__main__":
    unittest.main()
