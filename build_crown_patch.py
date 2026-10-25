"""Stage a zero-Crown patch from an existing private patch; never install it."""
import argparse
import json
import subprocess
import zipfile
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path, help="Existing patch version directory")
    parser.add_argument("output", type=Path, help="New, empty staging directory")
    options = parser.parse_args()
    source, output = options.source.resolve(), options.output.resolve()
    if source == output or output.exists():
        raise ValueError("Output must be a new staging directory.")
    for name in ("Data/GameData/Root.wad", "LatestFileList.xml", "LatestFileList.bin"):
        if not (source / name).is_file():
            raise FileNotFoundError(name)
    output.mkdir(parents=True)
    root = Path(__file__).resolve().parent
    project = root / "crown-tool/CrownTool.csproj"
    command = ["dotnet", "run", "--project", str(project), "-c", "Release", "--"]
    subprocess.run([*command, "test"], check=True)
    subprocess.run([*command, "build", str(source / "Data/GameData/Root.wad"), str(output / "crown-overrides.zip")], check=True)
    import pack_assets
    with zipfile.ZipFile(output / "crown-overrides.zip") as archive:
        overrides = {name: archive.read(name) for name in archive.namelist()}
    metadata = pack_assets.build_wad(source / "Data/GameData/Root.wad", output / "Data/GameData/Root.wad", overrides)
    pack_assets.BASE, pack_assets.OUTPUT = source, output
    pack_assets.update_lists(metadata)
    subprocess.run([*command, "verify", str(output / "Data/GameData/Root.wad"), str(source / "Data/GameData/Root.wad")], check=True)
    (output / "crown-pricing-report.json").write_text(json.dumps({"CrownItemsChanged": len(overrides), "Metadata": metadata,
        "OnlyCrownPriceFieldsChanged": True}, indent=2) + "\n", encoding="utf-8")
    print("Staged and verified. No running game files or player data were modified.")


if __name__ == "__main__":
    main()
