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

internal sealed class HousingArtifactAndRentalService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingArtifactAndRentalService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_RENTALUPDATE))]
    private void ReceiveRentalUpdate(WIZARD_12_PROTOCOL.MSG_RENTALUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTARTIFACTLOCATION))]
    private void ReceiveRequestArtifactLocation(WIZARD3_56_PROTOCOL.MSG_REQUESTARTIFACTLOCATION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REMOVEGUILDACHIEVEMENT))]
    private void ReceiveRemoveGuildAchievement(WIZARD3_56_PROTOCOL.MSG_REMOVEGUILDACHIEVEMENT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REMOVESPELLFUSIONFROMDECK))]
    private void ReceiveRemoveSpellFusionFromDeck(WIZARD3_56_PROTOCOL.MSG_REMOVESPELLFUSIONFROMDECK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_RELEASEDELAY))]
    private void ReceiveReleaseDelay(GAME_5_PROTOCOL.MSG_RELEASEDELAY message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_RELEASEDELAY());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_RESTOREORIGINALCASTLE))]
    private void ReceiveRestoreOriginalCastle(WIZARD2_53_PROTOCOL.MSG_RESTOREORIGINALCASTLE message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_RESTOREORIGINALCASTLE());
    }
}
