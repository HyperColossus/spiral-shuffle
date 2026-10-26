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

internal sealed class InstanceFishingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new InstanceFishingService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETINSTANCEFISH))]
    private void ReceiveSetInstanceFish(WIZARDHOUSING_50_PROTOCOL.MSG_SETINSTANCEFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ADDINSTANCEFISH))]
    private void ReceiveAddInstanceFish(WIZARDHOUSING_50_PROTOCOL.MSG_ADDINSTANCEFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETFISHINGPLAYERS))]
    private void ReceiveSetFishingPlayers(WIZARDHOUSING_50_PROTOCOL.MSG_SETFISHINGPLAYERS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEFISH))]
    private void ReceiveDeleteFish(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEFISH message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DELETEFISH {
            GlobalID = wizard.GameObjectID,
            FishTemplateID = message.FishTemplateID,
            Size = message.Size
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CSRREQUESTFISHLIST))]
    private void ReceiveCsrRequestFishList(WIZARD2_53_PROTOCOL.MSG_CSRREQUESTFISHLIST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CSRREQUESTFISHLIST {
            CharacterGID = wizard.GameObjectID,
            FishData = ByteString.Empty
        });
    }
}
