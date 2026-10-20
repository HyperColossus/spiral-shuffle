using System;
using System.Collections.Generic;
using System.Linq;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;

namespace Imlight.CoreLib.Game;

internal sealed class EquipmentCard {
    public uint Original { get; set; }
    public uint Donor { get; set; }
    public uint[] Items { get; set; } = [];
    public uint[] Spells { get; set; } = [];
    public double[] Multipliers { get; set; } = [];
}

internal static class EquipmentItemCards {
    private static readonly Lazy<AccountToggleStore> Settings = new(() => new("Config/Randomizer-item-cards.json"));
    private static readonly Lazy<Dictionary<uint, EquipmentCard>> Cards = new(() =>
        RandomizerRuntime.EquipmentCardPool.ToDictionary(card => card.Original));
    private static readonly Lazy<Dictionary<uint, uint>> Originals = new(() =>
        RandomizerRuntime.EquipmentCardPool.SelectMany(card => card.Items.Select(id => (Id: id, card.Original)))
            .ToDictionary(pair => pair.Id, pair => pair.Original));
    private static readonly Lazy<HashSet<uint>> SpellIds = new(() =>
        RandomizerRuntime.EquipmentCardPool.SelectMany(card => card.Spells).ToHashSet());

    internal static bool IsEnabled(ulong accountId) => RandomizerRuntime.Enabled && Settings.Value.Get(accountId);

    internal static bool IsSpellId(uint id) => RandomizerRuntime.Enabled && SpellIds.Value.Contains(id);

    internal static uint OriginalId(uint id) => Originals.Value.GetValueOrDefault(id, id);

    internal static ulong TemplateId(ulong templateId, ulong accountId) {
        if (!RandomizerRuntime.Enabled || templateId > uint.MaxValue) {
            return templateId;
        }

        uint original = OriginalId((uint)templateId);
        int difficulty = CreatureRandomization.Difficulty(RandomizerRuntime.GetMode(accountId));
        return IsEnabled(accountId) && Cards.Value.TryGetValue(original, out EquipmentCard card) && card.Items.Length == 3
            ? card.Items[difficulty - 1] : original;
    }

    internal static void Apply(Wizard wizard, WizClientObjectItem item) {
        if (item is not null) {
            item.m_templateID = (GID)TemplateId(item.m_templateID, wizard.AccountId);
        }
    }

    internal static void Apply(Wizard wizard) {
        foreach (WizClientObjectItem item in (wizard.InventoryBehavior.Items ?? [])
                     .Concat(wizard.EquipmentBehavior.EquippedItems ?? [])) {
            Apply(wizard, item);
        }
    }

    internal static void PrepareRecalculation(Wizard wizard) {
        wizard.SpellbookBehavior?.TemporarySpells.RemoveAll(spell => IsSpellId(spell.m_templateID));
    }

    internal static List<GID> ShopItems(Wizard wizard, IReadOnlyList<GID> original) => original.Select(id => {
        GID result = id;
        result.MParts.TemplateId = (uint)TemplateId(id.MParts.TemplateId, wizard.AccountId);
        return result;
    }).ToList();

    internal static void ApplyLoot(Wizard wizard, DropTableResult results) {
        foreach (var item in results.Items) {
            if (uint.TryParse(item.ItemId, out uint id)) {
                item.ItemId = TemplateId(id, wizard.AccountId).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }

    internal static bool HandleMenuSetting(ulong accountId, ByteString message, out byte[] response) {
        response = [];
        if (!RandomizerRuntime.TryReadMenuMessage(message, "SpiralItemCards", out int score) ||
            score is not (0 or 2 or -2)) {
            return false;
        }

        if (score != 0) {
            Settings.Value.Set(accountId, score > 0);
        }

        byte[] packet = MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS {
            score = IsEnabled(accountId) ? 1 : -1, gameName = "SpiralItemCards"
        });
        response = packet[8..];
        return true;
    }
}
