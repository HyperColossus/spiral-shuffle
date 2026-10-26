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

internal sealed class InteractiveMusicService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new InteractiveMusicService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTINTERACTIVEMUSIC))]
    private void ReceiveRequestInteractiveMusic(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTINTERACTIVEMUSIC message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTINTERACTIVEMUSIC {
            ObjectGID = message.ObjectGID,
            InUse = 0,
            PlayerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString()
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSIC))]
    private void ReceivePlayInteractiveMusic(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSIC2))]
    private void ReceivePlayInteractiveMusic2(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSIC2 message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOP))]
    private void ReceivePlayInteractiveMusicLoop(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PLAYINTERACTIVEMUSICLOOP {
            PlayerGID = wizard.GameObjectID,
            TemplateID = message.TemplateID,
            LoopIndex = message.LoopIndex,
            ObjectGID = message.ObjectGID,
            TargetPlayers = ByteString.Empty,
            LoopOffset = message.LoopOffset
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_STOPINTERACTIVEMUSICLOOP))]
    private void ReceiveStopInteractiveMusicLoop(WIZARDHOUSING_50_PROTOCOL.MSG_STOPINTERACTIVEMUSICLOOP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_STOPINTERACTIVEMUSICLOOP {
            PlayerGID = wizard.GameObjectID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETMUSICLOOP))]
    private void ReceiveSetMusicLoop(WIZARDHOUSING_50_PROTOCOL.MSG_SETMUSICLOOP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEMUSICLOOP))]
    private void ReceiveDeleteMusicLoop(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEMUSICLOOP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SELECTMUSICSTYLE))]
    private void ReceiveSelectMusicStyle(WIZARDHOUSING_50_PROTOCOL.MSG_SELECTMUSICSTYLE message) {
        SendToSocket(message);
    }
}
