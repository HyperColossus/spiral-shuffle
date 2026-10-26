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

internal sealed class FishMarketService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FishMarketService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SELLFISHOPEN))]
    private void ReceiveSellFishOpen(WIZARDHOUSING_50_PROTOCOL.MSG_SELLFISHOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SELLFISHREQUEST))]
    private void ReceiveSellFishRequest(WIZARDHOUSING_50_PROTOCOL.MSG_SELLFISHREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.AddGold(100);
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_FISHTOURNAMENTLEADERBOARDREQUEST))]
    private void ReceiveFishTournamentLeaderboard(WIZARDHOUSING_50_PROTOCOL.MSG_FISHTOURNAMENTLEADERBOARDREQUEST message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_FISHTOURNAMENTLEADERBOARDOPEN {
            ThisWeeksExpireTime = 604800,
            LastWeeksExpireTime = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTALLFISH))]
    private void ReceiveRequestAllFish(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTALLFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTFISHHISTORY))]
    private void ReceiveRequestFishHistory(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTFISHHISTORY message) {
        SendToSocket(message);
    }
}
