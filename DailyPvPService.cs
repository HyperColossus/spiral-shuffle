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

internal sealed class DailyPvPService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DailyPvPService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYPVPOPEN))]
    private void ReceiveDailyPvPOpen(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYPVPOPEN message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DailyPvPUpdate {
            ConsecutiveDays = 1,
            Flags = 1,
            TimeLeft = 86400
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DailyPvPUpdate))]
    private void ReceiveDailyPvPUpdate(WIZARDHOUSING_50_PROTOCOL.MSG_DailyPvPUpdate message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DAILYPVPCOMPLETED))]
    private void ReceiveDailyPvPCompleted(WIZARD2_53_PROTOCOL.MSG_DAILYPVPCOMPLETED message) {
        if (GetActiveWizard() is { } wizard) {
            wizard.AddGold(250);
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wizard.GameStats.m_currentGold
            });
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_DAILYPVPCOMPLETED {
            ConsecutiveDays = message.ConsecutiveDays,
            ItemTemplateID = 2001
        });
    }
}
