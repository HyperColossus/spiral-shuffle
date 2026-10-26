import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class GauntletTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GauntletService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/GauntletService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTCLASSPROJECTMONTHLYGAUNTLETINFO", svc)
        self.assertIn("MSG_MONTHLYGAUNTLETCREDITSCLAIMED", svc)
        self.assertIn("MSG_REQUESTHOUSINGZONE", svc)
        self.assertIn("MSG_REQUESTHOUSINGZONETELEPORT", svc)
        self.assertIn("MSG_HOUSINGZONEPLAYER", svc)
        self.assertIn("MSG_HOUSINGZONEPLAYERLIST", svc)

if __name__ == "__main__":
    unittest.main()
