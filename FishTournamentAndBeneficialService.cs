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

internal sealed class FishTournamentAndBeneficialService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FishTournamentAndBeneficialService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ENDFISHINGCAST))]
    private void ReceiveEndFishingCast(WIZARDHOUSING_50_PROTOCOL.MSG_ENDFISHINGCAST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_ENTERBENEFICIALOBJECT))]
    private void ReceiveEnterBeneficialObject(CANTRIPSMESSAGES_57_PROTOCOL.MSG_ENTERBENEFICIALOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ENTERTOURNAMENTFISH))]
    private void ReceiveEnterTournamentFish(WIZARDHOUSING_50_PROTOCOL.MSG_ENTERTOURNAMENTFISH message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_ENTERTOURNAMENTFISHRESULT {
            Position = 1
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ENTERTOURNAMENTFISHRESULT))]
    private void ReceiveEnterTournamentFishResult(WIZARDHOUSING_50_PROTOCOL.MSG_ENTERTOURNAMENTFISHRESULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ENERGYBUYREQUEST))]
    private void ReceiveEnergyBuyRequest(WIZARD_12_PROTOCOL.MSG_ENERGYBUYREQUEST message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_BUYENERGYCONFIRM {
            Failure = 0,
            WebFailure = 0,
            Credits = 250
        });
    }
}
