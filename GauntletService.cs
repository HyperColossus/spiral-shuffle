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

internal sealed class GauntletService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GauntletService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTCLASSPROJECTMONTHLYGAUNTLETINFO))]
    private void ReceiveRequestGauntletInfo(WIZARD3_56_PROTOCOL.MSG_REQUESTCLASSPROJECTMONTHLYGAUNTLETINFO message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTCLASSPROJECTMONTHLYGAUNTLETINFO {
            ProjectID = message.ProjectID,
            Tokens = 10,
            ClaimedDailyCredits = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_MONTHLYGAUNTLETCREDITSCLAIMED))]
    private void ReceiveGauntletCreditsClaimed(WIZARD3_56_PROTOCOL.MSG_MONTHLYGAUNTLETCREDITSCLAIMED message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_MONTHLYGAUNTLETCREDITSCLAIMED {
            ProjectID = message.ProjectID,
            Tokens = message.Tokens + 1
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSINGZONE))]
    private void ReceiveRequestHousingZone(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSINGZONE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSINGZONE {
            GlobalID = message.GlobalID,
            Status = 1,
            TemplateGID = message.TemplateGID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSINGZONETELEPORT))]
    private void ReceiveRequestHousingZoneTeleport(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSINGZONETELEPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGZONEPLAYER))]
    private void ReceiveHousingZonePlayer(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGZONEPLAYER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGZONEPLAYERLIST))]
    private void ReceiveHousingZonePlayerList(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGZONEPLAYERLIST message) {
        SendToSocket(message);
    }
}
