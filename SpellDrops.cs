using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Newtonsoft.Json;

namespace Imlight.CoreLib.Game;

internal sealed class SpellDropSettingsStore {
    private readonly string _path;
    private readonly object _lock = new();
    private readonly Dictionary<ulong, int> _settings;

    internal SpellDropSettingsStore(string path) {
        _path = path;
        _settings = File.Exists(path)
            ? JsonConvert.DeserializeObject<Dictionary<ulong, int>>(File.ReadAllText(path))
                ?? throw new InvalidDataException("Invalid saved spell-drop settings.")
            : [];
    }

    internal int Get(ulong accountId) {
        lock (_lock) {
            int setting = _settings.GetValueOrDefault(accountId, SpellDropRules.DefaultSetting);
            return SpellDropRules.IsValidSetting(setting) ? setting : SpellDropRules.DefaultSetting;
        }
    }

    internal void Set(ulong accountId, int setting) {
        if (!SpellDropRules.IsValidSetting(setting)) {
            throw new ArgumentOutOfRangeException(nameof(setting));
        }

        lock (_lock) {
            _settings[accountId] = setting;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
            File.WriteAllText(_path + ".new", JsonConvert.SerializeObject(_settings));
            File.Move(_path + ".new", _path, true);
        }
    }
}

internal static class SpellDrops {
    private static readonly Lazy<SpellDropSettingsStore> Settings = new(() => new("Config/Randomizer-spell-drops.json"));

    internal static int GetSetting(ulong accountId)
        => RandomizerRuntime.Enabled ? Settings.Value.Get(accountId) : SpellDropRules.DefaultSetting;

    internal static bool HandleMenuSetting(ulong accountId, ByteString message, out ByteString response) {
        response = "";
        if (!RandomizerRuntime.TryReadMenuMessage(message, "SpiralSpellDrops", out int requested) ||
            (requested != 0 && !SpellDropRules.IsValidSetting(requested))) {
            return false;
        }

        if (requested != 0) {
            Settings.Value.Set(accountId, requested);
        }

        byte[] encoded = MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS {
            score = GetSetting(accountId), gameName = "SpiralSpellDrops"
        });
        response = (ByteString)encoded[8..];
        return true;
    }

    internal static void AddCreatureDrop(Wizard wizard, DropTableResult loot, double encounterUnit) {
        int chance = SpellDropRules.Chance(GetSetting(wizard.AccountId), encounterUnit);
        if (!SpellDropRules.Wins(chance, RandomNumberGenerator.GetInt32(SpellDropRules.Scale))) {
            return;
        }

        uint[] pool = SpellDropRules.Available(RandomizerRuntime.DropSpellPool,
            wizard.SpellbookBehavior.LearnedSpellTemplateIds.ToHashSet(),
            CreatureRandomization.Difficulty(RandomizerRuntime.GetMode(wizard.AccountId)));
        if (pool.Length == 0) {
            return;
        }

        uint selected = pool[RandomNumberGenerator.GetInt32(pool.Length)];
        loot.Items.Add(new DropItemResult {
            ItemId = selected.ToString(CultureInfo.InvariantCulture), Quantity = 1
        });
    }
}
