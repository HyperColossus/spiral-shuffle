import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestAdventureKioskFocus(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdventureKioskAndFocusService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/AdventureKioskAndFocusService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADVENTUREPARTASKADDTOKIOSK", svc)
        self.assertIn("MSG_CANSEETIEREDSPELLGROUPS", svc)
        self.assertIn("MSG_CHANGECHARACTERNAME", svc)
        self.assertIn("MSG_CHEERCOSTMAPUPDATE", svc)
        self.assertIn("MSG_CHOOSEFOCUS", svc)
        self.assertIn("MSG_CREATESETFAILED", svc)

if __name__ == "__main__":
    unittest.main()
