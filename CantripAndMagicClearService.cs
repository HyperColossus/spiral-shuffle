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

internal sealed class CantripAndMagicClearService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CantripAndMagicClearService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCLEAR))]
    private void ReceiveCastleMagicClear(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCLEAR message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCLEAR());
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHCASTLEMAGIC))]
    private void ReceivePatchCastleMagic(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHCASTLEMAGIC message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHCASTLEMAGIC {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPSRESPONSEERROR))]
    private void ReceiveCantripsResponseError(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPSRESPONSEERROR message) {
        SendToSocket(new CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPSRESPONSEERROR {
            ProblemType = message.ProblemType
        });
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPTUTORIALEFFECT))]
    private void ReceiveCantripTutorialEffect(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPTUTORIALEFFECT message) {
        var wizard = GetActiveWizard();
        SendToSocket(new CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPTUTORIALEFFECT {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID
        });
    }
}
