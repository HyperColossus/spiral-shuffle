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

internal sealed class HenchmanService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HenchmanService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CROWNSHOPOFFER))]
    private void ReceiveCrownShopOffer(WIZARD3_56_PROTOCOL.MSG_CROWNSHOPOFFER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOGCROWNSHOPOFFER))]
    private void ReceiveLogCrownShopOffer(WIZARD3_56_PROTOCOL.MSG_LOGCROWNSHOPOFFER message) {
        if (GetActiveWizard() is not { } wizard) return;

        string action = message.Action.ToString();
        if (action.Equals("Buy", StringComparison.OrdinalIgnoreCase) || action.Equals("Hire", StringComparison.OrdinalIgnoreCase)) {
            const int HenchmanCrownCost = 100;
            if (wizard.Account.Crowns >= HenchmanCrownCost) {
                wizard.Account.SetCrowns(wizard.Account.Crowns - HenchmanCrownCost);
            }
        }

        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLAIMPERIODICOFFERS))]
    private void ReceiveClaimPeriodicOffers(WIZARD2_53_PROTOCOL.MSG_CLAIMPERIODICOFFERS message) {
        if (GetActiveWizard() is { } wizard) {
            wizard.Account.SetCrowns(wizard.Account.Crowns + 50);
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CLAIMPERIODICOFFERS {
            Success = 1,
            Crowns = 50,
            Items = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LOGOFFER))]
    private void ReceiveLogOffer(WIZARD_12_PROTOCOL.MSG_LOGOFFER message) {
        SendToSocket(message);
    }
}
