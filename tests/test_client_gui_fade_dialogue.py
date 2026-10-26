import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestClientGuiFadeDialogue(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ClientGuiFadeAndDialogueService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ClientGuiFadeAndDialogueService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DISPLAYHELPGUI", svc)
        self.assertIn("MSG_DISPLAYMESSAGE", svc)
        self.assertIn("MSG_FADECONTROL", svc)
        self.assertIn("MSG_HIDEGUI", svc)
        self.assertIn("MSG_CORE_PIIRADIALMENUEMOTE", svc)
        self.assertIn("MSG_ENDORSEINFO", svc)

if __name__ == "__main__":
    unittest.main()
