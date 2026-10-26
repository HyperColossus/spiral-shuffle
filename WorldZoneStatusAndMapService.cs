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

internal sealed class WorldZoneStatusAndMapService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new WorldZoneStatusAndMapService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_LEMURIASTATUS))]
    private void ReceiveLemuriaStatus(WIZARD_12_PROTOCOL.MSG_LEMURIASTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_LAVALOCKSTATUS))]
    private void ReceiveLavaLockStatus(WIZARD2_53_PROTOCOL.MSG_LAVALOCKSTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_KHANDANCESTATUS))]
    private void ReceiveKhandanceStatus(WIZARD2_53_PROTOCOL.MSG_KHANDANCESTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_MAPREQUIREMENTS))]
    private void ReceiveMapRequirements(WIZARD2_53_PROTOCOL.MSG_MAPREQUIREMENTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_LEAVESTATE))]
    private void ReceiveLeaveState(GAME_5_PROTOCOL.MSG_LEAVESTATE message) {
        SendToSocket(message);
    }
}
