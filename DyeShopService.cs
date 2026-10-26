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

internal sealed class DyeShopService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DyeShopService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_FULLDYEREQUEST))]
    private void ReceiveFullDyeRequest(WIZARD3_56_PROTOCOL.MSG_FULLDYEREQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_SETSKINTONEMESSAGE))]
    private void ReceiveSetSkinToneMessage(WIZARD3_56_PROTOCOL.MSG_SETSKINTONEMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHREMOVETEXTUREREMAP))]
    private void ReceivePatchRemoveTextureRemap(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHREMOVETEXTUREREMAP message) {
        SendToSocket(message);
    }
}
