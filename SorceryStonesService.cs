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

internal sealed class SorceryStonesService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SorceryStonesService(parentActor));

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SELECTGAME))]
    private void ReceiveSelectGame(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SELECTGAME message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_READY());
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_STARTSWAP))]
    private void ReceiveStartSwap(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_STARTSWAP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_REQUESTROW))]
    private void ReceiveRequestRow(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_REQUESTROW message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ROWINFO {
            Index1 = 1,
            Index2 = 2,
            Index3 = 3,
            Index4 = 1,
            Index5 = 2,
            Index6 = 3,
            Index7 = 1
        });
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_PAUSE))]
    private void ReceivePause(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_PAUSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_RESETGAME))]
    private void ReceiveResetGame(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_RESETGAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ENDGAME))]
    private void ReceiveEndGame(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ENDGAME message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_WIN());
    }
}
