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

internal sealed class CantripExtensionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CantripExtensionService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTMAGICWEAVINGTOME))]
    private void ReceiveRequestMagicWeavingTome(WIZARD3_56_PROTOCOL.MSG_REQUESTMAGICWEAVINGTOME message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTMAGICWEAVINGTOME {
            GlobalID = wizard.GameObjectID,
            npcGlobalID = message.npcGlobalID
        });
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPAFFECTEDPLAYER))]
    private void ReceiveCantripAffectedPlayer(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPAFFECTEDPLAYER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPENDLOOP))]
    private void ReceiveCantripEndLoop(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPENDLOOP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPLEVELUP))]
    private void ReceiveCantripLevelUp(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPLEVELUP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPLEVELUP {
            GlobalID = wizard.GameObjectID,
            NewLevel = message.NewLevel
        });
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_UPDATECANTRIPXP))]
    private void ReceiveUpdateCantripXP(CANTRIPSMESSAGES_57_PROTOCOL.MSG_UPDATECANTRIPXP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new CANTRIPSMESSAGES_57_PROTOCOL.MSG_UPDATECANTRIPXP {
            GlobalID = wizard.GameObjectID,
            XP = message.XP,
            Level = message.Level
        });
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_UPDATERITUALOBJECT))]
    private void ReceiveUpdateRitualObject(CANTRIPSMESSAGES_57_PROTOCOL.MSG_UPDATERITUALOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCANTRIP))]
    private void ReceiveCastleMagicCantrip(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICCANTRIP message) {
        SendToSocket(message);
    }
}
