/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using Akka.Actor;
using Imcodec.CoreObject;
using Imcodec.Cryptography;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imlight.Common;
using Imlight.CoreLib.Game.CrownShop;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imlight.CoreLib.Game.Services;

internal class CrownShopService(SessionActor sessionActor) : MessageService(sessionActor) {
    private const uint ReagentPropertyFlags =
        (uint) (PropertyFlags.Prop_Save | PropertyFlags.Prop_Copy | PropertyFlags.Prop_Transmit | PropertyFlags.Prop_AuthorityTransmit);

    private static readonly CoreObjectSerializer s_coreObjectSerializer = new(behaviors: SerializerFlags.None);
    private static readonly ObjectSerializer s_nonVersionableSerializer = new(Versionable: false, Behaviors: SerializerFlags.None);

    private static readonly int s_errorNotEnoughCrowns = (int) StringHash.Compute("Error_NotEnoughCrowns");
    private static readonly int s_errorCantAfford = (int) StringHash.Compute("Error_BuyerCantAffordItem");
    private static readonly int s_errorInvFull = (int) StringHash.Compute("Error_CantHoldItemInventoryFull");
    private static readonly int s_errorMaxGold = (int) StringHash.Compute("Error_CantHoldItemMaxGold");
    private static readonly int s_errorMaxLunari = (int) StringHash.Compute("CantHoldItemMaxLunari");
    private static readonly int s_errorInCombat = (int) StringHash.Compute("Error_InCombat");
    private static readonly int s_errorNotInCombat = (int) StringHash.Compute("Error_NotInCombat");
    private static readonly int s_errorCSDisabled = (int) StringHash.Compute("Error_CrownShopDisabled");
    private static readonly int s_errorPurchase = (int) StringHash.Compute("Error_PriceLockMismatch");
    private static readonly int s_errorNoGiftThisItem = (int) StringHash.Compute("Error_NoGiftThisItem");
    private static readonly int s_errorNoPriceForType = (int) StringHash.Compute("Error_PriceLockNoPriceForType");

