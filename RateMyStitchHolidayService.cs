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

internal sealed class RateMyStitchHolidayService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new RateMyStitchHolidayService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHCSRRESULTS))]
    private void ReceiveRateMyStitchCSRResults(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHCSRRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHHELP))]
    private void ReceiveRateMyStitchHelp(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHHELP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHHOLIDAYFLAGS))]
    private void ReceiveRateMyStitchHolidayFlags(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHHOLIDAYFLAGS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHRATEFAIL))]
    private void ReceiveRateMyStitchRateFail(WIZARD3_56_PROTOCOL.MSG_RATEMYSTITCHRATEFAIL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_RANKING))]
    private void ReceiveRanking(GAME_5_PROTOCOL.MSG_RANKING message) {
        SendToSocket(message);
    }
}
