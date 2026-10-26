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

internal sealed class HatchmakingFriendPetQueryService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HatchmakingFriendPetQueryService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGFRIENDPETS))]
    private void ReceiveRequestHatchmakingFriendPets(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGFRIENDPETS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCHEDPET))]
    private void ReceiveRequestHatchmakingHatchedPet(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCHEDPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETDATA))]
    private void ReceiveRequestHatchmakingPetData(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETTYPES))]
    private void ReceiveRequestHatchmakingPetTypes(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETTYPES message) {
        SendToSocket(message);
    }
}
