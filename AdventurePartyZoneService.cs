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

internal sealed class AdventurePartyZoneService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdventurePartyZoneService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYREQUESTMEMBERZONES))]
    private void ReceivePartyRequestMemberZones(GAME_5_PROTOCOL.MSG_PARTYREQUESTMEMBERZONES message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYZONEREQUEST))]
    private void ReceivePartyZoneRequest(GAME_5_PROTOCOL.MSG_PARTYZONEREQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATEADVENTUREPOWERCOOLDOWN))]
    private void ReceiveUpdateAdventurePowerCooldown(WIZARD2_53_PROTOCOL.MSG_UPDATEADVENTUREPOWERCOOLDOWN message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEADVENTUREPOWERCOOLDOWN {
            AdventurePowerCooldownTime = message.AdventurePowerCooldownTime
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RECLAIMPARTYNAME))]
    private void ReceiveReclaimPartyName(WIZARD3_56_PROTOCOL.MSG_RECLAIMPARTYNAME message) {
        SendToSocket(message);
    }
}
