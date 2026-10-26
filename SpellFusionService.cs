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

internal sealed class SpellFusionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SpellFusionService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADDSPELLFUSIONTODECK))]
    private void ReceiveAddSpellFusionToDeck(WIZARD3_56_PROTOCOL.MSG_ADDSPELLFUSIONTODECK message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_ADDSPELLFUSIONTODECK {
            SpellID1 = message.SpellID1,
            SpellID2 = message.SpellID2,
            DeckID = message.DeckID,
            Success = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ADDSPELLTOBOOK))]
    private void ReceiveAddSpellToBook(WIZARD_12_PROTOCOL.MSG_ADDSPELLTOBOOK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ADDTREASURESPELLTOBOOK))]
    private void ReceiveAddTreasureSpellToBook(WIZARD_12_PROTOCOL.MSG_ADDTREASURESPELLTOBOOK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATENEWSPELLBOOKLAYOUTWARNING))]
    private void ReceiveUpdateNewSpellBookLayoutWarning(WIZARD2_53_PROTOCOL.MSG_UPDATENEWSPELLBOOKLAYOUTWARNING message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATENEWSPELLBOOKLAYOUTWARNING {
            WarningShown = 1
        });
    }
}
