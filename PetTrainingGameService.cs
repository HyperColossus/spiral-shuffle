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

internal sealed class PetTrainingGameService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetTrainingGameService(parentActor));

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEDROPOBJECT))]
    private void ReceivePetGameDropObject(PET_9_PROTOCOL.MSG_PETGAMEDROPOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEDROPBONUS))]
    private void ReceivePetGameDropBonus(PET_9_PROTOCOL.MSG_PETGAMEDROPBONUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETMORPHSET))]
    private void ReceivePetMorphSet(PET_9_PROTOCOL.MSG_PETMORPHSET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETSNACKADD))]
    private void ReceivePetSnackAdd(PET_9_PROTOCOL.MSG_PETSNACKADD message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new PET_9_PROTOCOL.MSG_PETSNACKADD {
            GlobalID = wizard.GameObjectID,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETSNACKREMOVE))]
    private void ReceivePetSnackRemove(PET_9_PROTOCOL.MSG_PETSNACKREMOVE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new PET_9_PROTOCOL.MSG_PETSNACKREMOVE {
            GlobalID = wizard.GameObjectID,
            ItemID = message.ItemID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETTOMEPETADDED))]
    private void ReceivePetTomePetAdded(WIZARD2_53_PROTOCOL.MSG_PETTOMEPETADDED message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PETTOMEPETADDED {
            GlobalID = wizard.GameObjectID,
            PetTemplateID = message.PetTemplateID
        });
    }
}
