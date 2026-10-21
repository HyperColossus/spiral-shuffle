using System;
using System.Collections.Generic;
using System.Linq;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game;

internal static class QaGear {
    private static readonly Lazy<HashSet<uint>> Templates = new(() => CoreObjectFactory.TemplateManifest.m_serializedTemplates
        .Where(location => location.m_filename.ToString().StartsWith("ObjectData/QA_Gear/", StringComparison.OrdinalIgnoreCase))
        .Select(location => location.m_id).ToHashSet());

    internal static bool IsQa(ulong templateId) => templateId <= uint.MaxValue &&
        Templates.Value.Contains(EquipmentItemCards.OriginalId((uint)templateId));

    internal static bool RemoveFrom(Wizard wizard) {
        HashSet<ulong> removed = (wizard.InventoryBehavior.Items ?? [])
            .Concat(wizard.EquipmentBehavior.EquippedItems ?? [])
            .Where(item => IsQa(item.m_templateID)).Select(item => item.m_globalID.Full).ToHashSet();
        if (removed.Count == 0) {
            return false;
        }

        wizard.InventoryBehavior.Items?.RemoveAll(item => removed.Contains(item.m_globalID));
        wizard.InventoryBehavior.InventoryItemIds?.RemoveAll(removed.Contains);
        wizard.EquipmentBehavior.EquippedItems?.RemoveAll(item => removed.Contains(item.m_globalID));
        wizard.EquipmentBehavior.EquippedItemIds?.RemoveAll(removed.Contains);
        wizard.EquipmentBehavior.SlotList?.RemoveAll(slot => removed.Contains(slot.ItemId));
        return true;
    }
}
