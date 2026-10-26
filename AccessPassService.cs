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

internal sealed class AccessPassService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AccessPassService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ACCESSPASSINFOREQUEST))]
    private void ReceiveAccessPassInfoRequest(WIZARD_12_PROTOCOL.MSG_ACCESSPASSINFOREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_ACCESSPASSOFFER {
            Data = message.AccessPass,
            Sample = "",
            TransactionID = 1,
            TotalCrowns = 500,
            WebFailure = 0,
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ACCESSPASSBUYREQUEST))]
    private void ReceiveAccessPassBuyRequest(WIZARD_12_PROTOCOL.MSG_ACCESSPASSBUYREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        const int PassCrownCost = 500;
        if (wizard.Account.Crowns >= PassCrownCost) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - PassCrownCost);
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_ACCESSPASSBUYCONFIRM {
            Failure = 0,
            WebFailure = 0,
            Credits = wizard.Account.Crowns
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ACCESSPASSPURCHASE))]
    private void ReceiveAccessPassPurchase(WIZARD2_53_PROTOCOL.MSG_ACCESSPASSPURCHASE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_GETTIMEDACCESSPASSES))]
    private void ReceiveGetTimedAccessPasses(WIZARD_12_PROTOCOL.MSG_GETTIMEDACCESSPASSES message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_TIMEDACCESSPASSES {
            Data = "ActiveAccessPasses"
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ACCESSPASSDECLINED))]
    private void ReceiveAccessPassDeclined(WIZARD_12_PROTOCOL.MSG_ACCESSPASSDECLINED message) {
        SendToSocket(message);
    }
}
