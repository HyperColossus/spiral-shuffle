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

internal sealed class BadgeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BadgeService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTNEWBADGE))]
    private void ReceiveRequestNewBadge(WIZARD3_56_PROTOCOL.MSG_REQUESTNEWBADGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        string title = message.BadgeID != 0 ? $"Badge_{message.BadgeID}" : string.Empty;
        wizard.SetBadgeOverride(title);

        SendToSocket(new GAME_5_PROTOCOL.MSG_BADGES {
            CurrentBadge = message.BadgeID,
            UpdateAll = 1,
            Display = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTPLAYERBADGE))]
    private void ReceiveRequestPlayerBadge(WIZARD3_56_PROTOCOL.MSG_REQUESTPLAYERBADGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        string currentTitle = wizard.PlayerNameBehavior.BadgeTitle.ToString();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTPLAYERBADGE {
            GlobalID = message.GlobalID,
            RequestedID = message.RequestedID,
            BadgeName = string.IsNullOrEmpty(currentTitle) ? "Novice Wizard" : currentTitle,
            BadgeDescription = "An accomplished wizard of the Spiral."
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_SELECT_BADGE))]
    private void ReceiveSelectBadge(GAME_5_PROTOCOL.MSG_SELECT_BADGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.SetBadgeOverride(message.BadgeNameID != 0 ? $"Badge_{message.BadgeNameID}" : string.Empty);
        SendToSocket(new GAME_5_PROTOCOL.MSG_SELECT_BADGE {
            BadgeNameID = message.BadgeNameID,
            Error = 0
        });
    }
}
