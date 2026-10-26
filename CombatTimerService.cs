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

internal sealed class CombatTimerService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CombatTimerService(parentActor));

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATMOVESELECTION))]
    private void ReceiveCombatMoveSelection(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATMOVESELECTION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETPLANNINGPHASETIMER))]
    private void ReceiveSetPlanningPhaseTimer(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETPLANNINGPHASETIMER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETDUELTIMER))]
    private void ReceiveSetDuelTimer(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETDUELTIMER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETSTATUS))]
    private void ReceiveSetStatus(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SETSTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATEAFTERCOMBATDANCE))]
    private void ReceiveUpdateAfterCombatDance(WIZARD2_53_PROTOCOL.MSG_UPDATEAFTERCOMBATDANCE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEAFTERCOMBATDANCE {
            PlayerGID = wizard.GameObjectID,
            AfterCombatDance = message.AfterCombatDance
        });
    }
}
