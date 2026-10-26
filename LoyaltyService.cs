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

internal sealed class LoyaltyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new LoyaltyService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORELISTREQUEST))]
    private void ReceiveLoyaltyStoreListRequest(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORELISTREQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_SHOPLOYALTYLIST());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOYALTYCLAIMLOYALTYTOKENS))]
    private void ReceiveLoyaltyClaimTokens(WIZARD3_56_PROTOCOL.MSG_LOYALTYCLAIMLOYALTYTOKENS message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_LOYALTYCLAIMLOYALTYTOKENSCONFIRM());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_LOCK_REQUEST))]
    private void ReceiveLoyaltyPurchaseLock(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_LOCK_REQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_LOCK_RESPONSE {
            Item = message.Item,
            LoyaltyTokensCost = 1,
            Error = 0
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_REQUEST))]
    private void ReceiveLoyaltyPurchase(WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_REQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_LOYALTYSTORE_PURCHASE_RESPONSE {
            Item = message.Item,
            Type = 1,
            LoyaltyTokensCost = message.LoyaltyTokensCost,
            Error = 0
        });
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_REQUESTEXPLICITLOYALTYSTOREUPDATE))]
    private void ReceiveRequestExplicitLoyaltyStoreUpdate(GAME2_55_PROTOCOL.MSG_REQUESTEXPLICITLOYALTYSTOREUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTCROWNSREWARDSEVENTS))]
    private void ReceiveRequestCrownsRewardsEvents(WIZARD2_53_PROTOCOL.MSG_REQUESTCROWNSREWARDSEVENTS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ADDCROWNSREWARDSEVENT());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_WHIRLYBURLYOPENKIOSK))]
    private void ReceiveWhirlyburlyOpenKiosk(WIZARD2_53_PROTOCOL.MSG_WHIRLYBURLYOPENKIOSK message) {
        SendToSocket(message);
    }
}
