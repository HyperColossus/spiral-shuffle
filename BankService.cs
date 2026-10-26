using Imcodec.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class BankService(SessionActor sessionActor) : MessageService(sessionActor) {

    private static readonly BindSerializer s_serializer = new();
    private const PropertyFlags ITEM_SERIALIZE_FLAGS = (PropertyFlags) 24;
    private const int DEFAULT_BANK_LIMIT = 100;
    private const int DEFAULT_SHARED_BANK_LIMIT = 100;

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BankService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_OPENBANK))]
    private void ReceiveOpenBank(WIZARD_12_PROTOCOL.MSG_OPENBANK message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;
        var account = wizard.Account;

        // 1. Send personal bank items
        var personalItems = wizard.BankItems ?? [];
        foreach (var item in personalItems) {
            if (s_serializer.Serialize(item, ITEM_SERIALIZE_FLAGS, out ByteString itemBytes)) {
                SendToSocket(new GAME_5_PROTOCOL.MSG_CSRBANKITEMS {
                    GlobalID = wizard.GameObjectID,
                    SharedBank = 0,
                    SerializedItem = itemBytes
                });
            }
        }
        SendToSocket(new GAME_5_PROTOCOL.MSG_CSRBANKITEMSDONE {
            GlobalID = wizard.GameObjectID,
            SharedBank = 0,
            Limit = wizard.BankLimit > 0 ? wizard.BankLimit : DEFAULT_BANK_LIMIT
        });

        // 2. Send shared bank items
        var sharedItems = account?.SharedBankItems ?? [];
        foreach (var item in sharedItems) {
            if (s_serializer.Serialize(item, ITEM_SERIALIZE_FLAGS, out ByteString itemBytes)) {
                SendToSocket(new GAME_5_PROTOCOL.MSG_CSRBANKITEMS {
                    GlobalID = wizard.GameObjectID,
                    SharedBank = 1,
                    SerializedItem = itemBytes
                });
            }
        }
        SendToSocket(new GAME_5_PROTOCOL.MSG_CSRBANKITEMSDONE {
            GlobalID = wizard.GameObjectID,
            SharedBank = 1,
            Limit = DEFAULT_SHARED_BANK_LIMIT
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_MOVEINVTOBANK))]
    private void ReceiveMoveInvToBank(WIZARD_12_PROTOCOL.MSG_MOVEINVTOBANK message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;
        var account = wizard.Account;

        var item = wizard.InventoryBehavior?.GetItem(message.GlobalID);
        if (item is null) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_INVTOBANKCONFIRM {
                ClientRequestID = message.ClientRequestID,
                OriginalGlobalID = message.GlobalID,
                GlobalID = message.GlobalID,
                UseShared = message.UseShared,
                Failure = 1
            });
            return;
        }

        bool isShared = message.UseShared != 0;
        int limit = isShared ? DEFAULT_SHARED_BANK_LIMIT : (wizard.BankLimit > 0 ? wizard.BankLimit : DEFAULT_BANK_LIMIT);
        int currentCount = isShared ? (account?.SharedBankItems?.Count ?? 0) : (wizard.BankItems?.Count ?? 0);

        if (currentCount >= limit) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_INVTOBANKCONFIRM {
                ClientRequestID = message.ClientRequestID,
                OriginalGlobalID = message.GlobalID,
                GlobalID = message.GlobalID,
                UseShared = message.UseShared,
                Failure = 2
            });
            return;
        }

        if (!wizard.RemoveItemFromInventory(message.GlobalID)) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_INVTOBANKCONFIRM {
                ClientRequestID = message.ClientRequestID,
                OriginalGlobalID = message.GlobalID,
                GlobalID = message.GlobalID,
                UseShared = message.UseShared,
                Failure = 1
            });
            return;
        }

        if (isShared && account != null) {
            account.SharedBankItems ??= [];
            account.SharedBankItemIds ??= [];
            account.SharedBankItems.Add(item);
            account.SharedBankItemIds.Add(item.m_globalID);
            item.m_characterId = (GID) 0;
            AccountCollection.UpdateSharedBank(account.AccountId, account.SharedBankItemIds);
            WizardItemCollection.UpdateItemCharacter(item.m_globalID, 0);
        } else {
            wizard.BankItems ??= [];
            wizard.BankItemIds ??= [];
            wizard.BankItems.Add(item);
            wizard.BankItemIds.Add(item.m_globalID);
            WizardCollection.UpdateCharacterItems(wizard);
        }

        SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM {
            GlobalID = wizard.GameObjectID,
            ItemID = message.GlobalID
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_INVTOBANKCONFIRM {
            ClientRequestID = message.ClientRequestID,
            OriginalGlobalID = message.GlobalID,
            GlobalID = message.GlobalID,
            UseShared = message.UseShared,
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_MOVEBANKTOINV))]
    private void ReceiveMoveBankToInv(WIZARD_12_PROTOCOL.MSG_MOVEBANKTOINV message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;
        var account = wizard.Account;

        bool isShared = message.UseShared != 0;
        WizClientObjectItem? item = isShared
            ? account?.SharedBankItems?.FirstOrDefault(i => i.m_globalID == message.GlobalID)
            : wizard.BankItems?.FirstOrDefault(i => i.m_globalID == message.GlobalID);

        if (item is null) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOINVCONFIRM {
                ClientRequestID = message.ClientRequestID,
                OriginalGlobalID = message.GlobalID,
                GlobalID = message.GlobalID,
                Failure = 1
            });
            return;
        }

        int maxInv = ConfigurationManager.Settings["Character.MaxInventoryItems"].AsInt(80);
        if ((wizard.InventoryBehavior?.Items?.Count ?? 0) >= maxInv) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOINVCONFIRM {
                ClientRequestID = message.ClientRequestID,
                OriginalGlobalID = message.GlobalID,
                GlobalID = message.GlobalID,
                Failure = 2
            });
            return;
        }

        if (isShared && account != null) {
            account.SharedBankItems?.Remove(item);
            account.SharedBankItemIds?.Remove(item.m_globalID);
            item.m_characterId = (GID) wizard.CharId;
            AccountCollection.UpdateSharedBank(account.AccountId, account.SharedBankItemIds);
            WizardItemCollection.UpdateItemCharacter(item.m_globalID, wizard.CharId);
        } else {
            wizard.BankItems?.Remove(item);
            wizard.BankItemIds?.Remove(item.m_globalID);
        }

        item.m_characterId = (GID) wizard.CharId;
        wizard.AddItemToInventory(item);
        WizardCollection.UpdateCharacterItems(wizard);

        if (s_serializer.Serialize(item, ITEM_SERIALIZE_FLAGS, out ByteString itemBytes)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
                GlobalID = wizard.GameObjectID,
                SerializedItem = itemBytes
            });
            SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
                ItemGlobalID = item.m_globalID,
                ItemLocation = 1,
                ItemTemplateID = (uint) item.m_templateID
            });
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOINVCONFIRM {
            ClientRequestID = message.ClientRequestID,
            OriginalGlobalID = message.GlobalID,
            GlobalID = message.GlobalID,
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_MOVEBANKTOBANK))]
    private void ReceiveMoveBankToBank(WIZARD_12_PROTOCOL.MSG_MOVEBANKTOBANK message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;
        var account = wizard.Account;

        if (message.BankID != 0 && account != null) {
            var item = wizard.BankItems?.FirstOrDefault(i => i.m_globalID == message.BankID);
            if (item != null) {
                wizard.BankItems?.Remove(item);
                wizard.BankItemIds?.Remove(item.m_globalID);
                account.SharedBankItems ??= [];
                account.SharedBankItemIds ??= [];
                account.SharedBankItems.Add(item);
                account.SharedBankItemIds.Add(item.m_globalID);
                item.m_characterId = (GID) 0;
                AccountCollection.UpdateSharedBank(account.AccountId, account.SharedBankItemIds);
                WizardItemCollection.UpdateItemCharacter(item.m_globalID, 0);
                WizardCollection.UpdateCharacterItems(wizard);

                SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOBANKCONFIRM {
                    ClientRequestID = message.ClientRequestID,
                    OriginalGlobalID = message.BankID,
                    GlobalID = message.BankID,
                    UseShared = 1,
                    Error = 0
                });
                return;
            }
        }

        if (message.BankSharedID != 0 && account != null) {
            var item = account.SharedBankItems?.FirstOrDefault(i => i.m_globalID == message.BankSharedID);
            if (item != null) {
                account.SharedBankItems.Remove(item);
                account.SharedBankItemIds?.Remove(item.m_globalID);
                item.m_characterId = (GID) wizard.CharId;
                wizard.BankItems ??= [];
                wizard.BankItemIds ??= [];
                wizard.BankItems.Add(item);
                wizard.BankItemIds.Add(item.m_globalID);
                AccountCollection.UpdateSharedBank(account.AccountId, account.SharedBankItemIds);
                WizardItemCollection.UpdateItemCharacter(item.m_globalID, wizard.CharId);
                WizardCollection.UpdateCharacterItems(wizard);

                SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOBANKCONFIRM {
                    ClientRequestID = message.ClientRequestID,
                    OriginalGlobalID = message.BankSharedID,
                    GlobalID = message.BankSharedID,
                    UseShared = 0,
                    Error = 0
                });
                return;
            }
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKTOBANKCONFIRM {
            ClientRequestID = message.ClientRequestID,
            OriginalGlobalID = message.BankID != 0 ? message.BankID : message.BankSharedID,
            GlobalID = message.BankID != 0 ? message.BankID : message.BankSharedID,
            UseShared = 0,
            Error = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_BANKDELETE))]
    private void ReceiveBankDelete(WIZARD_12_PROTOCOL.MSG_BANKDELETE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;
        var account = wizard.Account;

        bool isShared = message.UseShared != 0;
        if (isShared && account != null) {
            var item = account.SharedBankItems?.FirstOrDefault(i => i.m_globalID == message.GlobalID);
            if (item != null) {
                account.SharedBankItems.Remove(item);
                account.SharedBankItemIds?.Remove(item.m_globalID);
                AccountCollection.UpdateSharedBank(account.AccountId, account.SharedBankItemIds);
                WizardItemCollection.RemoveItem(0, item.m_globalID);
            }
        } else {
            var item = wizard.BankItems?.FirstOrDefault(i => i.m_globalID == message.GlobalID);
            if (item != null) {
                wizard.BankItems?.Remove(item);
                wizard.BankItemIds?.Remove(item.m_globalID);
                WizardCollection.UpdateCharacterItems(wizard);
                WizardItemCollection.RemoveItem(wizard.CharId, item.m_globalID);
            }
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_BANKDELETECONFIRM {
            ClientRequestID = message.ClientRequestID,
            GlobalID = message.GlobalID,
            UseShared = message.UseShared,
            Failure = 0
        });
    }

}
