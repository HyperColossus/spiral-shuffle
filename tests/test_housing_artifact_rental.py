import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingArtifactRental(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingArtifactAndRentalService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingArtifactAndRentalService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RENTALUPDATE", svc)
        self.assertIn("MSG_REQUESTARTIFACTLOCATION", svc)
        self.assertIn("MSG_REMOVEGUILDACHIEVEMENT", svc)
        self.assertIn("MSG_REMOVESPELLFUSIONFROMDECK", svc)
        self.assertIn("MSG_RELEASEDELAY", svc)
        self.assertIn("MSG_RESTOREORIGINALCASTLE", svc)

if __name__ == "__main__":
    unittest.main()
