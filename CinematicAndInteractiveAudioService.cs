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

internal sealed class CinematicAndInteractiveAudioService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CinematicAndInteractiveAudioService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PLAYCINEMATIC))]
    private void ReceivePlayCinematic(GAME_5_PROTOCOL.MSG_PLAYCINEMATIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PLAYGRAPHIC))]
    private void ReceivePlayGraphic(WIZARD3_56_PROTOCOL.MSG_PLAYGRAPHIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PLAYMUSIC))]
    private void ReceivePlayMusic(WIZARD_12_PROTOCOL.MSG_PLAYMUSIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOPING))]
    private void ReceivePlayInteractiveMusicLooping(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOPING message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOPING2))]
    private void ReceivePlayInteractiveMusicLooping2(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOPING2 message) {
        SendToSocket(message);
    }
}
