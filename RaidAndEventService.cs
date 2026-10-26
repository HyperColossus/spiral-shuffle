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

internal sealed class RaidAndEventService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new RaidAndEventService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RAIDGATEUNLOCKREQUEST))]
    private void ReceiveRaidGateUnlock(WIZARD3_56_PROTOCOL.MSG_RAIDGATEUNLOCKREQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATEEVENTCURRENCY1))]
    private void ReceiveUpdateEventCurrency1(WIZARD2_53_PROTOCOL.MSG_UPDATEEVENTCURRENCY1 message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATEEVENTCURRENCY2))]
    private void ReceiveUpdateEventCurrency2(WIZARD2_53_PROTOCOL.MSG_UPDATEEVENTCURRENCY2 message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTJOINEVENTTEAM))]
    private void ReceiveRequestJoinEventTeam(WIZARD3_56_PROTOCOL.MSG_REQUESTJOINEVENTTEAM message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTJOINEVENTTEAM {
            ProjectID = message.ProjectID,
            WorldName = message.WorldName,
            ZoneList = message.ZoneList,
            PlayerGID = message.PlayerGID,
            Status = 1,
            Level = message.Level,
            SchoolID = message.SchoolID,
            MetRequirements = 1
        });
    }
}
