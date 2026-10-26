import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class ClientServiceTests(unittest.TestCase):
    def test_client_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ClientService)", factory)

    def test_req_ask_server_handled(self):
        client_svc = (SOURCE / "Game/Services/ClientService.cs").read_text(encoding="utf-8")
        self.assertIn("ReceiveReqServer", client_svc)
        self.assertIn("MSG_REQASKSERVER", client_svc)
        self.assertIn("RequirementDispatcher.EvaluateRequirements", client_svc)
        self.assertIn("RequestID = message.RequestID", client_svc)
        self.assertIn("Response = response", client_svc)

    def test_item_finder_handled(self):
        client_svc = (SOURCE / "Game/Services/ClientService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_OPENITEMFINDER", client_svc)
        self.assertIn("MSG_ITEMFINDERREQUESTBANK", client_svc)
        self.assertIn("MSG_ITEMFINDERREQUESTHOUSE", client_svc)
        self.assertIn("MSG_ITEMFINDERHOUSECOMPLETE", client_svc)


if __name__ == "__main__":
    unittest.main()
