import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestEndorsements(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(EndorsementService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/EndorsementService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ENDORSEMENTREQUEST", svc)
        self.assertIn("MSG_ENDORSEMENTACK", svc)
        self.assertIn("MSG_REQUESTENDORSEMENTSCORE", svc)
        self.assertIn("MSG_UPDATEREALLOWENDORSEMENT", svc)

if __name__ == "__main__":
    unittest.main()
