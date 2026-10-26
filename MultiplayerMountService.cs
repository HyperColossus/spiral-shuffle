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

internal sealed class MultiplayerMountService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new MultiplayerMountService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTEQUIP))]
    private void ReceiveMountEquip(GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTEQUIP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTEQUIPUPDATE {
            DestinationCharacterID = wizard.GameObjectID,
            SourceCharacterID = message.SourceCharacterID,
            MountTemplateID = message.MountTemplateID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTJOIN))]
    private void ReceiveMountJoin(GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTJOIN message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new GAME_5_PROTOCOL.MSG_PARTYMULTIPLAYERMOUNTJOINUPDATE {
            DestinationCharacterID = wizard.GameObjectID,
            SourceCharacterID = message.SourceCharacterID,
            MountOwnerCharacterID = message.MountOwnerCharacterID,
            MountTemplateID = message.MountTemplateID
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_STARTRIDE))]
    private void ReceiveStartRide(WIZARD_12_PROTOCOL.MSG_STARTRIDE message) {
        SendToSocket(message);
    }
}
