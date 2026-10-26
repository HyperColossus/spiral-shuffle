import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingTexturePet(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingTextureAndPetService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingTextureAndPetService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PICKUPHOUSINGTEXTURE", svc)
        self.assertIn("MSG_SENDHOUSINGTEXTUREINFO", svc)
        self.assertIn("MSG_UPDATEHOUSINGPET", svc)
        self.assertIn("MSG_HOUSINGGAMEREQUESTPOWERUPLOCATIONS", svc)
        self.assertIn("MSG_SETHOUSINGGAMESTATE", svc)

if __name__ == "__main__":
    unittest.main()
