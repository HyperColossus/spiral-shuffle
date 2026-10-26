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
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class PvPKioskService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PvPKioskService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTPVPKIOSK))]
    private void ReceiveRequestPvPKiosk(WIZARD_12_PROTOCOL.MSG_REQUESTPVPKIOSK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPMATCHREQUEST))]
    private void ReceivePvPMatchRequest(WIZARD_12_PROTOCOL.MSG_PVPMATCHREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PVPMATCHREQUEST {
            TournamentNameID = message.TournamentNameID,
            MobileID = message.MobileID,
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPUPDATEREQUEST))]
    private void ReceivePvPUpdateRequest(WIZARD_12_PROTOCOL.MSG_PVPUPDATEREQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTSHOWPVPOPTION))]
    private void ReceiveRequestShowPvPOption(WIZARD3_56_PROTOCOL.MSG_REQUESTSHOWPVPOPTION message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATESHOWPVPOPTION {
            ShowPVPOption = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATESHOWPVPOPTION))]
    private void ReceiveUpdateShowPvPOption(WIZARD3_56_PROTOCOL.MSG_UPDATESHOWPVPOPTION message) {
        SendToSocket(message);
    }
}
