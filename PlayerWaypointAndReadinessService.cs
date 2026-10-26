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

internal sealed class PlayerWaypointAndReadinessService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PlayerWaypointAndReadinessService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PLAYERARRIVED))]
    private void ReceivePlayerArrived(WIZARD_12_PROTOCOL.MSG_PLAYERARRIVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PLAYERREADYACK))]
    private void ReceivePlayerReadyAck(GAME_5_PROTOCOL.MSG_PLAYERREADYACK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PLAYERBLACKBALLEDALERT))]
    private void ReceivePlayerBlackballedAlert(GAME_5_PROTOCOL.MSG_PLAYERBLACKBALLEDALERT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PIIUPDATEEMOTESRADIALMENU))]
    private void ReceivePIIUpdateEmotesRadialMenu(WIZARD3_56_PROTOCOL.MSG_PIIUPDATEEMOTESRADIALMENU message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PROMOTEPROVISIONALPLAYER))]
    private void ReceivePromoteProvisionalPlayer(WIZARD3_56_PROTOCOL.MSG_PROMOTEPROVISIONALPLAYER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPCONFIRM))]
    private void ReceivePvPConfirm(WIZARD_12_PROTOCOL.MSG_PVPCONFIRM message) {
        SendToSocket(message);
    }
}
