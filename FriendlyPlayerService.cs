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

internal sealed class FriendlyPlayerService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FriendlyPlayerService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTFRIENDLYPLAYERS))]
    private void ReceiveRequestFriendlyPlayers(WIZARD_12_PROTOCOL.MSG_REQUESTFRIENDLYPLAYERS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_SETVOLUNTEERFLAG))]
    private void ReceiveSetVolunteerFlag(WIZARD_12_PROTOCOL.MSG_SETVOLUNTEERFLAG message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTVOLUNTEERINFO))]
    private void ReceiveRequestVolunteerInfo(WIZARD_12_PROTOCOL.MSG_REQUESTVOLUNTEERINFO message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEVOLUNTEERINFO {
            CharacterID = message.CharacterID,
            Data = message.WorldInfo
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_VOLUNTEERREQUEST))]
    private void ReceiveVolunteerRequest(WIZARD_12_PROTOCOL.MSG_VOLUNTEERREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_VOLUNTEERREQUEST {
            PlayerGID = message.PlayerGID,
            TeamUpPlayerGID = message.TeamUpPlayerGID,
            SigilID = message.SigilID,
            SigilInfo = message.SigilInfo,
            Status = 1,
            ClientPlatform = message.ClientPlatform
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_VOLUNTEERTEAMHELPJOIN))]
    private void ReceiveVolunteerTeamHelpJoin(WIZARD_12_PROTOCOL.MSG_VOLUNTEERTEAMHELPJOIN message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_VOLUNTEERTEAMHELPCOMPLETE {
            PlayerGID = message.PlayerGID,
            Status = 1
        });
    }
}
