using System.Reflection;
using System.Text.Json;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Type = System.Type;

internal static class QaGearTests {
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static int _checks;

    internal static int Run(Assembly assembly, string root) {
        Type rules = assembly.GetType("Imlight.CoreLib.Game.QaGear")!;
        MethodInfo qa = rules.GetMethod("IsQa", Static)!;
        MethodInfo clean = rules.GetMethod("RemoveFrom", Static)!;
        foreach (var location in CoreObjectFactory.TemplateManifest.m_serializedTemplates.Where(location =>
                     location.m_filename.ToString().StartsWith("ObjectData/QA_Gear/", StringComparison.OrdinalIgnoreCase))) {
            Require((bool)qa.Invoke(null, [(ulong)location.m_id])!, "A QA template escaped the gear filter.");
        }
        foreach (uint id in new uint[] { 87256, 284071, 126983 }) {
            Require(!(bool)qa.Invoke(null, [(ulong)id])!, "Normal starter equipment was removed.");
        }
        using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/generated/catalog.json")));
        JsonElement card = catalog.RootElement.GetProperty("EquipmentCards").EnumerateArray()
            .First(item => item.GetProperty("Original").GetUInt32() == 1317127);
        foreach (JsonElement id in card.GetProperty("Items").EnumerateArray()) {
            Require((bool)qa.Invoke(null, [(ulong)id.GetUInt32()])!, "A randomized QA item bypassed cleanup.");
        }
        WizClientObjectItem hat = new() { m_globalID = 1UL, m_templateID = 1317127U };
        WizClientObjectItem wand = new() { m_globalID = 2UL, m_templateID = 87256U };
        WizClientObjectItem mount = new() { m_globalID = 3UL, m_templateID = 284071U };
        WizClientObjectItem boots = new() { m_globalID = 4UL, m_templateID = 1317234U };
        WizClientObjectItem deck = new() { m_globalID = 5UL, m_templateID = 126983U };
        Wizard wizard = new() {
            InventoryBehavior = new() { Items = [hat, wand, mount], InventoryItemIds = [1, 2, 3] },
            EquipmentBehavior = new() { EquippedItems = [boots, deck], EquippedItemIds = [4, 5],
                SlotList = [new() { SlotType = EquipmentSlotType.Shoes, ItemId = 4UL },
                    new() { SlotType = EquipmentSlotType.Deck, ItemId = 5UL }] },
            SpellbookBehavior = new() { LearnedSpellTemplateIds = [42] }
        };
        Require((bool)clean.Invoke(null, [wizard])!, "Existing QA equipment was not removed.");
        Require(wizard.InventoryBehavior.Items.SequenceEqual([wand, mount]) &&
            wizard.InventoryBehavior.InventoryItemIds.SequenceEqual([2UL, 3UL]), "Inventory cleanup removed normal items or retained QA references.");
        Require(wizard.EquipmentBehavior.EquippedItems.SequenceEqual([deck]) &&
            wizard.EquipmentBehavior.EquippedItemIds.SequenceEqual([5UL]) &&
            wizard.EquipmentBehavior.SlotList.Single().SlotType == EquipmentSlotType.Deck,
            "Equipped QA gear remained active or the normal deck was removed.");
        Require(wizard.SpellbookBehavior.LearnedSpellTemplateIds.SequenceEqual([42U]), "Gear cleanup changed learned spells.");
        Require(!(bool)clean.Invoke(null, [wizard])!, "Gear cleanup was not idempotent.");
        GID normal = new();
        normal.MParts.TemplateId = 87256;
        GID blocked = new();
        blocked.MParts.TemplateId = 1317127;
        Type vendors = assembly.GetType("Imlight.CoreLib.Game.VendorRandomization")!;
        var stock = (List<GID>)vendors.GetMethod("Items", Static)!.Invoke(null, [wizard, 1UL, new[] { blocked, normal }])!;
        Require(stock.SequenceEqual([normal]), "A vendor offered QA gear with shop shuffling disabled.");
        Console.WriteLine($"Passed {_checks} QA gear checks: starter IDs, randomized aliases, equipped stats, safe inventory cleanup and shop filtering.");
        return _checks;
    }

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}
