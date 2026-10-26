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

internal sealed class BeastmoonService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BeastmoonService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDQUEUEPLAYER))]
    private void ReceiveQueuePlayer(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDQUEUEPLAYER message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDQUEUEUPDATE {
            ProjectID = message.ProjectID,
            PlayerGID = message.PlayerGID,
            Waiting = 1,
            Cancel = 0,
            Kicked = "",
            Ready = 1,
            TimeLeft = 15.0f,
            DisableCrossPlay = message.DisableCrossPlay,
            ClientPlatform = message.ClientPlatform
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDQUEUEUPDATE))]
    private void ReceiveQueueUpdate(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDQUEUEUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDREQUESTMATCHDATA))]
    private void ReceiveRequestMatchData(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDREQUESTMATCHDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDTELEPORTPLAYER))]
    private void ReceiveTeleportPlayer(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDTELEPORTPLAYER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDUPDATEPOINTS))]
    private void ReceiveUpdatePoints(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDUPDATEPOINTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDPOLYMORPHIMPROVEREQ))]
    private void ReceivePolymorphImproveReq(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDPOLYMORPHIMPROVEREQ message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDPOLYMORPHIMPROVERESP {
            Failure = 0,
            Rating = message.Rating + 1,
            Level = message.Level + 1,
            Tag = message.Tag
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDEND))]
    private void ReceiveBattlegroundEnd(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDEND message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDPENALTY))]
    private void ReceiveBattlegroundPenalty(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDPENALTY message) {
        SendToSocket(message);
    }
}
