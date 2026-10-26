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

internal sealed class PetPavilionMinigameEngineService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetPavilionMinigameEngineService(parentActor));

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEINIT))]
    private void ReceivePetGameInit(PET_9_PROTOCOL.MSG_PETGAMEINIT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEJOIN))]
    private void ReceivePetGameJoin(PET_9_PROTOCOL.MSG_PETGAMEJOIN message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMEJOINRSP {
            Game = message.Game,
            Success = 1
        });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEJOINRSP))]
    private void ReceivePetGameJoinRsp(PET_9_PROTOCOL.MSG_PETGAMEJOINRSP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEJUMP))]
    private void ReceivePetGameJump(PET_9_PROTOCOL.MSG_PETGAMEJUMP message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMEJUMP());
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEMAZE))]
    private void ReceivePetGameMaze(PET_9_PROTOCOL.MSG_PETGAMEMAZE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEREADY))]
    private void ReceivePetGameReady(PET_9_PROTOCOL.MSG_PETGAMEREADY message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMEREADY());
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMESTART))]
    private void ReceivePetGameStart(PET_9_PROTOCOL.MSG_PETGAMESTART message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEENDING))]
    private void ReceivePetGameEnding(PET_9_PROTOCOL.MSG_PETGAMEENDING message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEINDIVIDUALRESULTS))]
    private void ReceivePetGameIndividualResults(PET_9_PROTOCOL.MSG_PETGAMEINDIVIDUALRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEDATA))]
    private void ReceivePetGameData(PET_9_PROTOCOL.MSG_PETGAMEDATA message) {
        SendToSocket(message);
    }
}
