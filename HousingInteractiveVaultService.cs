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
using Akka.Actor;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;

namespace Imlight.CoreLib.Game.Services;

internal sealed class HousingInteractiveVaultService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingInteractiveVaultService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTPROXIMITY))]
    private void ReceiveHousingObjectProximity(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTPROXIMITY message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTPROXIMITY {
            ObjectID = message.ObjectID,
            Inside = message.Inside
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTNOPICKUP))]
    private void ReceiveHousingObjectNoPickup(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTNOPICKUP message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGOBJECTNOPICKUP());
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HIDEHOUSINGOBJECT))]
    private void ReceiveHideHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_HIDEHOUSINGOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_HIDEHOUSINGOBJECT {
            ObjectGID = message.ObjectGID,
            Hide = message.Hide
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_LEAVEHOUSINGLOT))]
    private void ReceiveLeaveHousingLot(GAME_5_PROTOCOL.MSG_LEAVEHOUSINGLOT message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_LEAVEHOUSINGLOT());
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGARDENINGSHED))]
    private void ReceivePatchHousingGardeningShed(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGARDENINGSHED message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGARDENINGSHED {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGEARVAULT))]
    private void ReceivePatchHousingGearVault(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGEARVAULT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGGEARVAULT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGJEWELVAULT))]
    private void ReceivePatchHousingJewelVault(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGJEWELVAULT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGJEWELVAULT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHEQUIPHOUSINGOBJECT))]
    private void ReceivePatchEquipHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHEQUIPHOUSINGOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHEQUIPHOUSINGOBJECT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }
}
