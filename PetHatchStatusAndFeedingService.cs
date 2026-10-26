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

internal sealed class PetHatchStatusAndFeedingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetHatchStatusAndFeedingService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PETGAMEKIOSK))]
    private void ReceivePetGameKiosk(WIZARD_12_PROTOCOL.MSG_PETGAMEKIOSK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETGAMESKIPFEED))]
    private void ReceivePetGameSkipFeed(WIZARD2_53_PROTOCOL.MSG_PETGAMESKIPFEED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMESNACKFEEDFAILED))]
    private void ReceivePetGameSnackFeedFailed(PET_9_PROTOCOL.MSG_PETGAMESNACKFEEDFAILED message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMESNACKFEEDFAILED());
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PETHATCHJOINSTATUS))]
    private void ReceivePetHatchJoinStatus(WIZARD_12_PROTOCOL.MSG_PETHATCHJOINSTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PETHATCHREADYSTATUS))]
    private void ReceivePetHatchReadyStatus(WIZARD_12_PROTOCOL.MSG_PETHATCHREADYSTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETMORPHCANAFFORD))]
    private void ReceivePetMorphCanAfford(PET_9_PROTOCOL.MSG_PETMORPHCANAFFORD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETMORPHREADY))]
    private void ReceivePetMorphReady(PET_9_PROTOCOL.MSG_PETMORPHREADY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PETPOWERWORLDFAILURE))]
    private void ReceivePetPowerWorldFailure(WIZARD3_56_PROTOCOL.MSG_PETPOWERWORLDFAILURE message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PETPOWERWORLDFAILURE());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PREVENTPLAYASPET))]
    private void ReceivePreventPlayAsPet(WIZARD3_56_PROTOCOL.MSG_PREVENTPLAYASPET message) {
        SendToSocket(message);
    }
}
