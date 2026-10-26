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

internal sealed class HallOfFameService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HallOfFameService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVEHALLOFFAME))]
    private void ReceiveCastleToursRemoveHallOfFame(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVEHALLOFFAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPREADD))]
    private void ReceiveCastleToursPreAdd(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPREADD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHCLAIMJUDGESCHOICE))]
    private void ReceiveRateMyStitchClaimJudgesChoice(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHCLAIMJUDGESCHOICE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHCLAIMJUDGESCHOICE {
            GlobalID = wizard.GameObjectID
        });
    }
}
