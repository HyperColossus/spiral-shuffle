using System.Reflection;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Imcodec.CoreObject;
using Imcodec.Cryptography;
using Imcodec.IO;
using Imcodec.Math;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imcodec.Wad;
using Imlight.CoreLib.Shared.Resources;
using Type = System.Type;

internal static class SpellNumberTests {
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    private static int _checks;

    internal static int Run(Assembly assembly, string root) {
        using FileStream patched = File.OpenRead(Path.Combine(root, ".runtime/patch/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        assembly.GetType("Imlight.CoreLib.Shared.Resources.RootArchiveLoader")!.GetField("s_rootWad", Static)!
            .SetValue(null, ArchiveParser.Parse(patched)!);
        using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/generated/catalog.json")));
        using JsonDocument settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/settings.json")));
        using FileStream input = File.OpenRead(Path.Combine(settings.RootElement.GetProperty("revive_path").GetString()!,
            "Aurorium/data/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        Archive original = ArchiveParser.Parse(input)!;
        BindSerializer codec = new();
        codec.Deserialize(original.OpenFile("TemplateManifest.xml")!.Value.ToArray(), PropertyFlags.Prop_Save, out TemplateManifest manifest);
        Dictionary<uint, TemplateLocation> paths = manifest.m_serializedTemplates.GroupBy(p => p.m_id).ToDictionary(g => g.Key, g => g.First());
        using ZipArchive zip = ZipFile.OpenRead(Path.Combine(root, ".runtime/generated/overrides.zip"));
        Dictionary<string, string> descriptions = ReadLocale(zip.GetEntry("Locale/en-US/Spell.lang")!);
        JsonElement[] variants = catalog.RootElement.GetProperty("SpellVariants").EnumerateArray().ToArray();
        uint[] calls = [169474362, 994069234];
        uint[] sampleIds = [.. calls, 35806180];
        foreach (uint id in sampleIds) {
            codec.Deserialize(original.OpenFile(paths[id].m_filename)!.Value.ToArray(), PropertyFlags.Prop_Save, out SpellTemplate source);
            JsonElement[] rolls = variants.Where(v => v.GetProperty("Original").GetUInt32() == id).ToArray();
            Require(rolls.Length == 3, $"Missing difficulty rolls for {source.m_name}.");
            foreach (JsonElement roll in rolls) {
                SpellTemplate actual = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(roll.GetProperty("TemplateId").GetUInt32());
                int difficulty = roll.GetProperty("Difficulty").GetInt32();
                double factor = roll.GetProperty("Multiplier").GetDouble();
                Require(factor >= (difficulty == 1 ? 0.75 : 0.5) && factor <= (difficulty == 3 ? 2 : difficulty == 2 ? 1.5 : 1.25),
                    "A spell power roll escaped its difficulty bounds.");
                int baseDamage = source.m_effects[0].m_effectParam;
                int damage = actual.m_effects[0].m_effectParam;
                Require(damage == (int)Math.Round(baseDamage * factor) && damage != baseDamage && damage > 1,
                    $"{source.m_name} did not receive a bounded damage roll.");
                Require(descriptions[actual.m_description].Contains(damage.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    "The native card description does not contain its rolled damage.");
                Require(actual.m_spellRank.m_spellRank == source.m_spellRank.m_spellRank && actual.m_spellRank.m_shadowPips == source.m_spellRank.m_shadowPips,
                    "A numeric roll changed a spell's pip or shadow-pip cost.");
            }
        }

        foreach (string name in new[] { "Deathblade", "Death Trap" }) {
            TemplateLocation location = paths.Values.First(p => p.m_filename.ToString() == $"Spells/{name}.xml");
            codec.Deserialize(original.OpenFile(location.m_filename)!.Value.ToArray(), PropertyFlags.Prop_Save, out SpellTemplate source);
            foreach (JsonElement roll in variants.Where(v => v.GetProperty("Original").GetUInt32() == location.m_id)) {
                SpellTemplate actual = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(roll.GetProperty("TemplateId").GetUInt32());
                int expected = (int)Math.Clamp(Math.Round(source.m_effects[0].m_effectParam * roll.GetProperty("Multiplier").GetDouble()), -95, 200);
                Require(actual.m_effects[0].m_effectParam == expected && expected != source.m_effects[0].m_effectParam,
                    $"{name} was excluded from utility number rolls.");
                Require(descriptions[actual.m_description].Contains(Math.Abs(expected).ToString()), "A utility card displays the original percentage.");
            }
        }

        foreach (ZipArchiveEntry entry in zip.Entries.Where(e => e.FullName.EndsWith("/Spell.lang", StringComparison.Ordinal))) {
            Dictionary<string, string> rows = ReadLocale(entry);
            Dictionary<string, string> prior = [];
            if (original.OpenFile(entry.FullName) is { } bytes) {
                using StringReader reader = new(Encoding.Unicode.GetString(bytes.Span).TrimStart('\uFEFF'));
                prior = ReadLocale(reader);
            }
            Require(rows.Keys.Except(prior.Keys).All(key => key.Length == 14 && key.StartsWith("Spell_", StringComparison.Ordinal) && key[6..].All(char.IsAsciiDigit)),
                "A new native locale row has an overlong numeric ID.");
            Require(prior.All(row => rows.GetValueOrDefault(row.Key) == row.Value), "A vanilla spell locale row changed.");
        }
        Require(descriptions["Spell_00000053"] == "$eA$ $dT_image$$Damage_image$", "A vanilla locale row was overwritten.");
        Dictionary<uint, uint> donors = catalog.RootElement.GetProperty("SpellSwaps").EnumerateArray()
            .ToDictionary(swap => swap.GetProperty("From").GetUInt32(), swap => swap.GetProperty("To").GetUInt32());
        Dictionary<uint, SpellTemplate> sources = [];
        foreach (JsonElement roll in variants) {
            uint id = roll.GetProperty("Original").GetUInt32();
            uint donor = donors.GetValueOrDefault(id, id);
            if (!sources.TryGetValue(donor, out SpellTemplate? source)) {
                codec.Deserialize(original.OpenFile(paths[donor].m_filename)!.Value.ToArray(), PropertyFlags.Prop_Save, out SpellTemplate decoded);
                source = decoded;
                sources.Add(donor, source);
            }
            SpellTemplate actual = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(roll.GetProperty("TemplateId").GetUInt32());
            EquipmentItemCardTests.VerifyNumbers(source.m_effects, actual.m_effects, roll.GetProperty("Multiplier").GetDouble());
        }
        VerifyPresentation(assembly, calls[0]);
        Console.WriteLine($"Passed {_checks} spell number checks: regular/shadow Khrulhu, Lamassu, blades, traps, native text keys and spellbook/deck projection.");
        return _checks;
    }

    private static Dictionary<string, string> ReadLocale(ZipArchiveEntry entry) {
        using StreamReader reader = new(entry.Open(), Encoding.Unicode);
        return ReadLocale(reader);
    }

    private static Dictionary<string, string> ReadLocale(TextReader reader) {
        Require(reader.ReadLine() == "1:Spell", "The native spell locale header changed.");
        Dictionary<string, string> result = [];
        while (reader.ReadLine() is { } key && reader.ReadLine() is not null && reader.ReadLine() is { } value) {
            result.Add("Spell_" + key, value);
        }
        return result;
    }

    private static void VerifyPresentation(Assembly assembly, uint spellId) {
        Type runtime = assembly.GetType("Imlight.CoreLib.Game.RandomizerRuntime")!;
        MethodInfo setMode = runtime.GetMethod("SetMode", Static)!;
        MethodInfo map = runtime.GetMethod("CombatSpellId", Static)!;
        MethodInfo project = assembly.GetType("Imlight.CoreLib.Game.SpellPresentation")!.GetMethod("ForClient", Static)!;
        ulong account = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray());
        WizClientObjectItem item = new() { m_templateID = 123U, m_globalID = 456UL, m_primaryColor = 7,
            m_inactiveBehaviors = [new DeckBehavior { m_archmasterySchool = 9, m_serializedExclusionList = "excluded",
                m_spellList = [new SpellData { m_templateID = spellId, m_quantity = 3 }] }] };
        WizClientObject source = CoreObjectFactory.InitializeCoreObjectBehaviors(new WizClientObject(), 1);
        source.m_templateID = 1U;
        source.m_globalID = 321UL;
        source.m_permID = 321UL;
        source.m_characterId = 654UL;
        source.m_nMobileID = 17;
        source.m_fScale = 1f;
        source.m_location = new Vector3(12, 34, 56);
        source.m_orientation = new Vector3(0, 0, 1);
        source.m_gameStats = new WizGameStats { m_currentMana = 41, m_currentHitpoints = 434 };
        source.m_inactiveBehaviors.RemoveAll(behavior => behavior is ClientSpellbookBehavior or ClientWizEquipmentBehavior or ClientWizInventoryBehavior);
        source.m_inactiveBehaviors.Add(new ClientSpellbookBehavior { m_behaviorTemplateNameID = StringHash.Compute("ClientSpellbookBehavior"),
            m_spellIDList = [new SpellIDTracker { m_spellID = spellId }] });
        source.m_inactiveBehaviors.Add(new ClientWizEquipmentBehavior { m_behaviorTemplateNameID = StringHash.Compute("ClientWizEquipmentBehavior"), m_itemList = [item] });
        source.m_inactiveBehaviors.Add(new ClientWizInventoryBehavior { m_behaviorTemplateNameID = StringHash.Compute("ClientWizInventoryBehavior"), m_itemList = [item] });
        CoreObjectSerializer codec = new(versionable: false, behaviors: SerializerFlags.None);
        const PropertyFlags flags = PropertyFlags.Prop_Transmit | PropertyFlags.Prop_AuthorityTransmit;
        Require(codec.Serialize(source, flags, out ByteString before), "Cannot serialize the spellbook projection fixture.");
        foreach (int mode in new[] { 2, 3, 4, 5, 6, 1 }) {
            setMode.Invoke(null, [account, mode]);
            WizClientObject copy = (WizClientObject)project.Invoke(null, [source, account])!;
            Require(copy.m_templateID == source.m_templateID && copy.m_globalID == source.m_globalID &&
                copy.m_permID == source.m_permID && copy.m_characterId == source.m_characterId &&
                copy.m_nMobileID == source.m_nMobileID && copy.m_fScale == source.m_fScale,
                "The client projection lost the wizard's template, identity or mobile ID.");
            Require(copy.m_location == source.m_location && copy.m_orientation == source.m_orientation &&
                copy.m_gameStats.m_currentMana == 41 && copy.m_gameStats.m_currentHitpoints == 434,
                "The client projection changed the wizard's position or current stats.");
            Require(copy.m_inactiveBehaviors.Count == source.m_inactiveBehaviors.Count,
                "The client projection changed the wizard's behaviors.");
            uint expected = (uint)map.Invoke(null, [spellId, mode])!;
            Require(copy.m_inactiveBehaviors.OfType<ClientSpellbookBehavior>().Single().m_spellIDList[0].m_spellID == expected,
                "The spellbook preview used a different difficulty from combat.");
            CoreObject projectedItem = copy.m_inactiveBehaviors.OfType<ClientWizEquipmentBehavior>().Single().m_itemList[0];
            DeckBehavior deck = projectedItem.m_inactiveBehaviors.OfType<DeckBehavior>().Single();
            Require(deck.m_spellList[0].m_templateID == expected && deck.m_spellList[0].m_quantity == 3 && deck.m_archmasterySchool == 9,
                "The client deck projection lost cards, copies or archmastery choice.");
            Require(deck.m_serializedExclusionList == (ByteString)"excluded", "The deck projection lost its item card exclusion list.");
            CoreObject inventoryItem = copy.m_inactiveBehaviors.OfType<ClientWizInventoryBehavior>().Single().m_itemList[0];
            Require(inventoryItem.m_inactiveBehaviors.OfType<DeckBehavior>().Single().m_spellList[0].m_templateID == expected,
                "The inventory deck preview used a different difficulty from combat.");
            Require(projectedItem.m_globalID == (GID)456UL && ((WizClientObjectItem)projectedItem).m_primaryColor == 7,
                "The client projection changed an item ID or dye.");
            Require(item.m_inactiveBehaviors.OfType<DeckBehavior>().Single().m_spellList[0].m_templateID == spellId,
                "A projected equipped or inventory deck changed the saved deck's spell ID.");
            Require(codec.Serialize(copy, flags, out ByteString packet) && packet.Length > 0,
                "The projected wizard cannot be written to the login packet.");
            Require((uint)map.Invoke(null, [expected, mode])! == expected, "An already-randomized card was randomized a second time.");
        }
        Require(codec.Serialize(source, flags, out ByteString after) && ((byte[])before).SequenceEqual((byte[])after),
            "Projecting the spellbook changed the server's saved spell IDs or equipment.");
    }

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}
