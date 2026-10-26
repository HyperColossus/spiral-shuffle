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

internal sealed class SocialModerationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SocialModerationService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_IGNOREADD))]
    private void ReceiveIgnoreAdd(GAME_5_PROTOCOL.MSG_IGNOREADD message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_IGNORELIST {
            ListOwnerGID = message.ListOwnerGID,
            ListData = message.CharacterName,
            Add = 1
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_IGNOREDROP))]
    private void ReceiveIgnoreDrop(GAME_5_PROTOCOL.MSG_IGNOREDROP message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_IGNORELIST {
            ListOwnerGID = message.ListOwnerGID,
            ListData = "",
            Add = 0
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUGREPORT))]
    private void ReceiveBugReport(GAME_5_PROTOCOL.MSG_BUGREPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_HARASSMENTREPORT))]
    private void ReceiveHarassmentReport(GAME_5_PROTOCOL.MSG_HARASSMENTREPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REPORTHOUSE))]
    private void ReceiveReportHouse(WIZARDHOUSING_50_PROTOCOL.MSG_REPORTHOUSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTPRIVACYOPTIONS))]
    private void ReceiveRequestPrivacyOptions(WIZARD_12_PROTOCOL.MSG_REQUESTPRIVACYOPTIONS message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_RESPONSEPRIVACYOPTIONS {
            AllowFriendRequest = 1,
            AllowFriendTeleport = 1,
            AllowTradeRequest = 1,
            AllowHatchRequest = 1,
            AllowPartyInvites = 1,
            HidePVPEnemyChat = 0,
            LimitHomeToFriends = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_UPDATEPRIVACYOPTIONS))]
    private void ReceiveUpdatePrivacyOptions(WIZARD_12_PROTOCOL.MSG_UPDATEPRIVACYOPTIONS message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_RESPONSEPRIVACYOPTIONS {
            AllowFriendRequest = message.AllowFriendRequest,
            AllowFriendTeleport = message.AllowFriendTeleport,
            AllowTradeRequest = message.AllowTradeRequest,
            AllowHatchRequest = message.AllowHatchRequest,
            AllowPartyInvites = message.AllowPartyInvites,
            HidePVPEnemyChat = message.HidePVPEnemyChat,
            LimitHomeToFriends = message.LimitHomeToFriends
        });
    }
}
