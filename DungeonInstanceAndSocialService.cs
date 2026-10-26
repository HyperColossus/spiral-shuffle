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

internal sealed class DungeonInstanceAndSocialService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DungeonInstanceAndSocialService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_ADDZONETIMER))]
    private void ReceiveAddZoneTimer(GAME_5_PROTOCOL.MSG_ADDZONETIMER message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_ADDZONETIMER {
            TimerID = message.TimerID,
            Title = message.Title,
            TimerUI = message.TimerUI,
            CountdownTime = message.CountdownTime
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REMOVEZONETIMER))]
    private void ReceiveRemoveZoneTimer(GAME_5_PROTOCOL.MSG_REMOVEZONETIMER message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_REMOVEZONETIMER {
            TimerID = message.TimerID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_UPDATEZONETIMER))]
    private void ReceiveUpdateZoneTimer(GAME_5_PROTOCOL.MSG_UPDATEZONETIMER message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_UPDATEZONETIMER {
            TimerID = message.TimerID,
            Title = message.Title,
            TimerUI = message.TimerUI,
            CountdownTime = message.CountdownTime
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_ZONEEVENTTIMEREXPIRED))]
    private void ReceiveZoneEventTimerExpired(GAME_5_PROTOCOL.MSG_ZONEEVENTTIMEREXPIRED message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_ZONEEVENTTIMEREXPIRED {
            TimerName = message.TimerName,
            EventName = message.EventName
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_UPDATEZONECOUNTER))]
    private void ReceiveUpdateZoneCounter(GAME_5_PROTOCOL.MSG_UPDATEZONECOUNTER message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_UPDATEZONECOUNTER {
            ZoneCounter = message.ZoneCounter
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_TELEPORT_TO_GAME_ZONE))]
    private void ReceiveTeleportToGameZone(GAME_5_PROTOCOL.MSG_TELEPORT_TO_GAME_ZONE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_TELEPORT_TO_GAME_ZONE {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            Game = message.Game,
            Track = message.Track,
            ZoneInstanceID = message.ZoneInstanceID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_INVITEFRIEND))]
    private void ReceiveInviteFriend(GAME_5_PROTOCOL.MSG_INVITEFRIEND message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_INVITEFRIEND {
            PromoID = message.PromoID,
            YourName = message.YourName,
            FriendName = message.FriendName,
            FriendEmail = message.FriendEmail,
            Locale = message.Locale,
            SendInEnglish = message.SendInEnglish
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_SETFRIENDLYPLAYER))]
    private void ReceiveSetFriendlyPlayer(WIZARD_12_PROTOCOL.MSG_SETFRIENDLYPLAYER message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_SETFRIENDLYPLAYER {
            FriendlyPlayer = message.FriendlyPlayer,
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYPLAYERWORLDS))]
    private void ReceiveUpdateFriendlyPlayerWorlds(WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYPLAYERWORLDS message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYPLAYERWORLDS {
            Data = message.Data
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYWORLD))]
    private void ReceiveUpdateFriendlyWorld(WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYWORLD message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEFRIENDLYWORLD {
            FriendlyWorld = message.FriendlyWorld
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUDDYREQUESTDENYFWD))]
    private void ReceiveBuddyRequestDenyFwd(GAME_5_PROTOCOL.MSG_BUDDYREQUESTDENYFWD message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_BUDDYREQUESTDENYFWD {
            ListOwnerGID = message.ListOwnerGID,
            EntryGID = message.EntryGID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CHECK_MAIL))]
    private void ReceiveCheckMail(GAME_5_PROTOCOL.MSG_CHECK_MAIL message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_CHECK_MAIL {
            CharacterID = wizard?.CharId ?? message.CharacterID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MAIL_DATA))]
    private void ReceiveMailData(GAME_5_PROTOCOL.MSG_MAIL_DATA message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_MAIL_DATA {
            Data = message.Data
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NEW_MAIL))]
    private void ReceiveNewMail(GAME_5_PROTOCOL.MSG_NEW_MAIL message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_NEW_MAIL {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            MailType = message.MailType,
            AccountMail = message.AccountMail
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_RETRIEVE_MAIL))]
    private void ReceiveRetrieveMail(GAME_5_PROTOCOL.MSG_RETRIEVE_MAIL message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_RETRIEVE_MAIL());
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REDEEM_MAIL_GIFT))]
    private void ReceiveRedeemMailGift(GAME_5_PROTOCOL.MSG_REDEEM_MAIL_GIFT message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_REDEEM_MAIL_GIFT_RESPONSE {
            MailId = message.MailId,
            ErrorCode = 0,
            TemplateId = 0
        });
    }
}
