"""Test patch manifest generation to prevent client XML parsing regressions."""
import re
import struct
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import pack_assets


SAMPLE_XML = """<?xml version="1.0" ?>
<LatestFileList>
<_TableList>
  <RECORD>
    <Name TYPE="STR">Mob-WorldData</Name>
  </RECORD>
  <RECORD>
    <Name TYPE="STR">Root</Name>
  </RECORD>
</_TableList>
<Mob-WorldData>
  <RECORD>
    <SrcFileName TYPE="STR">Data/GameData/Mob-WorldData.wad</SrcFileName>
    <TarFileName TYPE="STR"></TarFileName>
    <FileType TYPE="UINT">5</FileType>
    <Size TYPE="UINT">1272454778</Size>
    <HeaderSize TYPE="UINT">2589250</HeaderSize>
    <CompressedHeaderSize TYPE="UINT">815242</CompressedHeaderSize>
    <CRC TYPE="UINT">2876964514</CRC>
    <HeaderCRC TYPE="UINT">789973058</HeaderCRC>
  </RECORD>
</Mob-WorldData>
<Root>
  <RECORD>
    <SrcFileName TYPE="STR">Data/GameData/Root.wad</SrcFileName>
    <TarFileName TYPE="STR"></TarFileName>
    <FileType TYPE="UINT">3</FileType>
    <Size TYPE="UINT">1000</Size>
    <HeaderSize TYPE="UINT">200</HeaderSize>
    <CompressedHeaderSize TYPE="UINT">50</CompressedHeaderSize>
    <CRC TYPE="UINT">12345</CRC>
    <HeaderCRC TYPE="UINT">67890</HeaderCRC>
  </RECORD>
</Root>
</LatestFileList>
"""


class PatchManifestTests(unittest.TestCase):
    def test_update_lists_no_self_closing_tags(self):
        with tempfile.TemporaryDirectory() as tmpdir:
            base_dir = Path(tmpdir) / "base"
            out_dir = Path(tmpdir) / "out"
            base_dir.mkdir()
            out_dir.mkdir()

            (base_dir / "LatestFileList.xml").write_text(SAMPLE_XML, encoding="utf-8")

            # Create a matching minimal binary file with Root.wad entry
            binary = bytearray(b"PREFIX_DATA")
            filename = b"Data/GameData/Root.wad"
            binary.extend(filename)
            binary.extend(struct.pack("<H6I", 0, 3, 1000, 200, 50, 12345, 67890))
            (base_dir / "LatestFileList.bin").write_bytes(binary)

            metadata = {
                "Size": 999999,
                "HeaderSize": 8888,
                "CompressedHeaderSize": 777,
                "CRC": 11111,
                "HeaderCRC": 22222,
            }

            with mock.patch.object(pack_assets, "BASE", base_dir), \
                 mock.patch.object(pack_assets, "OUTPUT", out_dir):
                pack_assets.update_lists(metadata)

            out_xml = (out_dir / "LatestFileList.xml").read_text(encoding="utf-8")
            self.assertTrue(out_xml.startswith("<?xml version=\"1.0\" ?>"))
            self.assertEqual(len(re.findall(r'<[a-zA-Z0-9]+[^>]*/>', out_xml)), 0)
            self.assertIn('<TarFileName TYPE="STR"></TarFileName>', out_xml)
            self.assertIn('<Size TYPE="UINT">999999</Size>', out_xml)
            self.assertIn('<HeaderSize TYPE="UINT">8888</HeaderSize>', out_xml)
            self.assertIn('<CompressedHeaderSize TYPE="UINT">777</CompressedHeaderSize>', out_xml)
            self.assertIn('<CRC TYPE="UINT">11111</CRC>', out_xml)
            self.assertIn('<HeaderCRC TYPE="UINT">22222</HeaderCRC>', out_xml)

            # Ensure Mob-WorldData was untouched
            self.assertIn('<Size TYPE="UINT">1272454778</Size>', out_xml)

            # Verify binary was updated
            out_bin = (out_dir / "LatestFileList.bin").read_bytes()
            expected_bin = struct.pack("<H6I", 0, 3, 999999, 8888, 777, 11111, 22222)
            self.assertIn(expected_bin, out_bin)

    def test_live_patch_manifest_has_zero_self_closing_tags(self):
        import profile_paths
        live_xml_path = profile_paths.PATCH_VERSION / "LatestFileList.xml"
        if not live_xml_path.exists():
            self.skipTest("Live patch XML not found")
        content = live_xml_path.read_text(encoding="utf-8")
        self.assertEqual(len(re.findall(r'<[a-zA-Z0-9]+[^>]*/>', content)), 0)
        self.assertTrue(content.startswith("<?xml version=\"1.0\" ?>"))


if __name__ == "__main__":
    unittest.main()