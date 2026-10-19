using System.Reflection;
using System.Text.Json;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Wad;
using Imlight.CoreLib.Game;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Type = System.Type;

internal static class SpellDropTests {
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static int _checks;

    internal static int Run(Assembly assembly, string root) {
        using FileStream patched = File.OpenRead(Path.Combine(root, ".runtime/patch/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        Archive patchedArchive = ArchiveParser.Parse(patched)!;
        Type loader = assembly.GetType("Imlight.CoreLib.Shared.Resources.RootArchiveLoader")!;
        loader.GetField("s_rootWad", Static)!.SetValue(null, patchedArchive);
        VerifyOdds();
        VerifySettings(assembly);
        DropSpell[] pool = VerifyAssets(assembly, root);
        HashSet<uint> learned = [pool[0].Original, pool[1].Variants[2]];
        for (int difficulty = 1; difficulty <= 3; difficulty++) {
            uint[] available = SpellDropRules.Available(pool, learned, difficulty);
            Require(available.Length == pool.Length - 2, "Already learned spell families were not excluded.");
            Require(available.All(id => pool.Skip(2).Any(spell => spell.Variants[difficulty - 1] == id)),
                "A dropped spell used the wrong difficulty variant.");
        }
        Require(SpellDropRules.Available(pool, pool.Select(spell => spell.Original).ToHashSet(), 1).Length == 0,
            "A completed spellbook still produced duplicates.");
        VerifyRewardStaging(assembly, pool);
        Console.WriteLine($"Passed {_checks} spell-drop checks: per-creature odds, low-end weighting, saved settings, full reward pool, numeric variants and reward staging.");
        return _checks;
    }

    private static void VerifyOdds() {
        foreach (int cap in new[] { 10, 20, 30, 50, 75, 100, 150, 200, 300, 500, 750, 1000, 1500, 2000, 3000, 4000, 5000, 7500, 10000 }) {
            double mean = 0;
            for (int index = 0; index < 10_000; index++) {
                int chance = SpellDropRules.Chance(cap, index / 9999.0);
                Require(chance >= 1000 && chance <= cap * 100, "Creature odds escaped their configured range.");
                mean += chance / 10_000.0;
            }
            if (cap > 10) {
                Require(mean < 1000 + (cap * 100 - 1000) * 0.3, "Creature odds were not weighted toward the low end.");
            }
            Require(SpellDropRules.Chance(-cap, 1) == 0, "Disabled drops still had a chance.");
        }

        foreach (int chance in new[] { 0, 1000, 1234, 500000, 1000000 }) {
            int wins = Enumerable.Range(0, SpellDropRules.Scale).Count(roll => SpellDropRules.Wins(chance, roll));
            Require(wins == chance, "Actual drop probability differs from the hover percentage.");
        }
        Require(SpellDropRules.Percent(1234) == "0.1234%", "The hover rounded away the actual odds.");
        Require(SpellDropRules.Percent(500000) == "50%", "The maximum percentage was incorrect.");
        Require(SpellDropRules.Chance(5000, 0.25) != SpellDropRules.Chance(5000, 0.75),
            "Different creatures reused the same percentage.");
        Require(SpellDropRules.Chance(5000, 0.25) == SpellDropRules.Chance(5000, 0.25),
            "Reopening the hover changed a creature's roll.");
        for (int index = 0; index <= 1000; index++) {
            Require(SpellDropRules.Chance(110000, index / 1000.0) == SpellDropRules.Scale,
                "Guaranteed mode failed for a creature's random encounter roll.");
            Require(SpellDropRules.Chance(100237, index / 1000.0) == 23700,
                "Fixed odds changed between creatures.");
        }
        Require(SpellDropRules.Chance(-110000, 1) == 0, "Disabling guaranteed drops still granted rewards.");
        foreach (int value in new[] { 0, 1, 9, -9, 10001, -10001, 100000, 100009, 110001, -110001, int.MaxValue, int.MinValue }) {
            Require(!SpellDropRules.IsValidSetting(value), "An invalid cap was accepted.");
        }
    }

    private static void VerifySettings(Assembly assembly) {
        Type type = assembly.GetType("Imlight.CoreLib.Game.SpellDropSettingsStore")!;
        string file = Path.Combine(Directory.GetCurrentDirectory(), "spell-drop-tests", Guid.NewGuid().ToString(), "settings.json");
        object store = Activator.CreateInstance(type, Instance, null, [file], null)!;
        MethodInfo get = type.GetMethod("Get", Instance)!;
        MethodInfo set = type.GetMethod("Set", Instance)!;
        Require((int)get.Invoke(store, [1UL])! == -100, "Drops did not default to off.");
        set.Invoke(store, [1UL, 5000]);
        set.Invoke(store, [2UL, -75]);
        object reloaded = Activator.CreateInstance(type, Instance, null, [file], null)!;
        Require((int)get.Invoke(reloaded, [1UL])! == 5000 && (int)get.Invoke(reloaded, [2UL])! == -75,
            "The cap or enabled preference did not survive a reload.");
        Type drops = assembly.GetType("Imlight.CoreLib.Game.SpellDrops")!;
        MethodInfo menu = drops.GetMethod("HandleMenuSetting", Static)!;
        MethodInfo read = assembly.GetType("Imlight.CoreLib.Game.RandomizerRuntime")!.GetMethod("TryReadMenuMessage", Static)!;
        foreach (int setting in new[] { 10, 5000, 10000, 110000, -100237, -75, 0 }) {
            object?[] args = [900003UL, Packet(setting), (ByteString)""];
            Require((bool)menu.Invoke(null, args)!, "A valid menu setting was rejected.");
            object?[] decoded = [args[2], "SpiralSpellDrops", 0];
            Require((bool)read.Invoke(null, decoded)! && (int)decoded[2]! == (setting == 0 ? -75 : setting),
                "The menu's saved cap did not round trip.");
        }
        foreach (int setting in new[] { 1, 9, -9, 10001, -10001, 100009, 110001 }) {
            Require(!(bool)menu.Invoke(null, [900003UL, Packet(setting), (ByteString)""])!, "An invalid network cap was saved.");
        }
    }

    private static DropSpell[] VerifyAssets(Assembly assembly, string root) {
        using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/generated/catalog.json")));
        JsonElement[] entries = catalog.RootElement.GetProperty("DropSpells").EnumerateArray().ToArray();
        Require(entries.Length > 400, "The spell reward catalog is incomplete.");
        Require(entries.Select(entry => entry.GetProperty("School").GetString()).Distinct().Count() == 11,
            "A magic school was excluded from spell rewards.");
        Require(entries.Any(entry => entry.GetProperty("Name").GetString() == "Pigsie"), "Drop-only rare spells were omitted.");
        Require(!entries.Any(entry => entry.GetProperty("Name").GetString()!.Contains("MOB", StringComparison.OrdinalIgnoreCase)),
            "Monster-only spells entered the reward pool.");
        using JsonDocument settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/settings.json")));
        string originalPath = Path.Combine(settings.RootElement.GetProperty("revive_path").GetString()!,
            "Aurorium/data/V_r806919.Wizard_1_610/Data/GameData/Root.wad");
        using FileStream stream = File.OpenRead(originalPath);
        Archive archive = ArchiveParser.Parse(stream)!;
        BindSerializer codec = new();
        if (!codec.Deserialize(archive.OpenFile("TemplateManifest.xml")!.Value.ToArray(), 1, out TemplateManifest? manifest) || manifest is null) {
            throw new InvalidDataException("Cannot inspect original spell data.");
        }
        Dictionary<uint, TemplateLocation> locations = manifest.m_serializedTemplates.DistinctBy(location => location.m_id)
            .ToDictionary(location => location.m_id);
        MethodInfo combat = assembly.GetType("Imlight.CoreLib.Game.RandomizerRuntime")!.GetMethod("CombatSpellId", Static)!;
        HashSet<uint> ids = [];
        List<DropSpell> pool = [];
        foreach (JsonElement entry in entries) {
            uint originalId = entry.GetProperty("Original").GetUInt32();
            if (!codec.Deserialize(archive.OpenFile(locations[originalId].m_filename)!.Value.ToArray(), 1, out SpellTemplate? original) || original is null) {
                throw new InvalidDataException("Cannot inspect a drop source spell.");
            }
            uint[] variants = entry.GetProperty("Variants").EnumerateArray().Select(id => id.GetUInt32()).ToArray();
            pool.Add(new(originalId, variants));
            for (int difficulty = 1; difficulty <= 3; difficulty++) {
                uint id = variants[difficulty - 1];
                Require(ids.Add(id), "Spell reward variants collided.");
                SpellTemplate variant = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(id);
                Require(!variant.m_Treasure && variant.m_sMagicSchoolName == original.m_sMagicSchoolName &&
                    variant.m_displayName == original.m_displayName, "A learned reward became a treasure card or changed identity.");
                Require((uint)combat.Invoke(null, [id, difficulty])! == id, "Combat changed the learned spell's displayed values.");
                double multiplier = entry.GetProperty("Multipliers")[difficulty - 1].GetDouble();
                double minimum = difficulty == 3 ? 0.25 : difficulty == 2 ? 0.5 : 0.75;
                double maximum = difficulty == 3 ? 2 : difficulty == 2 ? 1.5 : 1.25;
                Require(multiplier >= minimum && multiplier <= maximum && multiplier != 1,
                    "Spell power escaped its difficulty bounds.");
                SpellEffect[] before = Effects(original.m_effects).ToArray();
                SpellEffect[] after = Effects(variant.m_effects).ToArray();
                Require(before.Length == after.Length, "Spell randomization changed the effect structure.");
                foreach ((SpellEffect source, SpellEffect changed) in before.Zip(after)) {
                    if (source.m_effectType is kSpellEffects.kDamage or kSpellEffects.kStealHealth or
                        kSpellEffects.kDamageOverTime or kSpellEffects.kHeal or kSpellEffects.kHealOverTime) {
                        Require(changed.m_effectParam == Scaled(source.m_effectParam, multiplier) &&
                            changed.m_paramPerRound == Scaled(source.m_paramPerRound, multiplier), "A dropped spell retained default power.");
                    }
                    else if (source.m_effectType is kSpellEffects.kModifyOutgoingDamage or kSpellEffects.kModifyIncomingDamage or
                             kSpellEffects.kModifyAccuracy or kSpellEffects.kModifyOutgoingHeal or kSpellEffects.kModifyIncomingHeal) {
                        int expected = (int)Math.Clamp(Math.Round(source.m_effectParam * multiplier), -95, 200);
                        if (expected == 0 && source.m_effectParam != 0) { expected = Math.Sign(source.m_effectParam); }
                        Require(changed.m_effectParam == expected, "A dropped spell retained its default modifier.");
                    }
                    else if (source.m_effectType is kSpellEffects.kAbsorbDamage or
                             kSpellEffects.kModifyOutgoingHealFlat or kSpellEffects.kModifyIncomingHealFlat) {
                        Require(changed.m_effectParam == Scaled(source.m_effectParam, multiplier), "A dropped shield or heal retained default power.");
                    }
                }
            }
        }
        Console.WriteLine($"Audited {pool.Count} learned spell rewards across all eleven schools, with three numeric variants each.");
        return pool.ToArray();
    }

    private static void VerifyRewardStaging(Assembly assembly, DropSpell[] pool) {
        Type type = assembly.GetType("Imlight.CoreLib.Game.SpellDrops")!;
        MethodInfo menu = type.GetMethod("HandleMenuSetting", Static)!;
        MethodInfo add = type.GetMethod("AddCreatureDrop", Static)!;
        Wizard wizard = new() { AccountId = 900003, CharId = 910003, SpellbookBehavior = new ServerWizSpellbookBehavior() };
        DropTableResult loot = new();
        add.Invoke(null, [wizard, loot, 1.0]);
        Require(loot.Items.Count == 0, "An off setting still granted spells.");
        menu.Invoke(null, [wizard.AccountId, Packet(5000), (ByteString)""]);
        for (int attempt = 0; attempt < 1024 && loot.Items.Count == 0; attempt++) {
            add.Invoke(null, [wizard, loot, 1.0]);
        }
        Require(loot.Items.Count == 1 && pool.Any(spell => spell.Variants.Contains(uint.Parse(loot.Items[0].ItemId))),
            "A creature without a normal loot table could not stage a learned spell reward.");
        wizard.SpellbookBehavior.LearnedSpellTemplateIds.Add(uint.Parse(loot.Items[0].ItemId));
        DropTableResult second = new();
        for (int attempt = 0; attempt < 1024 && second.Items.Count == 0; attempt++) {
            add.Invoke(null, [wizard, second, 1.0]);
        }
        Require(second.Items.Count == 1 && second.Items[0].ItemId != loot.Items[0].ItemId, "Farming repeated an already learned reward.");
    }

    private static ByteString Packet(int setting) => (ByteString)MessageEncoder.Encode(
        new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS { score = setting, gameName = "SpiralSpellDrops" })[8..];

    private static int Scaled(int value, double multiplier)
        => value <= 0 ? value : (int)Math.Clamp(Math.Round(value * multiplier), 1, int.MaxValue);

    private static IEnumerable<SpellEffect> Effects(IEnumerable<SpellEffect> effects) {
        foreach (SpellEffect effect in effects) {
            yield return effect;
            IEnumerable<SpellEffect> children = effect switch {
                RandomSpellEffect random => random.m_effectList,
                VariableSpellEffect variable => variable.m_effectList,
                EffectListSpellEffect list => list.m_effectList,
                _ => []
            };
            foreach (SpellEffect child in Effects(children)) {
                yield return child;
            }
        }
    }

    private static void Require(bool condition, string message) {
        _checks++;
        if (!condition) { throw new InvalidDataException(message); }
    }
}
