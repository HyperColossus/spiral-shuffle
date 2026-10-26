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

internal sealed class PetProgressionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetProgressionService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BUYPETELIXIRCONFIRM))]
    private void ReceiveBuyPetElixirConfirm(WIZARD2_53_PROTOCOL.MSG_BUYPETELIXIRCONFIRM message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        const int ElixirCost = 250;
        if (wizard.Account.Crowns >= ElixirCost) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - ElixirCost);
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BUYPETELIXIRCONFIRM {
            Failure = 0,
            WebFailure = 0,
            Credits = wizard.Account.Crowns
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_DONATEPET))]
    private void ReceiveDonatePet(WIZARD3_56_PROTOCOL.MSG_DONATEPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_BLOCKPETSPAWN))]
    private void ReceiveBlockPetSpawn(WIZARDHOUSING_50_PROTOCOL.MSG_BLOCKPETSPAWN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DISPLAYPETONLYMESSAGE))]
    private void ReceiveDisplayPetOnlyMessage(WIZARD2_53_PROTOCOL.MSG_DISPLAYPETONLYMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYPETENERGYINFO))]
    private void ReceiveDerbyPetEnergyInfo(PET_9_PROTOCOL.MSG_DERBYPETENERGYINFO message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_DERBYPETENERGYINFO {
            Cost = message.Cost,
            Current = 100,
            Maximum = 100,
            GID = message.GID
        });
    }
}
