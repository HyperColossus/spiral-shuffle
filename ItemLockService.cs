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

internal sealed class ItemLockService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ItemLockService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTTOGGLELOCKITEM))]
    private void ReceiveRequestToggleLockItem(WIZARD_12_PROTOCOL.MSG_REQUESTTOGGLELOCKITEM message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_REQUESTTOGGLELOCKITEM {
            ItemGID = message.ItemGID,
            GlobalID = wizard.GameObjectID,
            IsLocked = message.IsLocked
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REMOVEITEMLOCKS))]
    private void ReceiveRemoveItemLocks(WIZARD2_53_PROTOCOL.MSG_REMOVEITEMLOCKS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REMOVEITEMLOCKS {
            GlobalID = wizard.GameObjectID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETLOCKUPDATE))]
    private void ReceiveJewelSocketLockUpdate(WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETLOCKUPDATE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETLOCKUPDATE {
            GlobalID = wizard.GameObjectID,
            ItemGID = message.ItemGID,
            Locked = message.Locked
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PUBLICJEWELSOCKETUPDATE))]
    private void ReceivePublicJewelSocketUpdate(WIZARD2_53_PROTOCOL.MSG_PUBLICJEWELSOCKETUPDATE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PUBLICJEWELSOCKETUPDATE {
            GlobalID = wizard.GameObjectID,
            ItemID = message.ItemID,
            SerializedInfo = message.SerializedInfo
        });
    }
}
