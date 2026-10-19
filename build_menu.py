"""Build native Wizard101 controls from the client's own menu styles."""
import copy
import json
import xml.etree.ElementTree as ET
from pathlib import Path


ROOT = Path(__file__).resolve().parent
OUTPUT = ROOT / ".runtime/native"
SOURCE = ROOT / ".build/research/CharacterSelectWindow.gui.bin"


def set_value(element: ET.Element, name: str, text: str) -> None:
    field = element.find(name)
    if field is None:
        field = ET.SubElement(element, name)
    field.text = text


def add_control(parent: ET.Element, control: ET.Element) -> None:
    ET.SubElement(parent, "m_Children").append(control)


def main() -> None:
    seed = json.loads((ROOT / ".runtime/settings.json").read_text(encoding="utf-8"))["seed"]
    tree = ET.parse(SOURCE)
    controls = {element.findtext("m_sName"): element for element in tree.iter("Class") if element.find("m_sName") is not None}
    menu = ET.Element("Objects")
    window = ET.SubElement(menu, "Class", {"Name": "class Window"})
    for name, value in {"m_sName": "SpiralShuffleMenu", "m_Style": "HAS_NO_BORDER", "m_Flags": "VISIBLE|HCENTER|VCENTER", "m_Window": "0,0,920,650", "m_fAlpha": "1", "m_fTargetAlpha": "1", "m_Scale": "1,1"}.items():
        set_value(window, name, value)

    background = copy.deepcopy(controls["sprSubBanner"])
    for child in background.findall("m_Children"):
        background.remove(child)
    set_value(background, "m_sName", "shuffleParchment")
    set_value(background, "m_Window", "0,0,920,650")
    set_value(background, "m_Flags", "VISIBLE")
    background.find(".//m_sFilename").text = "GUI/Backgrounds/Background_Credits.dds"
    add_control(window, background)

    rules_background = copy.deepcopy(background)
    set_value(rules_background, "m_sName", "shuffleRulesPaper")
    set_value(rules_background, "m_Window", "496,352,870,604")
    rules_background.find(".//m_sFilename").text = "GUI/paper_texture.dds"
    add_control(window, rules_background)

    def text(name: str, value: str, rect: str, scale: str = "0.66", color: str = "FF35221B", fancy: bool = False) -> None:
        control = copy.deepcopy(controls["txtName"])
        set_value(control, "m_sName", name)
        set_value(control, "m_Window", rect)
        set_value(control, "m_Flags", "VISIBLE")
        set_value(control, "m_sText", value)
        set_value(control, "m_bUseDropShadow", "0")
        control.find(".//m_fScale").text = scale
        control.find(".//m_sFilename").text = "Fonts/WizardFancy.xml" if fancy else "Fonts/WizardNormal.xml"
        control.find(".//m_pWindowStyle/Class/m_Color").text = color
        add_control(window, control)

    def button(name: str, label: str, rect: str, scale: str = "0.68") -> None:
        control = copy.deepcopy(controls["btnPlay"])
        set_value(control, "m_sName", name)
        set_value(control, "m_sLabel", label)
        set_value(control, "m_Window", rect)
        set_value(control, "m_Flags", "VISIBLE")
        set_value(control, "m_fMaxScale", "1.04")
        control.find("m_GamepadHotKey").text = "00000000"
        control.find(".//m_fScale").text = scale
        add_control(window, control)

    text("shuffleTitle", "<center>SPIRAL SHUFFLE</center>", "55,35,445,91", "0.90", fancy=True)
    text("shuffleSubtitle", "<center>Choose your challenge.</center>", "60,95,440,130", "0.66")
    descriptions = [
        ("Shuffle", "Enemy HP 80-120%. Spell power 75-125%."),
        ("Chaos", "Enemy HP 91-210%. Spell power 50-150%."),
        ("Nightmare", "Enemy HP 113-394%. Spell power 25-200%."),
    ]
    for index, (name, description) in enumerate(descriptions, 1):
        top = 146 + (index - 1) * 96
        button(f"shuffleMode{index}", name, f"77,{top},421,{top + 47}", "0.72")
        text(f"shuffleDescription{index}", f"<center>{description}</center>", f"79,{top + 49},419,{top + 91}", "0.58")

    text("shuffleStatus", f"<center>Selected: Shuffle | Seed: {seed}</center>", "58,436,442,478", "0.53")
    button("shuffleNew", "New Level 1 Wizard", "77,484,421,531", "0.63")
    button("shuffleContinue", "Choose My Wizard", "77,540,421,584", "0.63")
    text("shuffleCreatureTitle", "<center>CREATURE RULES</center>", "501,366,861,405", "0.71", fancy=True)
    button("shuffleCreatures", "Creature Stats Only", "507,415,855,466", "0.60")
    text("shuffleCreatureDescription", "", "512,478,850,548", "0.57")
    text("shuffleCreatureLimits", "<center>Extra resist: 0-5 / 12 / 20 points.<br>Accuracy: +/-5 / 10 / 15 points.<br>Existing crit and block ratings also vary.</center>", "512,550,850,599", "0.45")
    text("shuffleTravel", "<center>Travel and spawn points stay fixed.</center>", "53,587,447,626", "0.48")
    OUTPUT.mkdir(parents=True, exist_ok=True)
    ET.indent(menu, space="    ")
    (OUTPUT / "RandomizerMenu.gui").write_bytes(ET.tostring(menu, encoding="utf-8"))
    script = (ROOT / "Menu.lua").read_text(encoding="utf-8")
    for mode in range(1, 7):
        (OUTPUT / f"Menu{mode}.lua").write_text(script.replace("INITIAL_MODE", str(mode)).replace("SPIRAL-101", seed), encoding="utf-8")


if __name__ == "__main__":
    main()
