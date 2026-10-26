import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestChatCodesWhisper(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ChatCodeAndWhisperService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ChatCodeAndWhisperService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTCHATCODE", svc)
        self.assertIn("MSG_SENDCHATCODE", svc)
        self.assertIn("MSG_USECHATCODE", svc)
        self.assertIn("MSG_REQUESTDIRECTEDCHATBYNAME", svc)
        self.assertIn("MSG_DIRECTEDCHATBYNAMERESPONSE", svc)
        self.assertIn("MSG_CHATFILTERBLACK", svc)
        self.assertIn("MSG_CHATFILTERWHITE", svc)
        self.assertIn("MSG_CHATERROR", svc)
        self.assertIn("MSG_CHATDIAGNOSTICS", svc)
        self.assertIn("MSG_REQCHATDIAGNOSTICS", svc)

if __name__ == "__main__":
    unittest.main()
