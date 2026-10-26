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

internal sealed class PvPTournamentService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PvPTournamentService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPCONFIRMTOURNEY))]
    private void ReceivePvPConfirmTourney(WIZARD_12_PROTOCOL.MSG_PVPCONFIRMTOURNEY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PVPCONFIRMTOURNEY {
            CharacterID = wizard.CharId,
            RegistrationID = message.RegistrationID,
            BracketID = message.BracketID > 0 ? message.BracketID : 1,
            PricePvPTourneyCurrency = message.PricePvPTourneyCurrency,
            PlayerPvPTourneyCurrency = 1000
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVP5THAGEKIOSKREQUEST))]
    private void ReceivePvP5thAgeKiosk(WIZARD3_56_PROTOCOL.MSG_PVP5THAGEKIOSKREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_AdvPvPEloData {
            GlobalID = wizard.GameObjectID,
            EntryList = "Rank: Warlord, Rating: 1500",
            CheckCompletion = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PvPStatueInspect))]
    private void ReceivePvPStatueInspect(WIZARD3_56_PROTOCOL.MSG_PvPStatueInspect message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVP5THAGECANJOINMATCH))]
    private void ReceivePvP5thAgeCanJoinMatch(WIZARD3_56_PROTOCOL.MSG_PVP5THAGECANJOINMATCH message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PVP5THAGECANJOINMATCHRESPONSE {
            CanJoinQueue = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVP5THAGEOPENPVPWINDOW))]
    private void ReceivePvP5thAgeOpenPvPWindow(WIZARD3_56_PROTOCOL.MSG_PVP5THAGEOPENPVPWINDOW message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_AdvPvPLeaderboardDataRequest))]
    private void ReceiveAdvPvPLeaderboardDataRequest(WIZARD3_56_PROTOCOL.MSG_AdvPvPLeaderboardDataRequest message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_AdvPvPLeaderboardData {
            LeagueID = message.LeagueID,
            SeasonID = message.SeasonID,
            StartTime = message.StartTime,
            PlayerGID = wizard.GameObjectID,
            EntryList = "Leaderboard: Top Ranked Duelists"
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_EXPANDPVPSEARCH))]
    private void ReceiveExpandPvPSearch(WIZARD_12_PROTOCOL.MSG_EXPANDPVPSEARCH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_ALLOWLEAVEPVP))]
    private void ReceiveAllowLeavePvP(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_ALLOWLEAVEPVP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPCONFIRMHOSTTOURNEY))]
    private void ReceivePvPConfirmHostTourney(WIZARD3_56_PROTOCOL.MSG_PVPCONFIRMHOSTTOURNEY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PVPCONFIRMHOSTTOURNEY {
            CharacterID = wizard.CharId,
            HostingRequestID = message.HostingRequestID,
            HostingCrownsCost = 100,
            TotalCrowns = wizard.Account.Crowns,
            WebFailure = 0,
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPCONSUMECROWNSTOHOSTTOURNEY))]
    private void ReceivePvPConsumeCrownsToHostTourney(WIZARD3_56_PROTOCOL.MSG_PVPCONSUMECROWNSTOHOSTTOURNEY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        const int HostCost = 100;
        if (wizard.Account.Crowns >= HostCost) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - HostCost);
        }

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_HOSTTOURNEYCONFIRM {
            CharacterID = wizard.CharId,
            CreatedTourneyBracketID = 101,
            Success = 1,
            Credits = wizard.Account.Crowns,
            WebFailure = 0
        });
    }
}
