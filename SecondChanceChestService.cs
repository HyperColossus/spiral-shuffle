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

internal sealed class SecondChanceChestService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SecondChanceChestService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_PROMPT))]
    private void ReceiveRollPrompt(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_PROMPT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_RESPONSE))]
    private void ReceiveRollResponse(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_RESPONSE message) {
        if (GetActiveWizard() is not { } wizard) return;

        if (message.Response == 0) {
            return;
        }

        const int BaseCost = 50;
        if (wizard.Account.Crowns < BaseCost) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_ERROR());
            return;
        }

        wizard.Account.SetCrowns(wizard.Account.Crowns - BaseCost);

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PAID_LOOT_CROWNS_BALANCE {
            Balance = wizard.Account.Crowns
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PAID_LOOT_ROLL_RESULT {
            Id = message.Id,
            Cost = BaseCost * 2,
            Balance = wizard.Account.Crowns,
            Uses = 1,
            Loot = "BossSecondChanceLoot"
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_CROWNS_BALANCE))]
    private void ReceiveCrownsBalance(WIZARD_12_PROTOCOL.MSG_PAID_LOOT_CROWNS_BALANCE message) {
        if (GetActiveWizard() is { } wizard) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_PAID_LOOT_CROWNS_BALANCE {
                Balance = wizard.Account.Crowns
            });
        }
    }
}
