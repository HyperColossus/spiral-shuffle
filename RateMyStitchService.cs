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

internal sealed class RateMyStitchService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new RateMyStitchService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUEST))]
    private void ReceiveRateMyStitchRequest(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUESTOUTFITS {
            GlobalID = message.GlobalID,
            Status = 1,
            Buffer = ""
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUESTOUTFITS))]
    private void ReceiveRateMyStitchRequestOutfits(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUESTOUTFITS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUESTLEADERBOARD))]
    private void ReceiveRateMyStitchRequestLeaderboard(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHREQUESTLEADERBOARD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHRATE))]
    private void ReceiveRateMyStitchRate(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHRATE message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHRATERESPONSE {
            CharacterID = message.CharacterID,
            Type = message.Type
        });
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHPOSTRATE {
            CharacterID = message.CharacterID,
            BadgeCredit = 1,
            MaxBadgeCredit = 10
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHEREQUESTREMOVE))]
    private void ReceiveRateMyStitchRemove(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHEREQUESTREMOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHADD))]
    private void ReceiveRateMyStitchAdd(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHADD message) {
        SendToSocket(message);
    }
}
