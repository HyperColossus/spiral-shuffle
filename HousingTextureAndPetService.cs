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

internal sealed class HousingTextureAndPetService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingTextureAndPetService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PICKUPHOUSINGTEXTURE))]
    private void ReceivePickupHousingTexture(WIZARDHOUSING_50_PROTOCOL.MSG_PICKUPHOUSINGTEXTURE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SENDHOUSINGTEXTUREINFO))]
    private void ReceiveSendHousingTextureInfo(WIZARDHOUSING_50_PROTOCOL.MSG_SENDHOUSINGTEXTUREINFO message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGPET))]
    private void ReceiveUpdateHousingPet(WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEHOUSINGPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGGAMEREQUESTPOWERUPLOCATIONS))]
    private void ReceiveHousingGameRequestPowerUpLocations(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGGAMEREQUESTPOWERUPLOCATIONS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSINGGAMESTATE))]
    private void ReceiveSetHousingGameState(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSINGGAMESTATE message) {
        SendToSocket(message);
    }
}
