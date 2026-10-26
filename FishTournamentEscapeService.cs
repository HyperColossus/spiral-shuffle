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

internal sealed class FishTournamentEscapeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FishTournamentEscapeService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_FISHESCAPED))]
    private void ReceiveFishEscaped(WIZARDHOUSING_50_PROTOCOL.MSG_FISHESCAPED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_FISHINGCSRRESULTS))]
    private void ReceiveFishingCSRResults(WIZARDHOUSING_50_PROTOCOL.MSG_FISHINGCSRRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_FISHTOURNAMENTOPEN))]
    private void ReceiveFishTournamentOpen(WIZARDHOUSING_50_PROTOCOL.MSG_FISHTOURNAMENTOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_EXITBENEFICIALOBJECT))]
    private void ReceiveExitBeneficialObject(CANTRIPSMESSAGES_57_PROTOCOL.MSG_EXITBENEFICIALOBJECT message) {
        SendToSocket(message);
    }
}
