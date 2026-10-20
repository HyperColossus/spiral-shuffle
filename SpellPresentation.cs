using System.Linq;
using System.Reflection;
using Imcodec.ObjectProperty.TypeCache;

namespace Imlight.CoreLib.Game;

internal static class SpellPresentation {
    private static readonly MethodInfo CopyMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;

    internal static WizClientObject ForClient(WizClientObject source, ulong accountId) {
        int mode = RandomizerRuntime.GetMode(accountId);
        if (mode == 0 || CreatureRandomization.Difficulty(mode) == 1) {
            return source;
        }

        return Project(source, mode);
    }

    private static T Project<T>(T source, int mode) where T : CoreObject {
        T copy = Copy(source);
        copy.m_inactiveBehaviors = source.m_inactiveBehaviors?.Select(behavior => ProjectBehavior(behavior, mode)).ToList();
        return copy;
    }

    private static BehaviorInstance ProjectBehavior(BehaviorInstance source, int mode) {
        switch (source) {
            case ClientSpellbookBehavior book:
                ClientSpellbookBehavior bookCopy = Copy(book);
                bookCopy.m_spellIDList = book.m_spellIDList?.Select(spell => {
                    SpellIDTracker copy = Copy(spell);
                    copy.m_spellID = RandomizerRuntime.CombatSpellId(spell.m_spellID, mode);
                    return copy;
                }).ToList();
                return bookCopy;
            case DeckBehavior deck:
                DeckBehavior deckCopy = Copy(deck);
                deckCopy.m_spellList = deck.m_spellList?.Select(spell => {
                    SpellData copy = Copy(spell);
                    copy.m_templateID = RandomizerRuntime.CombatSpellId(spell.m_templateID, mode);
                    return copy;
                }).ToList();
                return deckCopy;
            case ClientWizEquipmentBehavior equipment:
                ClientWizEquipmentBehavior equipmentCopy = Copy(equipment);
                equipmentCopy.m_itemList = equipment.m_itemList?.Select(item => Project(item, mode)).ToList();
                return equipmentCopy;
            case ClientWizInventoryBehavior inventory:
                ClientWizInventoryBehavior inventoryCopy = Copy(inventory);
                inventoryCopy.m_itemList = inventory.m_itemList?.Select(item => Project(item, mode)).ToList();
                return inventoryCopy;
            default:
                return source;
        }
    }

    // Generated wire types have no copy API. Copy only branches whose spell IDs change.
    private static T Copy<T>(T source) where T : class => (T)CopyMethod.Invoke(source, null)!;
}
