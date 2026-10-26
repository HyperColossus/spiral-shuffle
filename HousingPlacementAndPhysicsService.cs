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

internal sealed class HousingPlacementAndPhysicsService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingPlacementAndPhysicsService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PICKUPALL))]
    private void ReceivePickupAll(WIZARDHOUSING_50_PROTOCOL.MSG_PICKUPALL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PICKUPOBJECT))]
    private void ReceivePickupObject(GAME_5_PROTOCOL.MSG_PICKUPOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PLACEOBJECT))]
    private void ReceivePlaceObject(GAME_5_PROTOCOL.MSG_PLACEOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PHYSICS_16_PROTOCOL.MSG_PHYSICS_TORQUE))]
    private void ReceivePhysicsTorque(PHYSICS_16_PROTOCOL.MSG_PHYSICS_TORQUE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SYSTEM_1_PROTOCOL.MSG_PING_RSP))]
    private void ReceivePingRsp(SYSTEM_1_PROTOCOL.MSG_PING_RSP message) {
        SendToSocket(new SYSTEM_1_PROTOCOL.MSG_PING_RSP());
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYTRAINSOUND))]
    private void ReceivePlayTrainSound(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYTRAINSOUND message) {
        SendToSocket(message);
    }
}
