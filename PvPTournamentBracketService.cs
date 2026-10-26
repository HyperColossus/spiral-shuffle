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

internal sealed class PvPTournamentBracketService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PvPTournamentBracketService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_BRACKETREPORT))]
    private void ReceiveBracketReport(WIZARD_12_PROTOCOL.MSG_BRACKETREPORT message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_BRACKETREPORT {
            Report = message.Report
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_SUBOPTIMAL_BRACKET_RESPONSE))]
    private void ReceiveSuboptimalBracketResponse(GAME_5_PROTOCOL.MSG_SUBOPTIMAL_BRACKET_RESPONSE message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_SUBOPTIMAL_BRACKET_RESPONSE {
            Choice = message.Choice
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NEWTOURNEYREWARDS))]
    private void ReceiveNewTourneyRewards(GAME_5_PROTOCOL.MSG_NEWTOURNEYREWARDS message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_NEWTOURNEYREWARDS {
            Data = message.Data,
            BracketID = message.BracketID,
            TourneyNameSTKey = message.TourneyNameSTKey
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_FREETOURNEYCREDITINFO))]
    private void ReceiveFreeTourneyCreditInfo(WIZARD_12_PROTOCOL.MSG_FREETOURNEYCREDITINFO message) {
        // Members get daily free tournament credits
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_FREETOURNEYCREDITINFO {
            HasFreeCredit = 1
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PVPUPDATEINFO))]
    private void ReceivePvPUpdateInfo(GAME_5_PROTOCOL.MSG_PVPUPDATEINFO message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_PVPUPDATEINFO {
            TournamentInfo = message.TournamentInfo,
            CharacterID = wizard?.CharId ?? message.CharacterID,
            PromptMsg = message.PromptMsg,
            DiffType = message.DiffType,
            IsPvPQueue = message.IsPvPQueue,
            IsPlayerAccountAlreadyHosting = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PREPVPKIOSK))]
    private void ReceivePrePvPKiosk(WIZARD_12_PROTOCOL.MSG_PREPVPKIOSK message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PREPVPKIOSK {
            TournamentNameID = message.TournamentNameID,
            MobileID = message.MobileID,
            LeagueID = message.LeagueID,
            SeasonID = message.SeasonID,
            TransactionID = message.TransactionID,
            Patching = 0
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPCONSUMEPVPTOURNEYCURRENCY))]
    private void ReceivePvPConsumePvPTourneyCurrency(WIZARD_12_PROTOCOL.MSG_PVPCONSUMEPVPTOURNEYCURRENCY message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PVPCONSUMEPVPTOURNEYCURRENCY {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            BracketID = message.BracketID,
            Bracket = message.Bracket,
            GroupJoinTeamID = message.GroupJoinTeamID,
            PricePvPTourneyCurrency = message.PricePvPTourneyCurrency,
            SubPricePvPTourneyCurrency = message.SubPricePvPTourneyCurrency
        });
    }
}
