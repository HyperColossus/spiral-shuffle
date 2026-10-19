using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Requirements;
using Imlight.CoreLib.Game.Requirements.Contexts;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Newtonsoft.Json;

namespace Imlight.CoreLib.Game;

internal static class RandomizerRuntime {
    internal const uint MenuJobId = 770101;
    private const string CatalogPath = "Config/Randomizer.json";
    private const string ModesPath = "Config/Randomizer-modes.json";
    private static readonly object ModeLock = new();
    private static readonly Lazy<Catalog> Data = new(() =>
        JsonConvert.DeserializeObject<Catalog>(File.ReadAllText(CatalogPath)) ?? new Catalog());
    private static readonly Lazy<Dictionary<ulong, int>> Modes = new(() =>
        File.Exists(ModesPath)
            ? JsonConvert.DeserializeObject<Dictionary<ulong, int>>(File.ReadAllText(ModesPath)) ?? []
            : []);
    private static readonly Lazy<Dictionary<ulong, uint>> CreatureAliases = new(() =>
        Data.Value.CreatureSwaps.ToDictionary(swap => (ulong)swap.From, swap => swap.Alias));
    private static readonly Lazy<Dictionary<(uint Original, int Difficulty), uint>> SpellVariants = new(() =>
        Data.Value.SpellVariants.ToDictionary(variant => (variant.Original, variant.Difficulty), variant => variant.TemplateId));

    internal static bool Enabled => File.Exists(CatalogPath);

    internal static string ModeName(int mode) => CreatureRandomization.RealmName(mode);

    internal static int GetMode(ulong accountId) {
        if (!Enabled) {
            return 0;
        }

        lock (ModeLock) {
            return Modes.Value.GetValueOrDefault(accountId, 1);
        }
    }

    internal static void SetMode(ulong accountId, int mode) {
        if (!Enabled || mode < 1 || mode > 6) {
            return;
        }

        lock (ModeLock) {
            Modes.Value[accountId] = mode;
            File.WriteAllText(ModesPath + ".new", JsonConvert.SerializeObject(Modes.Value));
            File.Move(ModesPath + ".new", ModesPath, true);
        }

        Logger.Information("Spiral Shuffle selected {Mode} for account {Account}.", Logger.Args(ModeName(mode), accountId));
    }

    internal static bool TryReadChoice(ByteString message, out int mode) {
        return TryReadOptions(message, out mode, out _);
    }

    internal static bool TryReadOptions(ByteString message, out int mode, out bool vendors) {
        mode = 0;
        vendors = false;
        if (!Enabled) {
            return false;
        }

        try {
            byte[] raw = message;
            var choice = new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS();
            if (raw.Length < 4 || raw[0] != choice.ServiceId || raw[1] != choice.MessageOrder) {
                return false;
            }

            int length = BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(2));
            if (length < 4 || length > raw.Length) {
                return false;
            }

            choice.Decode(new BitReader(raw[4..length]));
            int selectedMode = choice.score & 15;
            bool selectedVendors = (choice.score & 16) != 0;
            if (choice.gameName != "SpiralShuffle" || selectedMode < 1 || selectedMode > 6 ||
                choice.score != selectedMode + (selectedVendors ? 16 : 0)) {
                return false;
            }

            mode = selectedMode;
            vendors = selectedVendors;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or OverflowException) {
            return false;
        }
    }

    internal static int ModeForPath(string actorPath) {
        int basePort = ConfigurationManager.Settings["Game Server.GameServerPort"].AsInt(12343);
        return CreatureRandomization.ModeForPath(actorPath, basePort);
    }

    internal static CoreTemplate CreatureTemplate(CoreTemplate original, ulong originalId, string actorPath) {
        if (!Enabled || original is null) {
            return original;
        }

        ulong id = ClientCreatureId(originalId, actorPath);
        return id == originalId ? original : CoreObjectFactory.GetCoreTemplate(id) ?? original;
    }

    internal static ulong ClientCreatureId(ulong original, string actorPath) =>
        Enabled && ModeForPath(actorPath) > 3 && CreatureAliases.Value.TryGetValue(original, out uint alias)
            ? alias : original;

    internal static CreatureRoll RollCreature(string actorPath) => Enabled
        ? CreatureRandomization.Create(ModeForPath(actorPath), Random.Shared)
        : new CreatureRoll(1, 1, 0, 0, 1);

    internal static uint CombatSpellId(uint original, int mode) =>
        Enabled && mode > 0 && SpellVariants.Value.TryGetValue((original, CreatureRandomization.Difficulty(mode)), out uint id)
            ? id : original;

    internal static void RandomizeLoot(IActorRef playerActor, Wizard wizard, DropTableResult results) {
        if (!Enabled) {
            return;
        }

        int mode = CreatureRandomization.Difficulty(GetMode(wizard.AccountId));
        int level = wizard.MagicSchoolBehavior.Level;
        string school = wizard.MagicSchoolBehavior.MagicSchool.ToString();
        Dictionary<uint, Equipment> items = Data.Value.Equipment.ToDictionary(item => item.Id);
        int window = mode == 3 ? 30 : mode == 2 ? 20 : 10;
        foreach (var reward in results.Items) {
            if (!uint.TryParse(reward.ItemId, out uint id) || !items.TryGetValue(id, out Equipment original)) {
                continue;
            }

            Equipment[] candidates = Data.Value.Equipment.Where(item =>
                item.Level <= level && item.Level >= Math.Max(0, level - window) &&
                (item.School is "All" or "" || item.School == school)).OrderBy(item => item.Id).ToArray();
            if (candidates.Length == 0) {
                continue;
            }

            int start = Random.Shared.Next(candidates.Length);
            for (int attempt = 0; attempt < candidates.Length; attempt++) {
                Equipment selected = candidates[(start + attempt) % candidates.Length];
                if (CoreObjectFactory.GetCoreTemplate(selected.Id) is not WizItemTemplate template) {
                    continue;
                }

                var context = new GenericRequirementContext(template.m_equipRequirements, playerActor, wizard.GameObject, wizard);
                if (template.m_equipRequirements is not null &&
                    !RequirementDispatcher.EvaluateRequirements(template.m_equipRequirements, context)) {
                    continue;
                }

                reward.ItemId = selected.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                reward.ItemName = selected.Name;
                break;
            }
        }

        double multiplier = mode == 3 ? 1.5 : mode == 2 ? 1.2 : 1.0;
        results.GoldAmount = (int)Math.Round(results.GoldAmount * multiplier);
    }

    private sealed class Catalog {
        public string Seed { get; set; } = "SPIRAL-101";
        public List<Equipment> Equipment { get; set; } = [];
        public List<CreatureSwap> CreatureSwaps { get; set; } = [];
        public List<SpellVariant> SpellVariants { get; set; } = [];
    }

    private sealed class CreatureSwap {
        public uint From { get; set; }
        public uint Alias { get; set; }
    }

    private sealed class SpellVariant {
        public uint Original { get; set; }
        public int Difficulty { get; set; }
        public uint TemplateId { get; set; }
    }

    private sealed class Equipment {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string Slot { get; set; } = "";
        public string School { get; set; } = "";
        public int Level { get; set; }
    }
}
