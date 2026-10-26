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

internal sealed class PetDerbyRaceTrackService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetDerbyRaceTrackService(parentActor));

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYEFFECTAPPLY))]
    private void ReceiveDerbyEffectApply(PET_9_PROTOCOL.MSG_DERBYEFFECTAPPLY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYEFFECTSLISTUPDATE))]
    private void ReceiveDerbyEffectsListUpdate(PET_9_PROTOCOL.MSG_DERBYEFFECTSLISTUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYLOCATION))]
    private void ReceiveDerbyLocation(PET_9_PROTOCOL.MSG_DERBYLOCATION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYSTATMOD))]
    private void ReceiveDerbyStatMod(PET_9_PROTOCOL.MSG_DERBYSTATMOD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYSYNC))]
    private void ReceiveDerbySync(PET_9_PROTOCOL.MSG_DERBYSYNC message) {
        SendToSocket(message);
    }
}
