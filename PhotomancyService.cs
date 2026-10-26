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
using Imcodec.MessageLayer.Generated;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class PhotomancyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PhotomancyService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PHOTOMANCYUSETOME))]
    private void ReceivePhotomancyUseTome(WIZARD2_53_PROTOCOL.MSG_PHOTOMANCYUSETOME message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEPHOTOFILTERS {
            Filters = 0xFFFFFFFF
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PHOTOTAKEN))]
    private void ReceivePhotoTaken(WIZARD2_53_PROTOCOL.MSG_PHOTOTAKEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEPHOTOFILTERS))]
    private void ReceiveUpdatePhotoFilters(WIZARD3_56_PROTOCOL.MSG_UPDATEPHOTOFILTERS message) {
        SendToSocket(message);
    }
}
