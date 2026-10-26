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

internal sealed class RealmAndBadgeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new RealmAndBadgeService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_TRANSFER_REALMS))]
    private void ReceiveTransferRealms(GAME_5_PROTOCOL.MSG_TRANSFER_REALMS message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_TRANSFER_REALMS {
            RealmName = message.RealmName,
            Error = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATEREMEMBERLASTREALM))]
    private void ReceiveUpdateRememberLastRealm(WIZARD2_53_PROTOCOL.MSG_UPDATEREMEMBERLASTREALM message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEREMEMBERLASTREALM {
            RememberLastRealm = message.RememberLastRealm
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_SETTESTREALMWATERMARK))]
    private void ReceiveSetTestRealmWatermark(WIZARD3_56_PROTOCOL.MSG_SETTESTREALMWATERMARK message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_SETTESTREALMWATERMARK {
            Enable = message.Enable
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDFRIENDS))]
    private void ReceiveRequestBadgeLeaderboardFriends(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDFRIENDS message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDFRIENDS {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            Data = message.Data,
            IsExtraCredit = message.IsExtraCredit
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDPAGE))]
    private void ReceiveRequestBadgeLeaderboardPage(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDPAGE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELEADERBOARDPAGE {
            Page = message.Page,
            IsExtraCredit = message.IsExtraCredit,
            IsFriends = message.IsFriends,
            Data = message.Data,
            CharacterID = wizard?.CharId ?? message.CharacterID
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELOOT))]
    private void ReceiveRequestBadgeLoot(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELOOT message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTBADGELOOT {
            BadgeID = message.BadgeID,
            Data = message.Data,
            Data2 = message.Data2,
            OpenDetails = message.OpenDetails
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGEREWARD))]
    private void ReceiveRequestBadgeReward(WIZARD3_56_PROTOCOL.MSG_REQUESTBADGEREWARD message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTBADGEREWARD {
            BadgeID = message.BadgeID,
            Overcount = message.Overcount
        });
    }
}
