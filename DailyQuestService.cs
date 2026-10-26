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
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class DailyQuestService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DailyQuestService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTOPEN))]
    private void ReceiveDailyQuestOpen(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTOPEN message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DailyQuestUpdate {
            QuestIndex = 1,
            CurrentCircle = 1,
            ConsecutiveDays = 5,
            Flags = 0,
            TimeLeft = 86400
        });

        Logger.Information("Sent DailyQuestUpdate to player {0}", Logger.Args(wizard.GameObjectID));
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTCOMPLETED))]
    private void ReceiveDailyQuestCompleted(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTCOMPLETED message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.AddGold(250);
        wizard.GameStats.m_currentPvPCurrency += 5;

        if (wizard.Account is not null) {
            wizard.Account.SetCrowns(wizard.Account.Crowns + 10);
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DailyQuestUpdate {
            QuestIndex = message.QuestIndex,
            CurrentCircle = message.CurrentCircle,
            ConsecutiveDays = (byte)(message.ConsecutiveDays + 1),
            Flags = 1,
            TimeLeft = 86400
        });

        Logger.Information("Daily quest completed for player {0}. Streak: {1}",
            Logger.Args(wizard.GameObjectID, message.ConsecutiveDays + 1));
    }
}
