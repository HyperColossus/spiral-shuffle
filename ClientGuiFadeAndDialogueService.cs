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

internal sealed class ClientGuiFadeAndDialogueService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ClientGuiFadeAndDialogueService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DISPLAYHELPGUI))]
    private void ReceiveDisplayHelpGUI(WIZARD2_53_PROTOCOL.MSG_DISPLAYHELPGUI message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DISPLAYMESSAGE))]
    private void ReceiveDisplayMessage(WIZARD2_53_PROTOCOL.MSG_DISPLAYMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_FADECONTROL))]
    private void ReceiveFadeControl(WIZARD3_56_PROTOCOL.MSG_FADECONTROL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_HIDEGUI))]
    private void ReceiveHideGUI(WIZARD3_56_PROTOCOL.MSG_HIDEGUI message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CORE_PIIRADIALMENUEMOTE))]
    private void ReceiveCorePIIRadialMenuEmote(WIZARD3_56_PROTOCOL.MSG_CORE_PIIRADIALMENUEMOTE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ENDORSEINFO))]
    private void ReceiveEndorseInfo(WIZARD3_56_PROTOCOL.MSG_ENDORSEINFO message) {
        SendToSocket(message);
    }
}
