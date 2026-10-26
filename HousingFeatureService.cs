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
using Imcodec.MessageLayer.Generated;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class HousingFeatureService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingFeatureService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSETELEPORTER))]
    private void ReceiveSetHouseTeleporter(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSETELEPORTER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGZONE))]
    private void ReceiveRequestTeleporterHousingZone(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGZONE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGZONE {
            GlobalID = message.GlobalID,
            Status = 1,
            TemplateGID = message.TemplateGID,
            StructureGID = message.StructureGID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_EQUIPHOUSEMUSIC))]
    private void ReceiveEquipHouseMusic(WIZARDHOUSING_50_PROTOCOL.MSG_EQUIPHOUSEMUSIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SELECTHOUSEMUSIC))]
    private void ReceiveSelectHouseMusic(WIZARDHOUSING_50_PROTOCOL.MSG_SELECTHOUSEMUSIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REMOVEHOUSEMUSIC))]
    private void ReceiveRemoveHouseMusic(WIZARDHOUSING_50_PROTOCOL.MSG_REMOVEHOUSEMUSIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ADDFISHTOAQUARIUM))]
    private void ReceiveAddFishToAquarium(WIZARDHOUSING_50_PROTOCOL.MSG_ADDFISHTOAQUARIUM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REMOVEFISHFROMAQUARIUM))]
    private void ReceiveRemoveFishFromAquarium(WIZARDHOUSING_50_PROTOCOL.MSG_REMOVEFISHFROMAQUARIUM message) {
        SendToSocket(message);
    }
}
