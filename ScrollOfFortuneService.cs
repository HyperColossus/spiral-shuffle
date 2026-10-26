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

internal sealed class ScrollOfFortuneService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ScrollOfFortuneService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BUYSEASONPASS))]
    private void ReceiveBuySeasonPass(WIZARD2_53_PROTOCOL.MSG_BUYSEASONPASS message) {
        if (GetActiveWizard() is not { } wizard) return;

        const int PassCost = 7500;
        if (wizard.Account.Crowns >= PassCost) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - PassCost);
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BUYSEASONPASS {
            ProjectID = message.ProjectID,
            Purchase = 1,
            Credits = message.Credits,
            BattlePass = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADVANCESEASONPASS))]
    private void ReceiveAdvanceSeasonPass(WIZARD3_56_PROTOCOL.MSG_ADVANCESEASONPASS message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_ADVANCESEASONPASS {
            ProjectID = message.ProjectID,
            Advance = 1,
            BattlePass = message.BattlePass,
            Credits = message.Credits,
            StartRewardIndex = message.StartRewardIndex,
            EndRewardIndex = message.EndRewardIndex,
            NewTotalProgress = message.NewTotalProgress
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CROWNSREWARDSTIERREACHED))]
    private void ReceiveCrownsRewardsTierReached(WIZARD2_53_PROTOCOL.MSG_CROWNSREWARDSTIERREACHED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REMOVECROWNSREWARDSEVENT))]
    private void ReceiveRemoveCrownsRewardsEvent(WIZARD2_53_PROTOCOL.MSG_REMOVECROWNSREWARDSEVENT message) {
        SendToSocket(message);
    }
}
