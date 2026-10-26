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

internal sealed class PlatformAndPromotionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PlatformAndPromotionService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATECLIENTPLATFORM))]
    private void ReceiveUpdateClientPlatform(WIZARD3_56_PROTOCOL.MSG_UPDATECLIENTPLATFORM message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATECLIENTPLATFORM {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            ClientPlatform = message.ClientPlatform
        });
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_PLATFORMSYNCPURCHASES))]
    private void ReceivePlatformSyncPurchases(GAME2_55_PROTOCOL.MSG_PLATFORMSYNCPURCHASES message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_PlatformSyncPurchasesErr))]
    private void ReceivePlatformSyncPurchasesErr(GAME2_55_PROTOCOL.MSG_PlatformSyncPurchasesErr message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_SetPlatformFriends))]
    private void ReceiveSetPlatformFriends(GAME2_55_PROTOCOL.MSG_SetPlatformFriends message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME2_55_PROTOCOL.MSG_SetPlatformFriends {
            ListOwnerGID = wizard?.GameObjectID ?? message.ListOwnerGID,
            FullList = message.FullList,
            Added = message.Added,
            Removed = message.Removed
        });
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_SetPlatformIgnore))]
    private void ReceiveSetPlatformIgnore(GAME2_55_PROTOCOL.MSG_SetPlatformIgnore message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME2_55_PROTOCOL.MSG_SetPlatformIgnore {
            ListOwnerGID = wizard?.GameObjectID ?? message.ListOwnerGID,
            FullList = message.FullList,
            Added = message.Added,
            Removed = message.Removed
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUEST_PROMOTIONS))]
    private void ReceiveRequestPromotions(GAME_5_PROTOCOL.MSG_REQUEST_PROMOTIONS message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_RECEIVE_PROMOTIONS {
            Success = 1,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(LOGIN_7_PROTOCOL.MSG_WEBCHARACTERINFO))]
    private void ReceiveWebCharacterInfo(LOGIN_7_PROTOCOL.MSG_WEBCHARACTERINFO message) {
        var wizard = GetActiveWizard();
        SendToSocket(new LOGIN_7_PROTOCOL.MSG_WEBCHARACTERINFO {
            Name = message.Name,
            Gender = wizard is not null ? (int)wizard.PlayerNameBehavior.Gender : message.Gender,
            School = wizard is not null ? (int)wizard.MagicSchoolBehavior.MagicSchool : message.School
        });
    }

    [MessageHandler(typeof(LOGIN_7_PROTOCOL.MSG_WEB_AUTHEN))]
    private void ReceiveWebAuthen(LOGIN_7_PROTOCOL.MSG_WEB_AUTHEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(LOGIN_7_PROTOCOL.MSG_WEB_VALIDATE))]
    private void ReceiveWebValidate(LOGIN_7_PROTOCOL.MSG_WEB_VALIDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSREDITCHARACTER))]
    private void ReceiveCSREditCharacter(GAME_5_PROTOCOL.MSG_CSREDITCHARACTER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSREDITCHARACTERFAILED))]
    private void ReceiveCSREditCharacterFailed(GAME_5_PROTOCOL.MSG_CSREDITCHARACTERFAILED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSRSEARCHLIST))]
    private void ReceiveCSRSearchList(GAME_5_PROTOCOL.MSG_CSRSEARCHLIST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSRFINISHEDHOUSE))]
    private void ReceiveCSRFinishedHouse(GAME_5_PROTOCOL.MSG_CSRFINISHEDHOUSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MUTE))]
    private void ReceiveMute(GAME_5_PROTOCOL.MSG_MUTE message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_MUTE_RSP {
            MutedID = ByteString.Empty,
            MuteTime = message.MuteTime,
            MuteType = ByteString.Empty,
            Success = 1
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NOTMUTED))]
    private void ReceiveNotMuted(GAME_5_PROTOCOL.MSG_NOTMUTED message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_NOTMUTED());
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BAN_RSP))]
    private void ReceiveBanRsp(GAME_5_PROTOCOL.MSG_BAN_RSP message) {
        SendToSocket(message);
    }
}
