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

internal sealed class PetHatchmakingStatusService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetHatchmakingStatusService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_GAINPETXP))]
    private void ReceiveGainPetXP(WIZARD2_53_PROTOCOL.MSG_GAINPETXP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_GETSNACKLIST))]
    private void ReceiveGetSnackList(WIZARD_12_PROTOCOL.MSG_GETSNACKLIST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGMESSAGE))]
    private void ReceiveHatchmakingMessage(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSCSRRESULTS))]
    private void ReceiveHatchmakingsCSRResults(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSCSRRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSEARCHRESULTS))]
    private void ReceiveHatchmakingSearchResults(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSEARCHRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSTATUS))]
    private void ReceiveHatchmakingStatus(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSTATUS message) {
        SendToSocket(message);
    }
}
