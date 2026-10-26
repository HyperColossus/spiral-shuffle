/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.CoreObject;
using Imcodec.Math;
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

internal sealed class HousingService(SessionActor sessionActor) : MessageService(sessionActor) {

    private readonly CoreObjectSerializer _itemSerializer = new(behaviors: SerializerFlags.None);

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLACEHOUSINGOBJECT))]
    private void ReceivePlaceHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_PLACEHOUSINGOBJECT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (!wizard.InventoryBehavior.HasItem(message.ObjectID)) {
            Logger.Debug("HousingService: Player does not possess item {0} to place.", Logger.Args(message.ObjectID));
            return;
        }

        var item = wizard.InventoryBehavior.Items?.FirstOrDefault(i => (ulong) i.m_globalID == message.ObjectID);
        if (item is null) return;

        wizard.RemoveItemFromInventory(message.ObjectID);
        SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM {
            GlobalID = wizard.GameObjectID,
            ItemID = message.ObjectID
        });

        item.m_location = new Vector3(message.LocX, message.LocY, message.LocZ);
        item.m_orientation = new Vector3(0, 0, message.Yaw);
        WizardItemCollection.UpdateItem(item);

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGOBJECT {
            ObjectGID = message.ObjectID,
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ,
            Yaw = message.Yaw,
            Scale = 1.0f
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHADDHOUSINGOBJECT {
            ObjectID = message.ObjectID,
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ,
            Yaw = message.Yaw
        });

        Logger.Information("Placed housing object {0} at ({1}, {2}, {3}) yaw {4}",
            Logger.Args(message.ObjectID, message.LocX, message.LocY, message.LocZ, message.Yaw));
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGOBJECT))]
    private void ReceiveUpdateHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGOBJECT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGOBJECT {
            ObjectGID = message.ObjectGID,
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ,
            Yaw = message.Yaw,
            Scale = message.Scale > 0 ? message.Scale : 1.0f
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHDELETEHOUSINGOBJECT))]
    private void ReceiveDeleteHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHDELETEHOUSINGOBJECT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var item = new WizClientObjectItem {
            m_globalID = message.GIDID,
            m_templateID = 10183
        };
        wizard.AddItemToInventory(item);
        if (_itemSerializer.Serialize(item, 1, out var itemData)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
                GlobalID = wizard.GameObjectID,
                SerializedItem = itemData
            });
        }

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHDELETEHOUSINGOBJECT {
            GIDID = message.GIDID,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTATTIC))]
    private void ReceiveRequestAttic(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTATTIC message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEATTICCOUNT {
            GlobalID = wizard.GameObjectID,
            ItemCount = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETOATTIC))]
    private void ReceiveMoveToAttic(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETOATTIC message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (wizard.InventoryBehavior.HasItem(message.ItemID)) {
            wizard.RemoveItemFromInventory(message.ItemID);
            SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM {
                GlobalID = wizard.GameObjectID,
                ItemID = message.ItemID
            });
            SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEATTICCOUNT {
                GlobalID = wizard.GameObjectID,
                ItemCount = 1
            });
        }
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEFROMATTIC))]
    private void ReceiveMoveFromAttic(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEFROMATTIC message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var item = new WizClientObjectItem {
            m_globalID = message.ItemID,
            m_templateID = 10183
        };
        wizard.AddItemToInventory(item);
        if (_itemSerializer.Serialize(item, 1, out var itemData)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
                GlobalID = wizard.GameObjectID,
                SerializedItem = itemData
            });
        }
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEATTICCOUNT {
            GlobalID = wizard.GameObjectID,
            ItemCount = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCHANGEACTION))]
    private void ReceiveCastleMagicChangeAction(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCHANGEACTION message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCHANGEACTION {
            GlobalID = message.GlobalID,
            ActionNumber = message.ActionNumber,
            SpellTemplateID = message.SpellTemplateID,
            TargetTemplateID = message.TargetTemplateID,
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTUSE))]
    private void ReceiveCastleMagicRequestUse(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTUSE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICUPDATESTATE {
            GlobalID = message.ItemGID,
            State = 1,
            RandomNumbers = (uint) Random.Shared.Next(1, 100)
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICUPDATESTATE))]
    private void ReceiveCastleMagicUpdateState(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICUPDATESTATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICORB))]
    private void ReceiveCastleMagicOrb(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICORB message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICORB {
            OrbFlags = message.OrbFlags
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCURRENTSTATE))]
    private void ReceiveCastleMagicCurrentState(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCURRENTSTATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWMOUNTS))]
    private void ReceiveCastleMagicAllowMounts(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWMOUNTS message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWMOUNTS {
            State = message.State
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWPVP))]
    private void ReceiveCastleMagicAllowPvP(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWPVP message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTALLOWPVP {
            State = message.State
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTPLAYERTELEPORT))]
    private void ReceiveCastleMagicPlayerTeleport(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTPLAYERTELEPORT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new GAME_5_PROTOCOL.MSG_SERVERTELEPORT {
            LocationX = (ushort) (message.LocX / 4.0f),
            LocationY = (ushort) (message.LocY / 4.0f),
            LocationZ = (ushort) (message.LocZ / 4.0f),
            Direction = (byte) (message.Yaw / Math.PI / 2 * 250),
            MobileID = (ushort) (message.TargetPlayerGID > 0 ? (message.TargetPlayerGID & 0xFFFF) : (wizard.GameObjectID & 0xFFFF))
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICTUTORIAL))]
    private void ReceiveCastleMagicTutorial(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICTUTORIAL message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICTUTORIAL {
            State = message.State
        });
    }
}
