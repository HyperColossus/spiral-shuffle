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

internal sealed class CraftingAndRecyclingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CraftingAndRecyclingService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTADD))]
    private void ReceiveCraftingSlotAdd(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTADD message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTADD {
            GlobalID = wizard.GameObjectID,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTREMOVE))]
    private void ReceiveCraftingSlotRemove(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTREMOVE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTREMOVE {
            GlobalID = wizard.GameObjectID,
            ItemID = message.ItemID
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTCOUNT))]
    private void ReceiveCraftingSlotCount(WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTCOUNT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CRAFTINGSLOTCOUNT {
            GlobalID = wizard.GameObjectID,
            BonusSlots = message.BonusSlots
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTRECYCLINGBIN))]
    private void ReceiveRequestRecyclingBin(WIZARD3_56_PROTOCOL.MSG_REQUESTRECYCLINGBIN message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTRECYCLINGBIN {
            SerializedItem = ByteString.Empty,
            ItemGID = message.ItemGID,
            Status = 0
        });
    }
}
