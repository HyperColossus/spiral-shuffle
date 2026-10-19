"""Build a compact enemy tooltip from native combat parchment and fonts."""
import copy
import xml.etree.ElementTree as ET
from pathlib import Path

from build_menu import add_control, set_value


ROOT = Path(__file__).resolve().parent
WIDTH = 410
HEIGHT = 370


def native_color(rgb: str) -> str:
    return rgb[:2] + rgb[6:8] + rgb[4:6] + rgb[2:4]


def add_parchment(panel: ET.Element, prototype: ET.Element) -> None:
    positions_x = (0, 18, WIDTH - 18, WIDTH)
    positions_y = (0, 18, HEIGHT - 18, HEIGHT)
    texture_x = (0, 0.125, 0.875, 1)
    texture_y = (0, 0.25, 0.75, 1)
    for row in range(3):
        for column in range(3):
            sprite = copy.deepcopy(prototype)
            for child in sprite.findall("m_Children"):
                sprite.remove(child)
            values = {
                "m_sName": f"enemyPaper{row}{column}",
                "m_Style": "HAS_NO_BORDER|DO_NOT_CAPTURE_MOUSE",
                "m_Flags": "VISIBLE",
                "m_Window": f"{positions_x[column]},{positions_y[row]},{positions_x[column + 1]},{positions_y[row + 1]}",
                "m_Color": "FFFFFFFF",
            }
            for field, value in values.items():
                set_value(sprite, field, value)
            sprite.find(".//m_sFilename").text = "GUI/Backgrounds/Background_Combat_Name.dds"
            sprite.find(".//m_Frames").text = f"{texture_x[column]},{texture_y[row]},{texture_x[column + 1]},{texture_y[row + 1]}"
            add_control(panel, sprite)


def add_text(panel: ET.Element, prototype: ET.Element, name: str, value: str, rectangle: str,
             scale: str = "0.53", color: str = "FF35221B", fancy: bool = False) -> None:
    control = copy.deepcopy(prototype)
    values = {"m_sName": name, "m_Style": "HAS_NO_BORDER|DO_NOT_CAPTURE_MOUSE|IS_TRANSPARENT",
              "m_Flags": "VISIBLE", "m_Window": rectangle, "m_sText": value,
              "m_bUseDropShadow": "0", "m_bEnableWordWrap": "0", "m_ParentOffset": "0,0,0,0"}
    for field, content in values.items():
        set_value(control, field, content)
    control.find(".//m_fScale").text = scale
    control.find(".//m_sFilename").text = "Fonts/WizardFancy.xml" if fancy else "Fonts/WizardNormal.xml"
    control.find(".//m_pWindowStyle/Class/m_Color").text = native_color(color)
    add_control(panel, control)


def main() -> None:
    source = ET.parse(ROOT / ".build/research/CharacterSelectWindow.gui.bin")
    controls = {node.findtext("m_sName"): node for node in source.iter("Class") if node.find("m_sName") is not None}
    tooltip = ET.parse(ROOT / ".build/research/ToolTip.gui")
    text = next(node for node in tooltip.iter("Class") if node.findtext("m_sName") == "ToolTipText")
    document = ET.Element("Objects")
    panel = ET.SubElement(document, "Class", {"Name": "class Window"})
    for name, value in {"m_sName": "RandomizerEnemyInspector", "m_Style": "HAS_NO_BORDER|DO_NOT_CAPTURE_MOUSE",
                        "m_Flags": "NOCLIP", "m_Window": f"0,0,{WIDTH},{HEIGHT}", "m_Scale": "1,1",
                        "m_fAlpha": "1", "m_fTargetAlpha": "1"}.items():
        set_value(panel, name, value)
    add_parchment(panel, controls["sprSubBanner"])
    add_text(panel, text, "enemyTitle", "", "22,16,388,53", "0.62", fancy=True)
    add_text(panel, text, "enemyMeta", "", "22,53,388,80", "0.50", "FF624D34")
    add_text(panel, text, "healthLabel", "Health", "22,83,157,115", "0.60", "FF7A2424", True)
    add_text(panel, text, "enemyHealth", "", "172,83,388,115", "0.60", "FF7A2424")
    add_text(panel, text, "enemySchool", "", "22,119,388,145", "0.46", "FF765427", True)
    for index, label in enumerate(("Damage", "Resistance", "Accuracy", "Pierce", "Critical", "Block"), 1):
        top = 149 + (index - 1) * 24
        add_text(panel, text, f"enemyLabel{index}", label, f"22,{top},170,{top + 27}")
        add_text(panel, text, f"enemyValue{index}", "", f"172,{top},388,{top + 27}")
    add_text(panel, text, "enemyPips", "", "22,298,388,322", "0.46")
    add_text(panel, text, "enemyExtras", "", "22,321,388,345", "0.46")
    add_text(panel, text, "enemyNote", "Scroll over enemy: next school", "22,346,388,367", "0.39", "FF705A3B")
    ET.indent(document, space="    ")
    (ROOT / ".runtime/native/RandomizerEnemyInspector.gui").write_bytes(ET.tostring(document, encoding="utf-8"))


if __name__ == "__main__":
    main()
