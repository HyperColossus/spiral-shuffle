import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPetDerbyRaceTrack(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetDerbyRaceTrackService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PetDerbyRaceTrackService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DERBYEFFECTAPPLY", svc)
        self.assertIn("MSG_DERBYEFFECTSLISTUPDATE", svc)
        self.assertIn("MSG_DERBYLOCATION", svc)
        self.assertIn("MSG_DERBYSTATMOD", svc)
        self.assertIn("MSG_DERBYSYNC", svc)

if __name__ == "__main__":
    unittest.main()
