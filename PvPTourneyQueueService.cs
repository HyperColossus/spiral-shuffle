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

internal sealed class PvPTourneyQueueService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PvPTourneyQueueService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PLAYERHOSTABNLETOURNEYLIST))]
    private void ReceivePlayerHostableTourneyList(WIZARD3_56_PROTOCOL.MSG_PLAYERHOSTABNLETOURNEYLIST message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PLAYERHOSTABNLETOURNEYLIST {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            Buffer = ByteString.Empty,
            IsPlayerAccountAlreadyHosting = 0,
            IsHostedTourneyScheduleFull = 0,
            MinLevelToHostTournament = 50
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPTOURNEYHOSTINGCREDIT))]
    private void ReceivePvPTourneyHostingCredit(WIZARD3_56_PROTOCOL.MSG_PVPTOURNEYHOSTINGCREDIT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPQUEUE))]
    private void ReceivePvPQueue(WIZARD_12_PROTOCOL.MSG_PVPQUEUE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PVPQUEUE {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            PvPQueue = message.PvPQueue,
            Clear = message.Clear
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPINTENT))]
    private void ReceivePvPIntent(WIZARD_12_PROTOCOL.MSG_PVPINTENT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPREFUNDPVPTOURNEYCURRENCY))]
    private void ReceivePvPRefundPvPTourneyCurrency(WIZARD3_56_PROTOCOL.MSG_PVPREFUNDPVPTOURNEYCURRENCY message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PVPREFUNDPVPTOURNEYCURRENCY {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            BracketID = message.BracketID,
            Bracket = message.Bracket,
            GroupJoinTeamID = message.GroupJoinTeamID,
            PricePvPTourneyCurrency = message.PricePvPTourneyCurrency,
            SubPricePvPTourneyCurrency = message.SubPricePvPTourneyCurrency
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTPVPACTOR))]
    private void ReceiveRequestPvPActor(WIZARD_12_PROTOCOL.MSG_REQUESTPVPACTOR message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_REQUESTPVPACTOR {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            TournamentNameID = message.TournamentNameID,
            MatchNameID = message.MatchNameID,
            RequestID = message.RequestID
        });
    }
}
