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

internal sealed class GiftRedemptionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GiftRedemptionService(parentActor));

    [MessageHandler(typeof(LOGIN_7_PROTOCOL.MSG_FULFILLPROMOCODE))]
    private void ReceiveFulfillPromoCode(LOGIN_7_PROTOCOL.MSG_FULFILLPROMOCODE message) {
        if (GetActiveWizard() is { } wizard) {
            wizard.Account.SetCrowns(wizard.Account.Crowns + 500);
        }

        SendToSocket(new LOGIN_7_PROTOCOL.MSG_FULFILLPROMOCODE {
            Code = message.Code,
            Success = 1,
            ItemList = "PromoCrownsReward",
            ErrorDesc = ""
        });

        SendToSocket(new GAME_5_PROTOCOL.MSG_NOTIFY_GIFT {
            Gift = 1,
            CrownsRewards = 1,
            PeriodicItems = 0
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUEST_GIFTS))]
    private void ReceiveRequestGifts(GAME_5_PROTOCOL.MSG_REQUEST_GIFTS message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_RECEIVE_GIFTS {
            Success = 1,
            Data = "PromoGiftsAvailable",
            CrownsRewards = 1,
            PeriodicItems = 1,
            AutoRedeem = 0
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REDEEM_GIFT))]
    private void ReceiveRedeemGift(GAME_5_PROTOCOL.MSG_REDEEM_GIFT message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_GIFT_REDEEMED {
            Success = 1,
            GiftID = message.GiftID,
            GID = 1001,
            Data = "RedeemedPromoGift",
            TemplateID = 1001,
            ErrorCode = 0,
            Quantity = 1,
            HasMorePromoGifts = 0,
            HasMoreCrownsRewards = 0,
            HasMorePeriodicItems = 0,
            AutoRedeem = 0
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_DELETE_GIFT))]
    private void ReceiveDeleteGift(GAME_5_PROTOCOL.MSG_DELETE_GIFT message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_DELETE_GIFT {
            GiftID = message.GiftID,
            Success = 1,
            IsPeriodicItem = message.IsPeriodicItem
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NOTIFY_GIFT))]
    private void ReceiveNotifyGift(GAME_5_PROTOCOL.MSG_NOTIFY_GIFT message) {
        SendToSocket(message);
    }
}
