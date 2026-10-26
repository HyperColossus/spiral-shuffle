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

internal sealed class CharacterIdentityService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CharacterIdentityService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTRENAMECHARACTER))]
    private void ReceiveRequestRenameCharacter(WIZARD3_56_PROTOCOL.MSG_REQUESTRENAMECHARACTER message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTRENAMECHARACTER {
            Status = 0,
            NewName = message.NewName,
            NewGender = message.NewGender,
            Text = message.Text
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CSRREQUESTRENAMEINFO))]
    private void ReceiveCsrRequestRenameInfo(WIZARD3_56_PROTOCOL.MSG_CSRREQUESTRENAMEINFO message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CSRREQUESTRENAMEINFO {
            CharacterID = wizard.CharId,
            UsedRename = 1,
            NameBlob = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_UPDATEGENDER))]
    private void ReceiveUpdateGender(WIZARD_12_PROTOCOL.MSG_UPDATEGENDER message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGENDER {
            GlobalID = wizard.GameObjectID,
            NewGender = message.NewGender
        });
    }
}
