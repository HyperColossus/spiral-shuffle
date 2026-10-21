using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Newtonsoft.Json;

namespace Imlight.CoreLib.Game;

internal sealed class VendorSettingsStore {
    private readonly string _path;
    private readonly object _lock = new();
    private readonly SavedSettings _settings;

    internal VendorSettingsStore(string path) {
        _path = path;
        _settings = File.Exists(path)
            ? JsonConvert.DeserializeObject<SavedSettings>(File.ReadAllText(path))
                ?? throw new InvalidDataException("Invalid saved vendor settings.")
            : new SavedSettings();
    }

    internal bool IsEnabled(ulong accountId) {
        lock (_lock) {
            return _settings.EnabledAccounts.GetValueOrDefault(accountId);
        }
    }

    internal void SetEnabled(ulong accountId, bool enabled) {
        lock (_lock) {
            _settings.EnabledAccounts[accountId] = enabled;
            Save();
        }
    }

    internal byte[] SeedForWizard(ulong characterId) {
        lock (_lock) {
            if (!_settings.WizardSeeds.TryGetValue(characterId, out string seed)) {
                do {
                    seed = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                } while (_settings.WizardSeeds.ContainsValue(seed));

                _settings.WizardSeeds[characterId] = seed;
                Save();
            }

            return Convert.FromBase64String(seed);
        }
    }

    private void Save() {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        File.WriteAllText(_path + ".new", JsonConvert.SerializeObject(_settings));
        File.Move(_path + ".new", _path, true);
    }

    private sealed class SavedSettings {
        public Dictionary<ulong, bool> EnabledAccounts { get; set; } = [];
        public Dictionary<ulong, string> WizardSeeds { get; set; } = [];
    }
}

internal static class VendorStock {
    internal static T[] Choose<T>(IEnumerable<T> candidates, Func<T, string> identity,
        byte[] wizardSeed, ulong vendorId, string shopKind, int originalCount) {
        int count = Math.Clamp(originalCount, 8, 48);
        return candidates.GroupBy(identity, StringComparer.Ordinal).Select(group => group.First())
            .Select(item => (Item: item, Key: Convert.ToHexString(HMACSHA256.HashData(wizardSeed,
                Encoding.UTF8.GetBytes($"{shopKind}:{vendorId}:{identity(item)}")))))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Take(count).Select(entry => entry.Item).ToArray();
    }
}

internal static class VendorRandomization {
    private static readonly Lazy<VendorSettingsStore> Settings = new(() => new("Config/Randomizer-vendors.json"));
    private static readonly ConcurrentDictionary<(ulong Wizard, ulong Vendor), GID[]> ItemRolls = new();
    private static readonly ConcurrentDictionary<(ulong Wizard, ulong Vendor), TreasureCardEntry[]> CardRolls = new();
    private static readonly Lazy<GID[]> ItemPool = new(() => SpiralDB.NpcInventories.Values
        .SelectMany(shop => shop.Inventory).DistinctBy(item => item.MParts.TemplateId)
        .Where(item => !QaGear.IsQa(item.MParts.TemplateId) &&
            CoreObjectFactory.GetCoreTemplate(item.MParts.TemplateId) is WizItemTemplate)
        .OrderBy(item => item.MParts.TemplateId).ToArray());
    private static readonly Lazy<TreasureCardEntry[]> CardPool = new(() => SpellFactory.GetTreasureCardTemplates()
        .Where(card => card.m_baseCost > 0 && card.m_effects is { Count: > 0 } &&
            !card.m_alwaysFizzle && !card.m_battlegroundsOnly)
        .Select(card => new TreasureCardEntry { SpellName = card.m_name, Price = card.m_baseCost })
        .DistinctBy(card => card.SpellName)
        .OrderBy(card => card.SpellName, StringComparer.Ordinal).ToArray());

    internal static bool IsEnabled(ulong accountId)
        => RandomizerRuntime.Enabled && Settings.Value.IsEnabled(accountId);

    internal static void SetEnabled(ulong accountId, bool enabled) {
        if (RandomizerRuntime.Enabled) {
            Settings.Value.SetEnabled(accountId, enabled);
        }
    }

    internal static List<GID> Items(Wizard wizard, ulong vendorId, IReadOnlyList<GID> original) {
        if (RandomizerRuntime.Enabled) {
            original = original.Where(item => !QaGear.IsQa(item.MParts.TemplateId)).ToArray();
        }
        if (!IsEnabled(wizard.AccountId) || ItemPool.Value.Length == 0) {
            return original.ToList();
        }

        GID[] roll = ItemRolls.GetOrAdd((wizard.CharId, vendorId), key => VendorStock.Choose(
            ItemPool.Value, item => item.MParts.TemplateId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Settings.Value.SeedForWizard(key.Wizard), key.Vendor, "items", original.Count));
        return roll.ToList();
    }

    internal static List<TreasureCardEntry> Cards(Wizard wizard, ulong vendorId, IReadOnlyList<TreasureCardEntry> original) {
        if (!IsEnabled(wizard.AccountId) || CardPool.Value.Length == 0) {
            return original.ToList();
        }

        TreasureCardEntry[] roll = CardRolls.GetOrAdd((wizard.CharId, vendorId), key => VendorStock.Choose(
            CardPool.Value, card => card.SpellName, Settings.Value.SeedForWizard(key.Wizard),
            key.Vendor, "cards", original.Count));
        return roll.Select(card => new TreasureCardEntry { SpellName = card.SpellName, Price = card.Price }).ToList();
    }
}
