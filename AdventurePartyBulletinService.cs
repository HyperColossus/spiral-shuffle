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

internal sealed class AdventurePartyBulletinService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdventurePartyBulletinService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CANACQUIREGROUPELIXIR))]
    private void ReceiveCanAcquireGroupElixir(WIZARD3_56_PROTOCOL.MSG_CANACQUIREGROUPELIXIR message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CANACQUIREGROUPELIXIR {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            CanAcquireGroupElixir = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_SHOWCLIENTGROUPELIXIRPOPUP))]
    private void ReceiveShowClientGroupElixirPopup(WIZARD3_56_PROTOCOL.MSG_SHOWCLIENTGROUPELIXIRPOPUP message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_SHOWCLIENTGROUPELIXIRPOPUP {
            SenderPlayerGID = message.SenderPlayerGID,
            GroupElxirDisplayKey = message.GroupElxirDisplayKey
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_POSTPARTYMESSAGE))]
    private void ReceivePostPartyMessage(WIZARD3_56_PROTOCOL.MSG_POSTPARTYMESSAGE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_POSTPARTYMESSAGE {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PartyGID = message.PartyGID,
            ActivityType = message.ActivityType,
            WorldID = message.WorldID,
            TimeOffset = message.TimeOffset,
            ClientTime = message.ClientTime,
            HourOffset = message.HourOffset
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REMOVEPARTYMESSAGE))]
    private void ReceiveRemovePartyMessage(WIZARD3_56_PROTOCOL.MSG_REMOVEPARTYMESSAGE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REMOVEPARTYMESSAGE {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PartyGID = message.PartyGID,
            CreationTime = message.CreationTime,
            RemovalTime = message.RemovalTime
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CHECKPARTYNAME))]
    private void ReceiveCheckPartyName(WIZARD3_56_PROTOCOL.MSG_CHECKPARTYNAME message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CHECKPARTYNAME {
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale,
            PartyGID = message.PartyGID,
            RequestingPlayerGID = wizard?.GameObjectID ?? message.RequestingPlayerGID,
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            Buffer = message.Buffer
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CANCELEDPARTYMESSAGE))]
    private void ReceiveCanceledPartyMessage(WIZARD3_56_PROTOCOL.MSG_CANCELEDPARTYMESSAGE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CANCELEDPARTYMESSAGE {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            CentralTime = message.CentralTime,
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale,
            Guild = message.Guild
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CHANGERESPONSEPARTYMESSAGE))]
    private void ReceiveChangeResponsePartyMessage(WIZARD3_56_PROTOCOL.MSG_CHANGERESPONSEPARTYMESSAGE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CHANGERESPONSEPARTYMESSAGE {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            MessageGID = message.MessageGID,
            PartyGID = message.PartyGID,
            CreationTime = message.CreationTime,
            RemovalTime = message.RemovalTime
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYSUBMITMEMBERZONES))]
    private void ReceivePartySubmitMemberZones(GAME_5_PROTOCOL.MSG_PARTYSUBMITMEMBERZONES message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_PARTYSUBMITMEMBERZONES {
            PartyMemberZones = message.PartyMemberZones,
            DestinationCharacterID = message.DestinationCharacterID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYZONEREQUESTRESPONSE))]
    private void ReceivePartyZoneRequestResponse(GAME_5_PROTOCOL.MSG_PARTYZONEREQUESTRESPONSE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_PARTYZONEREQUESTRESPONSE {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            PartyID = message.PartyID,
            PartyTotalSize = message.PartyTotalSize,
            FromAdventureParty = message.FromAdventureParty
        });
    }
}
