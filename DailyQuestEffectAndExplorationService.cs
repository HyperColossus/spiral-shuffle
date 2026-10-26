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

internal sealed class DailyQuestEffectAndExplorationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DailyQuestEffectAndExplorationService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTCSRDATA))]
    private void ReceiveDailyQuestCSRData(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTCSRDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTEXPLORE))]
    private void ReceiveDailyQuestExplore(WIZARDHOUSING_50_PROTOCOL.MSG_DAILYQUESTEXPLORE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DAILYQUESTEFFECT))]
    private void ReceiveDailyQuestEffect(WIZARD2_53_PROTOCOL.MSG_DAILYQUESTEFFECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_DECLINEQUEST))]
    private void ReceiveDeclineQuest(QUEST_MESSAGES_52_PROTOCOL.MSG_DECLINEQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_DEBUGQUESTDATA))]
    private void ReceiveDebugQuestData(QUEST_MESSAGES_52_PROTOCOL.MSG_DEBUGQUESTDATA message) {
        SendToSocket(message);
    }
}
