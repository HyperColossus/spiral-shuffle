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
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class FairgroundsArcadeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FairgroundsArcadeService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ENTERMINIGAME))]
    private void ReceiveEnterMinigame(WIZARD_12_PROTOCOL.MSG_ENTERMINIGAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEAVEMINIGAME))]
    private void ReceiveLeaveMinigame(WIZARD_12_PROTOCOL.MSG_LEAVEMINIGAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTMINIGAME))]
    private void ReceiveRequestMinigame(WIZARD2_53_PROTOCOL.MSG_REQUESTMINIGAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SETLEVEL))]
    private void ReceiveSoBlocksSetLevel(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_SETLEVEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_TIMEDDROP))]
    private void ReceiveSoBlocksTimedDrop(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_TIMEDDROP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ROCKDROP))]
    private void ReceiveSoBlocksRockDrop(SOBLOCKS_MESSAGES_25_PROTOCOL.MSG_SOBLOCKS_ROCKDROP message) {
        SendToSocket(message);
    }
}