    protected static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CrownShopService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_SEGDATA_REQUEST))]
    private void ReceiveCrownShopSegDataRequest(WIZARD_12_PROTOCOL.MSG_PCS_SEGDATA_REQUEST message) {
        var wizard = GetActiveWizard();
        var schoolStr = wizard.MagicSchoolBehavior?.MagicSchool.ToString() ?? "Balance";
        var schoolCode = schoolStr.Length >= 2 ? schoolStr[..2] : schoolStr;

        var segmentationInputData = new SegmentationInputData {
            m_bIsValidSegmentationData = true,
            m_playerLevel = wizard.MagicSchoolBehavior?.Level ?? 1,
            m_playerSchoolOfFocus = schoolCode,
            m_accountNDaysAged = (int) (DateTime.Now - wizard.Account.CreationTime).TotalDays,
            m_accountNDaysSinceLastLogin = 0,
            m_accountNDaysSinceLastPurchase = 0,
            m_accountNDaysLastCrownsPurchase = 0,
            m_accountIsMember = 0,
            m_accountIsCSR = wizard.Account.AuthLevel > AuthLevel.None ? 1 : 0,
            m_accountNCrownsSpent = 0,
            m_accountNCrownsInWallet = wizard.Account.Crowns,
            m_accountNDaysSinceItemPurchased = [],
            m_numOfParticularItemInInventory = [],
            m_numItemsOfCategoryInInventory = [],
            m_playerHasBadge = [],
            m_accountNHighestWorld = 0
        };

        if (!s_nonVersionableSerializer.Serialize(segmentationInputData, PropertyFlags.Prop_Transmit | PropertyFlags.Prop_AuthorityTransmit, out var serializedData)) {
            Logger.Error("Failed to serialize SegmentationInputData.");
            return;
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_SEGDATA_RESPONSE {
            Success = 1,
            Data = serializedData
        });

        SyncCrownBalance(wizard);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_LIST_REQUEST))]
    private void ReceiveCrownShopListRequest(WIZARD_12_PROTOCOL.MSG_PCS_LIST_REQUEST message) {
        var nextUpdateId = message.UpdateID == 0 ? 1u : message.UpdateID + 1;
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_LIST_RESPONSE {
            Data = CrownShopHandler.GetCrownShopData(),
            Updates = string.Empty,
            UpdateID = nextUpdateId,
            Error = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_PRICE_LOCK_REQUEST))]
    private void ReceivePriceLockReq(WIZARD_12_PROTOCOL.MSG_PCS_PRICE_LOCK_REQUEST message) {
        Logger.Information("Received MSG_PCS_PRICE_LOCK_REQUEST for item {0}", Logger.Args(message.Item));

        if (message.SaleID != 0) {
            Logger.Warning("SaleID {0} is not supported. Ignoring.", Logger.Args(message.SaleID));
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_PRICE_LOCK_RESPONSE {
                Error = s_errorPurchase
            });
            return;
        }

        if (!CrownShopHandler.TryGetCrownShopItem(message.Item, out var item)) {
            Logger.Warning("Item {0} not found in CrownShop.", Logger.Args(message.Item));
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_PRICE_LOCK_RESPONSE {
                Error = s_errorPurchase
            });
            return;
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_PRICE_LOCK_RESPONSE {
            CostCrowns = item.m_crownsCost,
            CostGold = item.m_goldCost,
            CostTickets = 0,
            Error = 0,
            Item = message.Item
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_PURCHASE_REQUEST))]
    private void ReceivePurchaseRequest(WIZARD_12_PROTOCOL.MSG_PCS_PURCHASE_REQUEST message) {
        var wizard = GetActiveWizard();

        if (message.Count < 1 || message.Count > CrownShopHandler.s_maxBuyCount || message.SaleID != 0) {
            Logger.Warning("Rejected purchase request for item {0}.", Logger.Args(message.Item));
            SendPurchaseError(s_errorPurchase);
            return;
        }

        if (!CrownShopHandler.TryGetCrownShopItem(message.Item, out var catalogItem)) {
            Logger.Warning("Item {0} not found in CrownShop.", Logger.Args(message.Item));
            SendPurchaseError(s_errorPurchase);
            return;
        }

        if (wizard.IsInDuel && !catalogItem.m_combatOnly) {
            SendPurchaseError(s_errorInCombat);
            return;
        }

        if (!wizard.IsInDuel && catalogItem.m_combatOnly) {
            SendPurchaseError(s_errorNotInCombat);
            return;
        }

        var isGift = message.Recipient != 0;
        if (isGift && catalogItem.m_noGift) {
            SendPurchaseError(s_errorNoGiftThisItem);
            return;
        }

        int amountToPay;
        switch (message.Type) {
            case 0: // Gold
                amountToPay = (int) message.Count * catalogItem.m_goldCost;
                if (wizard.GameStats.m_currentGold < amountToPay) {
                    SendPurchaseError(s_errorCantAfford);
                    return;
                }
                break;

            case 1: // Crowns
                amountToPay = (int) message.Count * catalogItem.m_crownsCost;
                if (wizard.Account.Crowns < amountToPay) {
                    SendPurchaseError(s_errorNotEnoughCrowns);
                    return;
                }
                break;

            default:
                Logger.Warning("Invalid currency type {0} for item {1}.", Logger.Args(message.Type, message.Item));
                SendPurchaseError(s_errorNoPriceForType);
                return;
        }

        var deliveryError = TryGrantPurchasedItem(wizard, message);
        if (deliveryError != 0) {
            SendPurchaseError(deliveryError);
            return;
        }

        if (message.Type == 0) {
            wizard.RemoveGold(amountToPay);
            SyncGoldBalance(wizard);
        }
        else if (message.Type == 1) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - amountToPay);
            SyncCrownBalance(wizard);
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_PURCHASE_RESPONSE {
            Item = message.Item,
            Error = 0,
            Cost = amountToPay,
            Count = message.Count,
            Gifted = isGift ? (byte) 1 : (byte) 0,
            Type = message.Type
        });
    }

    private int TryGrantPurchasedItem(Wizard wizard, WIZARD_12_PROTOCOL.MSG_PCS_PURCHASE_REQUEST message) {
        var itemTemplate = CoreObjectFactory.GetCoreTemplate(message.Item);

        switch (itemTemplate) {
            case GoldAmountTemplate goldItem:
                if (wizard.GameStats.m_currentGold + (goldItem.m_goldAmount * message.Count) >= wizard.GameStats.m_baseGoldPouch) {
                    return s_errorMaxGold;
                }
                wizard.AddGold(goldItem.m_goldAmount * message.Count);
                SyncGoldBalance(wizard);
                return 0;

            case LunariAmountTemplate lunariTemplate:
                if (wizard.GameStats.m_currentEventCurrency1 + (lunariTemplate.m_lunariAmount * message.Count) >= wizard.GameStats.m_baseEventCurrency1Pouch) {
                    return s_errorMaxLunari;
                }
                wizard.GameStats.m_currentEventCurrency1 += (int) (lunariTemplate.m_lunariAmount * message.Count);
                SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEEVENTCURRENCY1 {
                    EventCurrency1 = wizard.GameStats.m_currentEventCurrency1,
                    MaxEventCurrency1 = wizard.GameStats.m_baseEventCurrency1Pouch
                });
                return 0;

            case ReagentItemTemplate reagentTemplate:
                return TryGrantReagent(wizard, reagentTemplate.m_templateID, (int) message.Count);

            case PetSnackItemTemplate petSnackTemplate:
                return TryGrantPetSnack(wizard, petSnackTemplate.m_templateID, (int) message.Count);

            case WizItemTemplate:
                for (var i = 0; i < message.Count; i++) {
                    if (!TryAddItemToInventory(wizard, message.Item, message.Texture, message.Decal)) {
                        return s_errorInvFull;
                    }
                }
                return 0;

            default:
                for (var i = 0; i < message.Count; i++) {
                    if (!TryAddItemToInventory(wizard, message.Item, message.Texture, message.Decal)) {
                        return s_errorInvFull;
                    }
                }
                return 0;
        }
    }

    private int TryGrantReagent(Wizard wizard, ulong reagentTemplateId, int count) {
        ClientReagentItem? reagentObj = null;
        for (var i = 0; i < count; i++) {
            if (!wizard.AddReagent(reagentTemplateId, out reagentObj)) {
                Logger.Warning("Could not add reagent {0} to inventory.", Logger.Args(reagentTemplateId));
                return s_errorInvFull;
            }
        }

        if (reagentObj is null || !s_coreObjectSerializer.Serialize(reagentObj, ReagentPropertyFlags, out var serializedReagentObj)) {
            Logger.Warning("Failed to serialize reagent core object {0}.", Logger.Args(reagentTemplateId));
            return s_errorPurchase;
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_REAGENTADD {
            GlobalID = wizard.GameObjectID,
            Data = serializedReagentObj
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
            ItemGlobalID = reagentObj.m_globalID,
            ItemTemplateID = (uint) reagentObj.m_templateID,
            ItemLocation = 1
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_REAGENTUPDATE {
            GlobalID = wizard.GameObjectID,
            ItemID = reagentObj.m_globalID,
            Quantity = reagentObj.m_quantity
        });

        return 0;
    }

    private int TryGrantPetSnack(Wizard wizard, ulong snackTemplateId, int count) {
        ClientPetSnackItem? snackObj = null;
        for (var i = 0; i < count; i++) {
            if (!wizard.AddSnack(snackTemplateId, out snackObj)) {
                Logger.Warning("Could not add pet snack {0} to snack bag.", Logger.Args(snackTemplateId));
                return s_errorInvFull;
            }
        }

        if (snackObj is null || !s_coreObjectSerializer.Serialize(snackObj, 24, out var serializedSnackObj)) {
            Logger.Warning("Failed to serialize pet snack core object {0}.", Logger.Args(snackTemplateId));
            return s_errorPurchase;
        }

        SendToSocket(new PET_9_PROTOCOL.MSG_PETSNACKADD {
            GlobalID = wizard.GameObjectID,
            Data = serializedSnackObj
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
            ItemGlobalID = snackObj.m_globalID,
            ItemTemplateID = (uint) snackObj.m_templateID,
            ItemLocation = 1
        });

        SendToSocket(new PET_9_PROTOCOL.MSG_PETSNACKUPDATE {
            GlobalID = wizard.GameObjectID,
            ItemID = snackObj.m_globalID,
            Quantity = snackObj.m_quantity
        });

        return 0;
    }

    private bool TryAddItemToInventory(Wizard wizard, ulong itemTemplateId, int texture, int decal) {
        var obj = CoreObjectFactory.FinalizeCoreObject(itemTemplateId);
        if (obj is not WizClientObjectItem itemCoreObject) {
            Logger.Warning("Template {0} is not a WizClientObjectItem.", Logger.Args(itemTemplateId));
            return false;
        }

        itemCoreObject.m_characterId = (GID) wizard.CharId;
        if (texture != 0) {
            itemCoreObject.m_primaryColor = texture;
        }
        if (decal != 0) {
            itemCoreObject.m_secondaryColor = decal;
        }

        if (!wizard.AddItemToInventory(itemCoreObject)) {
            Logger.Warning("Could not add item {0} to inventory.", Logger.Args(itemTemplateId));
            return false;
        }

        if (!s_coreObjectSerializer.Serialize(itemCoreObject, 24, out var serializedItem)) {
            Logger.Warning("Failed to serialize core object {0}.", Logger.Args(itemTemplateId));
            return false;
        }

        SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
            GlobalID = wizard.GameObjectID,
            SerializedItem = serializedItem
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
            ItemGlobalID = itemCoreObject.m_globalID,
            ItemLocation = 1,
            ItemTemplateID = (uint) itemTemplateId
        });

        return true;
    }

    private void SendPurchaseError(int errorCode)
        => SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_PURCHASE_RESPONSE {
            Error = errorCode
        });

    private void SyncCrownBalance(Wizard wizard)
        => SendToSocket(new WIZARD_12_PROTOCOL.MSG_CROWNBALANCE {
            Failure = 0,
            TotalCrowns = wizard.Account.Crowns,
            CharacterID = wizard.CharId,
            CacheBalanceForCSSegmentation = 1
        });

    private void SyncGoldBalance(Wizard wizard)
        => SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold,
            MaxGold = wizard.GameStats.m_baseGoldPouch
        });

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_UPDATEUSERWISHLIST))]
    private void ReceiveWishlistUpdate(WIZARD_12_PROTOCOL.MSG_PCS_UPDATEUSERWISHLIST message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PCS_UPDATEUSERWISHLIST {
            GlobalID = wizard.GameObjectID,
            Data = message.Data,
            WLItemsRemAftPur = message.WLItemsRemAftPur
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CrownShopLogging))]
    private void ReceiveCrownShopLogging(WIZARD2_53_PROTOCOL.MSG_CrownShopLogging message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CrownShopLogging {
            Enabled = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_PATCH))]
    private void ReceivePCsPatch(WIZARD_12_PROTOCOL.MSG_PCS_PATCH message) {
        Logger.Debug("Received MSG_PCS_PATCH");
    }
}
