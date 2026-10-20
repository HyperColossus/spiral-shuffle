using System.Reflection;
using System.Text.Json;
using System.IO.Compression;
using System.Text;
using Imcodec.Cryptography;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imcodec.Wad;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Newtonsoft.Json;
using Type = System.Type;

internal static class EquipmentItemCardTests {
    private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
    private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
    private static int _checks;

    internal static int Run(Assembly assembly, string root) {
        using FileStream patched = File.OpenRead(Path.Combine(root, ".runtime/patch/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        assembly.GetType("Imlight.CoreLib.Shared.Resources.RootArchiveLoader")!.GetField("s_rootWad", Static)!
            .SetValue(null, ArchiveParser.Parse(patched)!);
        using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/generated/catalog.json")));
        JsonElement[] cards = catalog.RootElement.GetProperty("EquipmentCards").EnumerateArray().ToArray();
        HashSet<uint> equipment = catalog.RootElement.GetProperty("Equipment").EnumerateArray()
            .Select(item => item.GetProperty("Id").GetUInt32()).ToHashSet();
        Require(equipment.SetEquals(cards.Select(card => card.GetProperty("Original").GetUInt32())), "Equipment is missing an item-card roll.");
        Require(cards.GroupBy(card => card.GetProperty("Donor").GetUInt32()).Any(group =>
            group.Select(card => card.GetProperty("Multipliers")[0].GetDouble()).Distinct().Count() > 5),
            "Gear granting the same spell reused its numeric roll.");
        VerifyAssets(cards, root);
        VerifyDescriptionRendering();
        Type feature = assembly.GetType("Imlight.CoreLib.Game.EquipmentItemCards")!;
        MethodInfo map = feature.GetMethod("TemplateId", Static)!;
        MethodInfo menu = feature.GetMethod("HandleMenuSetting", Static)!;
        MethodInfo setMode = assembly.GetType("Imlight.CoreLib.Game.RandomizerRuntime")!.GetMethod("SetMode", Static)!;
        ulong account = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray());
        Require(Setting(menu, account, 0) == -1, "Item cards must default to off.");
        foreach (int invalid in new[] { -1, 1, 3, int.MinValue, int.MaxValue }) {
            Require(!(bool)menu.Invoke(null, [account, Packet(invalid), Array.Empty<byte>()])!, "An invalid item-card setting was accepted.");
        }
        Require(Setting(menu, account, 2) == 1, "The item-card toggle could not enable.");
        JsonElement sample = cards[0];
        uint original = sample.GetProperty("Original").GetUInt32();
        for (int mode = 1; mode <= 6; mode++) {
            setMode.Invoke(null, [account, mode]);
            uint alias = sample.GetProperty("Items")[(mode - 1) % 3].GetUInt32();
            Require((ulong)map.Invoke(null, [(ulong)original, account])! == alias, "Gear used the wrong difficulty card.");
            Require((ulong)map.Invoke(null, [(ulong)alias, account])! == alias, "Repeated gear mapping rerolled the item.");
            Require((ulong)map.Invoke(null, [0UL, account])! == 0, "An unrelated template was changed.");
        }
        VerifyEquipment(assembly, feature, account, original, sample.GetProperty("Items")[2].GetUInt32());
        Require(Setting(menu, account, -2) == -1, "The item-card toggle could not disable.");
        foreach (JsonElement alias in sample.GetProperty("Items").EnumerateArray()) {
            Require((ulong)map.Invoke(null, [(ulong)alias.GetUInt32(), account])! == original,
                "Turning item cards off did not restore the original equipment.");
        }
        Type storeType = assembly.GetType("Imlight.CoreLib.Game.AccountToggleStore")!;
        string settings = Path.Combine(Directory.GetCurrentDirectory(), "item-card-tests", Guid.NewGuid().ToString(), "settings.json");
        object store = Activator.CreateInstance(storeType, Instance, null, [settings], null)!;
        storeType.GetMethod("Set", Instance)!.Invoke(store, [account, true]);
        object reloaded = Activator.CreateInstance(storeType, Instance, null, [settings], null)!;
        Require((bool)storeType.GetMethod("Get", Instance)!.Invoke(reloaded, [account])!, "The toggle did not survive reloading.");
        Require(!(bool)storeType.GetMethod("Get", Instance)!.Invoke(reloaded, [account + 1])!, "An account inherited another account's toggle.");
        storeType.GetMethod("Set", Instance)!.Invoke(reloaded, [account, false]);
        object disabled = Activator.CreateInstance(storeType, Instance, null, [settings], null)!;
        Require(!(bool)storeType.GetMethod("Get", Instance)!.Invoke(disabled, [account])!, "Turning off the toggle did not persist.");
        Console.WriteLine($"Passed {_checks} equipment card checks across {cards.Length} pieces: all slots, independent numbers, tooltips, equip/unequip, shop IDs, loot and reversible settings.");
        return _checks;
    }

    private static void VerifyAssets(JsonElement[] cards, string root) {
        using ZipArchive overrides = ZipFile.OpenRead(Path.Combine(root, ".runtime/generated/overrides.zip"));
        using StreamReader descriptions = new(overrides.GetEntry("Locale/en-US/Spell.lang")!.Open(), Encoding.Unicode);
        Dictionary<string, string> text = [];
        descriptions.ReadLine();
        while (descriptions.ReadLine() is { } key && descriptions.ReadLine() is not null && descriptions.ReadLine() is { } value) {
            text.Add("Spell_" + key, value);
        }
        int lamassuCards = 0;
        using JsonDocument settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/settings.json")));
        using FileStream stream = File.OpenRead(Path.Combine(settings.RootElement.GetProperty("revive_path").GetString()!,
            "Aurorium/data/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        Archive originalArchive = ArchiveParser.Parse(stream)!;
        BindSerializer codec = new();
        codec.Deserialize(originalArchive.OpenFile("TemplateManifest.xml")!.Value.ToArray(), PropertyFlags.Prop_Save, out TemplateManifest manifest);
        Dictionary<uint, TemplateLocation> paths = manifest.m_serializedTemplates.GroupBy(location => location.m_id)
            .ToDictionary(group => group.Key, group => group.First());
        Dictionary<uint, SpellTemplate> originals = [];
        HashSet<string> schools = [];
        foreach (JsonElement entry in cards) {
            uint source = entry.GetProperty("Original").GetUInt32();
            uint donorId = entry.GetProperty("Donor").GetUInt32();
            if (!originals.TryGetValue(donorId, out SpellTemplate? donor)) {
                Require(codec.Deserialize<SpellTemplate>(originalArchive.OpenFile(paths[donorId].m_filename)!.Value.ToArray(),
                    PropertyFlags.Prop_Save, out var decoded), "An item-card donor could not be decoded.");
                donor = decoded;
                originals.Add(donorId, donor);
            }
            schools.Add(donor.m_sMagicSchoolName);
            WizItemTemplate original = (WizItemTemplate)CoreObjectFactory.GetCoreTemplate(source);
            for (int difficulty = 0; difficulty < 3; difficulty++) {
                uint itemId = entry.GetProperty("Items")[difficulty].GetUInt32();
                uint spellId = entry.GetProperty("Spells")[difficulty].GetUInt32();
                WizItemTemplate gear = (WizItemTemplate)CoreObjectFactory.GetCoreTemplate(itemId);
                SpellTemplate spell = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(spellId);
                ProvideSpellEffectInfo[] effects = gear.m_equipEffects.OfType<ProvideSpellEffectInfo>().ToArray();
                Require(effects.Length == 1 && effects[0].m_numSpells == 1 && StringHash.Compute(effects[0].m_spellName) == spellId,
                    "The equipment tooltip does not grant its random card exactly once.");
                Require(gear.m_templateID == itemId && gear.m_visualID == original.m_visualID && gear.m_displayName == original.m_displayName,
                    "Adding an item card changed the equipment's appearance or name.");
                Require(JsonConvert.SerializeObject(gear.m_equipEffects.Where(effect => effect is not ProvideSpellEffectInfo)) ==
                    JsonConvert.SerializeObject(original.m_equipEffects.Where(effect => effect is not ProvideSpellEffectInfo)),
                    "Adding a card changed the existing equipment stats.");
                Require(spell.m_sMagicSchoolName == donor.m_sMagicSchoolName && spell.m_displayName == donor.m_displayName,
                    "The random item card changed school or displayed the wrong spell.");
                double multiplier = entry.GetProperty("Multipliers")[difficulty].GetDouble();
                VerifyNumbers(donor.m_effects, spell.m_effects, multiplier);
                if (spell.m_description.ToString().StartsWith("Spell_", StringComparison.Ordinal)) {
                    Require(text.ContainsKey(spell.m_description), "An equipment card references missing native description text.");
                }
                if (donor.m_name == "Novus Life - T01 - Base") {
                    int expected = (int)Math.Round(535 * multiplier);
                    Require(expected != 535 && spell.m_effects[0].m_effectParam == expected,
                        "Lamassu still uses its unrolled base damage.");
                    Require(text[spell.m_description] == $"{expected} $dT_image$$Damage_image$",
                        "Lamassu's visible card description does not match its actual damage.");
                    lamassuCards++;
                }
            }
        }
        Require(schools.Count == 11, "A school was excluded from equipment item cards.");
        Require(lamassuCards > 0, "The Lamassu display regression was not tested.");
    }

    private static void VerifyDescriptionRendering() {
        MethodInfo render = Assembly.Load("AssetTool").GetType("SpellDescriptions")!.GetMethod("Render", Static)!;
        SpellEffect[] effects = [new RandomSpellEffect { m_effectList = [
            new SpellEffect { m_effectType = kSpellEffects.kDamage, m_effectParam = 138 }, new SpellEffect { m_effectType = kSpellEffects.kDamage, m_effectParam = 177 },
            new SpellEffect { m_effectType = kSpellEffects.kDamage, m_effectParam = 217 }
        ] }, new SpellEffect { m_effectType = kSpellEffects.kModifyIncomingDamage, m_effectParam = -27 }];
        string result = (string)render.Invoke(null, ["#1:$eA$-$eA3$ $Damage_image$ and -#2:$eA4$% ($eARounds4$ $Rounds_image$)", effects])!;
        Require(result == "138-217 $Damage_image$ and -27% ($eARounds4$ $Rounds_image$)",
            "A range, percentage, icon or duration token was rendered incorrectly.");
    }

    internal static void VerifyNumbers(IReadOnlyList<SpellEffect> original, IReadOnlyList<SpellEffect> rolled, double multiplier) {
        Require(original.Count == rolled.Count, "An item card lost an effect.");
        for (int index = 0; index < original.Count; index++) {
            SpellEffect source = original[index];
            SpellEffect result = rolled[index];
            Require(source.m_effectType == result.m_effectType, "An item card changed effect type.");
            if (source.m_effectType is kSpellEffects.kDamage or kSpellEffects.kDamageNoCrit or kSpellEffects.kStealHealth or kSpellEffects.kDamageOverTime or
                kSpellEffects.kHeal or kSpellEffects.kHealOverTime) {
                Require(result.m_effectParam == Scale(source.m_effectParam, multiplier) &&
                    result.m_paramPerRound == Scale(source.m_paramPerRound, multiplier), "An item card's damage or healing was not randomized.");
            }
            if (source is EffectListSpellEffect sourceList && result is EffectListSpellEffect resultList) {
                VerifyNumbers(sourceList.m_effectList, resultList.m_effectList, multiplier);
            }
            if (source is RandomSpellEffect sourceRandom && result is RandomSpellEffect resultRandom) {
                VerifyNumbers(sourceRandom.m_effectList, resultRandom.m_effectList, multiplier);
            }
            if (source is VariableSpellEffect sourceVariable && result is VariableSpellEffect resultVariable) {
                VerifyNumbers(sourceVariable.m_effectList, resultVariable.m_effectList, multiplier);
            }
            if (source is CountBasedSpellEffect sourceCount && result is CountBasedSpellEffect resultCount) {
                VerifyNumbers(sourceCount.m_effectList, resultCount.m_effectList, multiplier);
            }
            if (source is ConditionalSpellEffect sourceConditional && result is ConditionalSpellEffect resultConditional) {
                VerifyNumbers(sourceConditional.m_elements.Select(element => element.m_pEffect).ToArray(),
                    resultConditional.m_elements.Select(element => element.m_pEffect).ToArray(), multiplier);
                Require(JsonConvert.SerializeObject(sourceConditional.m_elements.Select(element => element.m_pReqs)) ==
                    JsonConvert.SerializeObject(resultConditional.m_elements.Select(element => element.m_pReqs)),
                    "Randomizing a conditional spell changed its activation requirements.");
            }
            if (source is TargetCountSpellEffect sourceTargets && result is TargetCountSpellEffect resultTargets) {
                VerifyNumbers(sourceTargets.m_effectLists.Cast<SpellEffect>().ToArray(), resultTargets.m_effectLists.Cast<SpellEffect>().ToArray(), multiplier);
            }
            if (source is HangingConversionSpellEffect sourceConversion && result is HangingConversionSpellEffect resultConversion) {
                VerifyNumbers(sourceConversion.m_outputEffect, resultConversion.m_outputEffect, multiplier);
            }
        }
    }

    private static int Scale(int amount, double multiplier) => amount > 0 ? (int)Math.Clamp(Math.Round(amount * multiplier), 1, int.MaxValue) : amount;

    private static void VerifyEquipment(Assembly assembly, Type feature, ulong account, uint original, uint alias) {
        foreach (string name in new[] { "Imlight.CoreLib.Game.Effects.CanonicalStatEffects", "Imlight.CoreLib.Game.Effects.GameEffectRuleData" }) {
            assembly.GetType(name)!.GetProperty("Instance", Static | BindingFlags.FlattenHierarchy)!.GetValue(null);
        }
        Wizard wizard = new() { AccountId = account, InventoryBehavior = new() { Items = [], InventoryItemIds = [] },
            EquipmentBehavior = new() { EquippedItems = [], EquippedItemIds = [], SlotList = [] },
            GameStats = new ServerWizGameStats(MagicSchool.Fire, 1), SpellbookBehavior = new() };
        WizClientObjectItem item = new() { m_templateID = original, m_globalID = 123456UL, m_primaryColor = 7, m_secondaryColor = 9 };
        wizard.EquipmentBehavior.EquippedItems.Add(item);
        feature.GetMethod("Apply", Static, null, [typeof(Wizard)], null)!.Invoke(null, [wizard]);
        Require(item.m_templateID.Full == alias && item.m_globalID.Full == 123456 && item.m_primaryColor == 7 && item.m_secondaryColor == 9,
            "Mapping saved equipment lost its item identity or dyes.");
        Type effects = assembly.GetType("Imlight.CoreLib.Game.Effects.CharacterEffectHelper")!;
        WizItemTemplate template = (WizItemTemplate)CoreObjectFactory.GetCoreTemplate(alias);
        effects.GetMethod("AddEffectsToWizard", Static)!.Invoke(null, [wizard, template]);
        Require(wizard.SpellbookBehavior.TemporarySpells.Count == 1 && wizard.SpellbookBehavior.TemporarySpells[0].m_itemCard,
            "Equipping gear did not grant its item card to the combat spellbook.");
        effects.GetMethod("RemoveEffectsFromWizard", Static)!.Invoke(null, [wizard, template]);
        Require(wizard.SpellbookBehavior.TemporarySpells.Count == 0, "Unequipping gear left its random card behind.");
        effects.GetMethod("AddEffectsToWizard", Static)!.Invoke(null, [wizard, template]);
        Spell questSpell = new() { m_templateID = 99 };
        wizard.SpellbookBehavior.TemporarySpells.Add(questSpell);
        feature.GetMethod("PrepareRecalculation", Static)!.Invoke(null, [wizard]);
        Require(wizard.SpellbookBehavior.TemporarySpells.SequenceEqual([questSpell]),
            "Recalculating gear duplicated its cards or removed a temporary quest spell.");
        GID shopId = (GID)((1UL << 40) | original);
        List<GID> shop = (List<GID>)feature.GetMethod("ShopItems", Static)!.Invoke(null, [wizard, new List<GID> { shopId }])!;
        Require(shop[0].MParts.TemplateId == alias && (shop[0].Full >> 32) == (shopId.Full >> 32), "A shop card changed the non-template part of its ID.");
        DropTableResult loot = new();
        var rewardType = loot.Items.GetType().GetGenericArguments()[0];
        object reward = Activator.CreateInstance(rewardType)!;
        rewardType.GetProperty("ItemId")!.SetValue(reward, original.ToString());
        ((System.Collections.IList)loot.Items).Add(reward);
        feature.GetMethod("ApplyLoot", Static)!.Invoke(null, [wizard, loot]);
        Require(loot.Items[0].ItemId == alias.ToString(), "Loot preview and granted equipment used different cards.");
    }

    private static int Setting(MethodInfo menu, ulong account, int score) {
        object?[] arguments = [account, Packet(score), Array.Empty<byte>()];
        Require((bool)menu.Invoke(null, arguments)!, "An item-card menu setting was rejected.");
        byte[] response = (byte[])arguments[2]!;
        MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS decoded = new();
        decoded.Decode(new BitReader(response[4..]));
        return decoded.score;
    }

    private static ByteString Packet(int score) => (ByteString)MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS {
        score = score, gameName = "SpiralItemCards"
    })[8..];

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}
