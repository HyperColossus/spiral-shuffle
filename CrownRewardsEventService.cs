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

internal sealed class CrownRewardsEventService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CrownRewardsEventService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ADDCROWNSREWARDSEVENT))]
    private void ReceiveAddCrownsRewardsEvent(WIZARD2_53_PROTOCOL.MSG_ADDCROWNSREWARDSEVENT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CROWNSBUYREQUEST))]
    private void ReceiveCrownsBuyRequest(WIZARD_12_PROTOCOL.MSG_CROWNSBUYREQUEST message) {
        SendToSocket(message);
    }
}
