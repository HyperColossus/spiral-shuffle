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

internal sealed class ArcadeAndPhysicsService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ArcadeAndPhysicsService(parentActor));

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEON))]
    private void ReceiveSoblocksAdvanceOn(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEON message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEON());
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEOFF))]
    private void ReceiveSoblocksAdvanceOff(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEOFF message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ADVANCEOFF());
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ATTACK))]
    private void ReceiveSoblocksAttack(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ATTACK message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ATTACK {
            Index1 = message.Index1,
            Index2 = message.Index2,
            Index3 = message.Index3,
            Index4 = message.Index4,
            Index5 = message.Index5,
            Index6 = message.Index6,
            Index7 = message.Index7
        });
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_COUNTDOWN))]
    private void ReceiveSoblocksCountdown(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_COUNTDOWN message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_COUNTDOWN {
            Number = message.Number
        });
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_FREEZEBLOCKS))]
    private void ReceiveSoblocksFreezeBlocks(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_FREEZEBLOCKS message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_FREEZEBLOCKS {
            Amount = message.Amount
        });
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LEVELUP))]
    private void ReceiveSoblocksLevelUp(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LEVELUP message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LEVELUP());
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LOSS))]
    private void ReceiveSoblocksLoss(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LOSS message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_LOSS());
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SENDROW))]
    private void ReceiveSoblocksSendRow(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SENDROW message) {
        SendToSocket(new SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SENDROW {
            Index1 = message.Index1,
            Index2 = message.Index2,
            Index3 = message.Index3,
            Index4 = message.Index4,
            Index5 = message.Index5,
            Index6 = message.Index6,
            Index7 = message.Index7
        });
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_FORCE))]
    private void ReceivePhysicsForce(PHYSICS_16_PROTOCOL.MSG_PHYSICS_FORCE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_FORCE_AT_POS))]
    private void ReceivePhysicsForceAtPos(PHYSICS_16_PROTOCOL.MSG_PHYSICS_FORCE_AT_POS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_GRAB))]
    private void ReceivePhysicsGrab(PHYSICS_16_PROTOCOL.MSG_PHYSICS_GRAB message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_RELEASE))]
    private void ReceivePhysicsRelease(PHYSICS_16_PROTOCOL.MSG_PHYSICS_RELEASE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_STATE))]
    private void ReceivePhysicsState(PHYSICS_16_PROTOCOL.MSG_PHYSICS_STATE message) {
        SendToSocket(message);
    }
}
