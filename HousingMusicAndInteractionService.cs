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

internal sealed class HousingMusicAndInteractionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingMusicAndInteractionService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICERROR))]
    private void ReceiveInteractiveMusicError(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICERROR message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICINUSE))]
    private void ReceiveInteractiveMusicInUse(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICINUSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICINUSE2))]
    private void ReceiveInteractiveMusicInUse2(WIZARDHOUSING_50_PROTOCOL.MSG_INTERACTIVEMUSICINUSE2 message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_INTERACTABLELOCKEDMESSAGE))]
    private void ReceiveInteractableLockedMessage(WIZARD3_56_PROTOCOL.MSG_INTERACTABLELOCKEDMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_INTERACTOBJECT))]
    private void ReceiveInteractObject(GAME_5_PROTOCOL.MSG_INTERACTOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_INTERACTCOMPLETEGOAL))]
    private void ReceiveInteractCompleteGoal(WIZARD_12_PROTOCOL.MSG_INTERACTCOMPLETEGOAL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_INTERACTUNDERWAYQUEST))]
    private void ReceiveInteractUnderwayQuest(WIZARD_12_PROTOCOL.MSG_INTERACTUNDERWAYQUEST message) {
        SendToSocket(message);
    }
}
