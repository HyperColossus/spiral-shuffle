"""Build the spell reward parchment using the client's own UI controls."""
import copy
import xml.etree.ElementTree as ET
from pathlib import Path

from build_enemy_ui import add_parchment, add_text
from build_menu import add_control, set_value


ROOT = Path(__file__).resolve().parent
WIDTH = 630
HEIGHT = 400


def main() -> None:
    source = ET.parse(ROOT / ".build/research/CharacterSelectWindow.gui.bin")
    controls = {node.findtext("m_sName"): node for node in source.iter("Class") if node.find("m_sName") is not None}
    tooltip = ET.parse(ROOT / ".build/research/ToolTip.gui")
    text = next(node for node in tooltip.iter("Class") if node.findtext("m_sName") == "ToolTipText")
    document = ET.Element("Objects")
    panel = ET.SubElement(document, "Class", {"Name": "class Window"})
    for name, value in {"m_sName": "RandomizerSpellReward", "m_Style": "HAS_NO_BORDER",
                        "m_Flags": "NOCLIP", "m_Window": f"0,0,{WIDTH},{HEIGHT}", "m_Scale": "1,1",
                        "m_fAlpha": "1", "m_fTargetAlpha": "1"}.items():
        set_value(panel, name, value)
    add_parchment(panel, controls["sprSubBanner"], WIDTH, HEIGHT)
    add_text(panel, text, "rewardTitle", "<center>NEW SPELL LEARNED</center>", "22,16,608,60", "0.81", "FF8C5818", True)
    add_text(panel, text, "rewardSubtitle", "<center>A lucky find from the Spiral.</center>", "30,60,600,88", "0.48", "FF705A3B")
    holder = ET.Element("Class", {"Name": "class Window"})
    for name, value in {"m_sName": "rewardCardHolder", "m_Style": "HAS_NO_BORDER|DO_NOT_CAPTURE_MOUSE",
                        "m_Flags": "VISIBLE", "m_Window": "30,96,224,368"}.items():
        set_value(holder, name, value)
    add_control(panel, holder)
    add_text(panel, text, "rewardName", "", "249,103,600,159", "0.67", "FF5F321B", True)
    add_text(panel, text, "rewardSchool", "", "249,164,600,195", "0.57", "FF705A3B")
    add_text(panel, text, "rewardCost", "", "249,205,600,235", "0.53")
    add_text(panel, text, "rewardAccuracy", "", "249,237,600,267", "0.53")
    add_text(panel, text, "rewardRoll", "", "249,276,600,305", "0.47", "FF8C5818")
    add_text(panel, text, "rewardLearned", "Added to your spellbook.", "249,309,600,336", "0.48", "FF356040")
    button = copy.deepcopy(controls["btnPlay"])
    for name, value in {"m_sName": "rewardDismiss", "m_sLabel": "Got it!", "m_Window": "249,343,478,385",
                        "m_Flags": "VISIBLE", "m_fMaxScale": "1.03"}.items():
        set_value(button, name, value)
    button.find("m_GamepadHotKey").text = "00000000"
    button.find(".//m_fScale").text = "0.61"
    add_control(panel, button)
    add_text(panel, text, "rewardCount", "", "487,349,600,379", "0.46", "FF705A3B")
    ET.indent(document, space="    ")
    output = ROOT / ".runtime/native/RandomizerSpellReward.gui"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(ET.tostring(document, encoding="utf-8"))


if __name__ == "__main__":
    main()
