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

internal sealed class TournamentMatchReadyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new TournamentMatchReadyService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MATCHINVITE))]
    private void ReceiveMatchInvite(GAME_5_PROTOCOL.MSG_MATCHINVITE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MATCHMAKERUPDATE))]
    private void ReceiveMatchmakerUpdate(GAME_5_PROTOCOL.MSG_MATCHMAKERUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MATCHREADY))]
    private void ReceiveMatchReady(GAME_5_PROTOCOL.MSG_MATCHREADY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MATCHRESULT))]
    private void ReceiveMatchResult(GAME_5_PROTOCOL.MSG_MATCHRESULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NOT_AFK))]
    private void ReceiveNotAFK(GAME_5_PROTOCOL.MSG_NOT_AFK message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_NOT_AFK());
    }
}
