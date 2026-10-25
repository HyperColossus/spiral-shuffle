import json
import unittest
from pathlib import Path
import subprocess

ROOT = Path("/home/hypercolossus/spiral-shuffle")
CACHE = Path("/home/hypercolossus/Revive101/Imlight/spiraldb-cache/CrownShop")


class CrownShopTests(unittest.TestCase):
    def test_crown_shop_cache_files(self):
        """Verify that all tab JSON definitions exist and are valid JSON."""
        self.assertTrue(CACHE.is_dir(), f"Cache directory {CACHE} must exist")
        tabs = list(CACHE.glob("Tab*.json"))
        self.assertGreaterEqual(len(tabs), 11, "Expected at least 11 tab configuration files")

        total_items = 0
        total_categories = 0
        for tab_file in tabs:
            with open(tab_file, "r", encoding="utf-8") as f:
                data = json.load(f)
            self.assertIn("tabId", data)
            self.assertIn("name", data)
            total_categories += len(data.get("categories", []))
            total_items += len(data.get("items", []))

        self.assertGreater(total_categories, 20, "Expected >20 categories across tabs")
        self.assertGreater(total_items, 1000, "Expected >1000 items in Crown Shop catalog")

    def test_crown_shop_source_integration(self):
        """Verify that CrownShop components are hooked into prepare_source.py and build outputs."""
        prep = (ROOT / "prepare_source.py").read_text(encoding="utf-8")
        self.assertIn("CrownShopService", prep)
        self.assertIn("CrownShopHandler", prep)
        self.assertIn("CrownShopModels", prep)

        built_core = ROOT / "server-build/bin/Release/net10.0/Imlight.CoreLib.dll"
        self.assertTrue(built_core.is_file(), "Imlight.CoreLib.dll must be built")

        built_dir = ROOT / "director-build/bin/Release/net10.0/Imlight.Director.dll"
        self.assertTrue(built_dir.is_file(), "Imlight.Director.dll must be built")

    def test_crown_shop_live_payload(self):
        """Verify that CrownShopHandler loads and serializes the catalog."""
        result = subprocess.run(
            ["dotnet", "run", "--project", "crown-tool/CrownTool.csproj", "--", "verify-crownshop"],
            cwd=str(ROOT),
            capture_output=True,
            text=True
        )
        self.assertEqual(result.returncode, 0, f"verify-crownshop failed: {result.stderr}\n{result.stdout}")
        self.assertIn("PASS: CrownShopHandler loaded", result.stdout)


if __name__ == "__main__":
    unittest.main()
