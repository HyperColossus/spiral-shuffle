import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class ChatChannelsTests(unittest.TestCase):
    def test_chat_channel_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ChatChannelService)", factory)

    def test_chat_channel_handlers(self):
        svc = (SOURCE / "Game/Services/ChatChannelService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTCREATECHANNEL", svc)
        self.assertIn("MSG_REQUESTJOINCHANNEL", svc)
        self.assertIn("MSG_REQUESTJOINCHANNELBYNAME", svc)
        self.assertIn("MSG_REQUESTLEAVECHANNEL", svc)
        self.assertIn("MSG_JOINBASICCHATCHANNEL", svc)
        self.assertIn("MSG_INVITETOBASICCHATCHANNEL", svc)

if __name__ == "__main__":
    unittest.main()
