using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.WizBang;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;

namespace Imlight.CoreLib.Game.Zone.Components;

internal sealed class InteractTrainerComponent(ZoneEntity entity) : ZoneEntityComponent(entity), IServiceComponent, IComponentFactory {
    public string ServiceName => "WizTrainingService";
    public string NpcIcon => null;
    public string NpcNameKey => null;
    public string NpcTextKey => "GUI_NPCInteractText";
    public WizBangs WizBang => WizBangs.Training;
    public string StateName => "Shop";
    public string InteractWizBang => "Registrar";
    public string DisplayKey => "GUI_ShopOptionEquipment";
    public List<NPCSpellEntry> SpellInventory { get; private set; } = [];

    public static bool ShouldAttachToEntity(CoreTemplate template) => template is GameObjectTemplate npc &&
        npc.m_behaviors.Any(behavior => behavior is NPCBehaviorTemplate) &&
        NpcSpellInventoryCollection.TryGetNpcInventory(npc.m_templateID, out _);

    public override void OnStart() {
        if (Entity.Template is GameObjectTemplate npc &&
            NpcSpellInventoryCollection.TryGetNpcInventory(npc.m_templateID, out NPCSpellInventory inventory)) {
            SpellInventory = inventory.Spells;
        }
    }

    public IEnumerable<ServiceOptionBase> GetServiceOptions(Wizard wizard) {
        if (wizard is null) {
            return [];
        }

        List<ServiceOptionBase> options = [];
        for (int index = 0; index < SpellInventory.Count; index++) {
            NPCSpellEntry entry = SpellInventory[index];
            if (wizard.SpellbookBehavior.HasSpell((uint)entry.TemplateID) ||
                CoreObjectFactory.GetCoreTemplate(TrainerRules.ClientId(wizard, entry.TemplateID)) is not SpellTemplate spell) {
                continue;
            }

            RequirementList requirements = null;
            if (entry.RequiredSpellID != 0 &&
                CoreObjectFactory.GetCoreTemplate(TrainerRules.ClientId(wizard, entry.RequiredSpellID)) is SpellTemplate prerequisite) {
                requirements = new RequirementList { m_operator = Operator.ROP_AND,
                    m_requirements = [new ReqHasSpell { m_operator = Operator.ROP_AND, m_spellName = prerequisite.m_name }] };
            }

            options.Add(new WizTrainingOption {
                m_iconKey = WizBang.ToString(),
                m_serviceName = ServiceName,
                m_spellName = spell.m_name,
                m_displayKey = spell.m_displayName,
                m_requiredLevel = entry.Level,
                m_trainingIndex = index,
                m_trainingCost = TrainerRules.Cost(wizard, entry),
                m_bCanTrain = TrainerRules.CanTrain(wizard, entry),
                m_requirements = requirements,
                m_failedRequirement = requirements is not null && !wizard.SpellbookBehavior.HasSpell((uint)entry.RequiredSpellID)
                    ? requirements.m_requirements[0] : null
            });
        }
        return options;
    }

    public void OnServiceInteraction(IActorRef playerActor, Wizard playerCharacter, CoreObject playerObject, uint serviceOptionIndex) { }
}
