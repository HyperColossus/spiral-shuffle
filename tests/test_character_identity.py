import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCharacterIdentity(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CharacterIdentityService)", factory)

    def test_identity_protocol_handlers(self):
        svc = (SOURCE / "Game/Services/CharacterIdentityService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTRENAMECHARACTER", svc)
        self.assertIn("MSG_CSRREQUESTRENAMEINFO", svc)
        self.assertIn("MSG_UPDATEGENDER", svc)

if __name__ == "__main__":
    unittest.main()
