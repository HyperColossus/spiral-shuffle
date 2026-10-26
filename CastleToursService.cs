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
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class CastleToursService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleToursService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUEST))]
    private void ReceiveCastleToursRequest(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITCASTLE))]
    private void ReceiveVisitCastle(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITCASTLE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        Teleport(
            destinationZone: "WizardCity/Interiors/WC_Housing_Dorm_Interior",
            destinationLocation: "Start",
            makePrivate: true
        );
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSSENDRATING))]
    private void ReceiveSendRating(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSSENDRATING message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }
}
