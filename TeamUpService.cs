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

internal sealed class TeamUpService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new TeamUpService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTTEAMUPINFO))]
    private void ReceiveRequestTeamUpInfo(WIZARD_12_PROTOCOL.MSG_REQUESTTEAMUPINFO message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_TEAMUPADDPLAYERTOCLIENT {
            SigilID = message.NPCID,
            PlayerGID = wizard.GameObjectID,
            PackedName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PlayerSchool = wizard.MagicSchoolBehavior.MagicSchool.ToString(),
            PlayerLevel = (uint) wizard.MagicSchoolBehavior.Level,
            SigilName = "Dungeon",
            UpdatePlayerGID = wizard.GameObjectID,
            SecondsLeft = 120,
            FromSigil = 1,
            RequestedSigilMode = message.RequestedSigilMode
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTTEAMUP))]
    private void ReceiveRequestTeamUp(WIZARD2_53_PROTOCOL.MSG_REQUESTTEAMUP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_TEAMUPADDPLAYERTOCLIENT {
            SigilID = message.NPCID,
            PlayerGID = wizard.GameObjectID,
            PackedName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PlayerSchool = wizard.MagicSchoolBehavior.MagicSchool.ToString(),
            PlayerLevel = (uint) wizard.MagicSchoolBehavior.Level,
            SigilName = "Dungeon",
            UpdatePlayerGID = wizard.GameObjectID,
            SecondsLeft = 120,
            FromSigil = 1,
            RequestedSigilMode = message.RequestedSigilMode
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_TEAMUPREMOVEPLAYER))]
    private void ReceiveTeamUpRemovePlayer(WIZARD2_53_PROTOCOL.MSG_TEAMUPREMOVEPLAYER message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_TEAMUPREMOVEPLAYERFROMCLIENT {
            SigilID = message.SigilID,
            PlayerGID = wizard.GameObjectID,
            UpdatePlayerGID = wizard.GameObjectID,
            Disbanded = 0,
            StopTimer = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATETEAMUPREADY))]
    private void ReceiveUpdateTeamUpReady(WIZARD3_56_PROTOCOL.MSG_UPDATETEAMUPREADY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATETEAMUPREADY {
            ReadyCount = 1,
            SigilID = message.SigilID,
            PlayerGID = wizard.GameObjectID
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_TEAMUPTELEPORTPLAYER {
            SigilID = message.SigilID,
            PlayerGID = wizard.GameObjectID,
            ZoneID = 0,
            PlayerPosition = 1,
            PlayerCount = 1,
            Level = (uint) wizard.MagicSchoolBehavior.Level
        });
    }
}
