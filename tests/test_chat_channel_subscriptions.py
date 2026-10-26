import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestChatChannelSubscriptions(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ChatChannelSubscriptionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ChatChannelSubscriptionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_NOTIFYCHANNELINSTANCE", svc)
        self.assertIn("MSG_REQUESTADVANCECHANNELINSTANCE", svc)
        self.assertIn("MSG_SETBASICCHATCHANNELPUBLIC", svc)
        self.assertIn("MSG_REQUESTLEAVECHANNELBYNAME", svc)
        self.assertIn("MSG_CHANNELSUBSCRIPTION", svc)
        self.assertIn("MSG_CHECKSUBSCRIPTION", svc)
        self.assertIn("MSG_PERIODICSUBSCRIPTIONS", svc)

if __name__ == "__main__":
    unittest.main()
