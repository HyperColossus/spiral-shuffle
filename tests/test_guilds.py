import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class GuildsTests(unittest.TestCase):
    def test_guild_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GuildService)", factory)

    def test_guild_creation_and_membership(self):
        svc = (SOURCE / "Game/Services/GuildService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CREATEGUILDREQUEST", svc)
        self.assertIn("MSG_GUILDINFO", svc)

    def test_guild_house_and_museum(self):
        svc = (SOURCE / "Game/Services/GuildService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_VISITGUILDHOUSE", svc)
        self.assertIn("MSG_LOADGUILDOBJECT", svc)
        self.assertIn("MSG_REQUESTGUILDINVENTORY", svc)
        self.assertIn("MSG_REQUESTGUILDACHIEVEMENTS", svc)
        self.assertIn("MSG_REQUESTGUILDMUSEUMDOOR", svc)

if __name__ == "__main__":
    unittest.main()
