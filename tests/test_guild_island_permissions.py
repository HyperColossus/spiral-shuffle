import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestGuildIslandPermissions(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GuildIslandPermissionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/GuildIslandPermissionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GUILDITEMCONTROL", svc)
        self.assertIn("MSG_UPDATEGUILDHOUSEFLAGS", svc)
        self.assertIn("MSG_PLAYERINGUILDCOOLDOWN", svc)
        self.assertIn("MSG_UPDATEGUILDNAME", svc)
        self.assertIn("MSG_REMOVEGUILDINVENTORYITEM", svc)
        self.assertIn("MSG_REQUESTGUILDOBJECT", svc)
        self.assertIn("MSG_UNLOADGUILDOBJECT", svc)

if __name__ == "__main__":
    unittest.main()
