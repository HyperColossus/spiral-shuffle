using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.CoreObject;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imlight.Common;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.Shared.Utilities;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class CraftingService(SessionActor sessionActor) : MessageService(sessionActor) {

    private readonly CoreObjectSerializer _itemSerializer = new(behaviors: SerializerFlags.None);
    private static readonly BindSerializer s_bindSerializer = new();

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CraftingService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_USERECIPE))]
    private void ReceiveUseRecipe(WIZARD_12_PROTOCOL.MSG_USERECIPE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var recipeNameStr = message.RecipeName.ToString();
        uint recipeTemplateId = CoreObjectFactory.GetCoreTemplateID(loc =>
            loc.m_filename.ToString().Contains(recipeNameStr, StringComparison.OrdinalIgnoreCase));

        var recipeTemplate = recipeTemplateId != 0
            ? CoreObjectFactory.GetCoreTemplate(recipeTemplateId) as RecipeTemplate
            : null;

        if (recipeTemplate is null) {
            Logger.Log.Warning("Recipe {0} not found for player {1}", Logger.Args(recipeNameStr, wizard.GameObjectID));
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_USERECIPE {
                RecipeName = message.RecipeName,
                FinalItemID = 0,
                Error = 1,
                Quantity = 0
            });
            return;
        }

        wizard.AlchemyBehavior ??= new ServerAlchemyBehavior();
        wizard.AlchemyBehavior.Reagents ??= [];

        // Check ingredients
        if (recipeTemplate.m_ingredients != null) {
            foreach (var ing in recipeTemplate.m_ingredients) {
                if (ing.m_itemID.Full != 0) {
                    var reagent = wizard.AlchemyBehavior.GetReagent(ing.m_itemID.Full);
                    if (reagent is null || reagent.m_quantity < ing.m_quantity) {
                        Logger.Log.Debug("Player missing reagent {0} (needs {1})", Logger.Args(ing.m_itemID.Full, ing.m_quantity));
                        SendToSocket(new WIZARD_12_PROTOCOL.MSG_USERECIPE {
                            RecipeName = message.RecipeName,
                            FinalItemID = 0,
                            Error = 2,
                            Quantity = 0
                        });
                        return;
                    }
                }
            }

            // Deduct ingredients
            foreach (var ing in recipeTemplate.m_ingredients) {
                if (ing.m_itemID.Full != 0) {
                    for (int i = 0; i < ing.m_quantity; i++) {
                        var reagent = wizard.AlchemyBehavior.GetReagent(ing.m_itemID.Full);
                        if (reagent != null) {
                            wizard.AlchemyBehavior.RemoveReagent(reagent.m_globalID, out var updated);
                            if (updated != null && updated.m_quantity > 0) {
                                SendToSocket(new WIZARD_12_PROTOCOL.MSG_REAGENTUPDATE {
                                    GlobalID = wizard.GameObjectID,
                                    ItemID = reagent.m_globalID,
                                    Quantity = updated.m_quantity
                                });
                            }
                            else {
                                SendToSocket(new WIZARD_12_PROTOCOL.MSG_REAGENTREMOVE {
                                    GlobalID = wizard.GameObjectID,
                                    ItemID = reagent.m_globalID
                                });
                            }
                        }
                    }
                }
            }
        }

        // Finalize crafted item
        ulong resultItemId = recipeTemplate.m_itemID.Full;
        var craftedObj = CoreObjectFactory.FinalizeCoreObject(resultItemId);
        ulong craftedGid = 0;
        if (craftedObj is WizClientObjectItem craftedItem) {
            wizard.AddItemToInventory(craftedItem);
            craftedGid = craftedItem.m_globalID;

            if (_itemSerializer.Serialize(craftedItem, 1, out var itemData)) {
                SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
                    GlobalID = wizard.GameObjectID,
                    SerializedItem = itemData
                });
            }

            SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
                ItemGlobalID = craftedItem.m_globalID,
                ItemTemplateID = (uint) resultItemId,
                ItemLocation = 1
            });
        }

        // Cooldown slot
        int cookTime = recipeTemplate.m_cookTime > 0 ? recipeTemplate.m_cookTime : 60;
        int finishTime = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds() + cookTime;
        var slot = new CraftingSlot {
            m_recipeName = message.RecipeName,
            m_timeFinished = finishTime,
            m_globalID = (GID) RandomGen.GenerateGUID()
        };
        wizard.AlchemyBehavior.CraftingSlots ??= [];
        wizard.AlchemyBehavior.CraftingSlots.Add(slot);

        if (s_bindSerializer.Serialize(slot, (PropertyFlags) 24, out ByteString slotBytes)) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTADD {
                GlobalID = wizard.GameObjectID,
                Data = slotBytes
            });
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_USERECIPE {
            RecipeName = message.RecipeName,
            FinalItemID = craftedGid,
            Error = 0,
            Quantity = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CLEARALLCRAFTINGSLOTS))]
    private void ReceiveClearAllCraftingSlots(WIZARD_12_PROTOCOL.MSG_CLEARALLCRAFTINGSLOTS message) {
        var wizard = GetActiveWizard();
        if (wizard?.AlchemyBehavior?.CraftingSlots == null) return;
        wizard.AlchemyBehavior.CraftingSlots.Clear();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CLEARALLCRAFTINGSLOTS {
            GlobalID = wizard.GameObjectID
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_RECIPEADD))]
    private void ReceiveRecipeAdd(WIZARD_12_PROTOCOL.MSG_RECIPEADD message) {
        SendToSocket(message);
    }
}
