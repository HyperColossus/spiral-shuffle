import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / ".build/source"

class GuildMuseumUnlocksTests(unittest.TestCase):
    def test_guild_museum_and_unlock_handlers(self):
        svc = (SOURCE / "Game/Services/GuildService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTMUSEUMCURATOR", svc)
        self.assertIn("MSG_REQUESTGUILDUNLOCKDATA", svc)
        self.assertIn("MSG_GUILDUNLOCKREQUEST", svc)
        self.assertIn("MSG_UPDATEGUILDUNLOCKTRIGGER", svc)

if __name__ == "__main__":
    unittest.main()
