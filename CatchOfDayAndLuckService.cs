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

internal sealed class CatchOfDayAndLuckService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CatchOfDayAndLuckService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CATCHOFTHEDAYOPEN))]
    private void ReceiveCatchOfTheDayOpen(WIZARDHOUSING_50_PROTOCOL.MSG_CATCHOFTHEDAYOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CSRADDFISH))]
    private void ReceiveCSRAddFish(WIZARD2_53_PROTOCOL.MSG_CSRADDFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CSRTRASHFISH))]
    private void ReceiveCSRTrashFish(WIZARD2_53_PROTOCOL.MSG_CSRTRASHFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_BENEFICIALLUCKUPDATE))]
    private void ReceiveBeneficialLuckUpdate(CANTRIPSMESSAGES_57_PROTOCOL.MSG_BENEFICIALLUCKUPDATE message) {
        SendToSocket(message);
    }
}
