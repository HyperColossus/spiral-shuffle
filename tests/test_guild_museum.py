import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestGuildMuseum(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GuildMuseumService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/GuildMuseumService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTMUSEUMESSENCEDATA", svc)
        self.assertIn("MSG_UPDATEMUSEUMESSENCEDATA", svc)
        self.assertIn("MSG_UPDATEMUSEUMEXHIBIT", svc)
        self.assertIn("MSG_MOVEGUILDMUSEUMDOOR", svc)
        self.assertIn("MSG_CREATEGUILDTESTOBJECT", svc)
        self.assertIn("MSG_CREATETESTISLAND", svc)

if __name__ == "__main__":
    unittest.main()
