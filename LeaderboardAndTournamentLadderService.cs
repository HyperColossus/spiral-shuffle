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

internal sealed class LeaderboardAndTournamentLadderService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new LeaderboardAndTournamentLadderService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEADERBOARDFRIENDREQUEST))]
    private void ReceiveLeaderboardFriendRequest(WIZARD_12_PROTOCOL.MSG_LEADERBOARDFRIENDREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_LEADERBOARDRESPONSE {
            Contents = message.Contents,
            TotalEntries = 1,
            CurrentPage = (uint)Math.Max(0, (int)message.Page)
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEADERBOARDREQUEST))]
    private void ReceiveLeaderboardRequest(WIZARD_12_PROTOCOL.MSG_LEADERBOARDREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_LEADERBOARDRESPONSE {
            Contents = ByteString.Empty,
            TotalEntries = 1,
            CurrentPage = (uint)Math.Max(0, (int)message.Page)
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEADERBOARDRESPONSE))]
    private void ReceiveLeaderboardResponse(WIZARD_12_PROTOCOL.MSG_LEADERBOARDRESPONSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_LADDER))]
    private void ReceiveLadder(GAME_5_PROTOCOL.MSG_LADDER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MATCHAWARD))]
    private void ReceiveMatchAward(GAME_5_PROTOCOL.MSG_MATCHAWARD message) {
        SendToSocket(message);
    }
}
