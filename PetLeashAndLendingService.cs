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

internal sealed class PetLeashAndLendingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetLeashAndLendingService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEASH))]
    private void ReceiveLeash(WIZARD_12_PROTOCOL.MSG_LEASH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEASHOFFSET))]
    private void ReceiveLeashOffset(WIZARD_12_PROTOCOL.MSG_LEASHOFFSET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_LENTPET))]
    private void ReceiveLentPet(PET_9_PROTOCOL.MSG_LENTPET message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_LENTPET());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_LOCKLEVEL))]
    private void ReceiveLockLevel(WIZARD3_56_PROTOCOL.MSG_LOCKLEVEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_LOADSAVEOBJECTS))]
    private void ReceiveLoadSaveObjects(GAME_5_PROTOCOL.MSG_LOADSAVEOBJECTS message) {
        SendToSocket(message);
    }
}
