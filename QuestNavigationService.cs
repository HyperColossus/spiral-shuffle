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

internal sealed class QuestNavigationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new QuestNavigationService(parentActor));

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_UPDATEUSERHIDDENQUESTS))]
    private void ReceiveUpdateUserHiddenQuests(QUEST_MESSAGES_52_PROTOCOL.MSG_UPDATEUSERHIDDENQUESTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTNEXTCLOSESTQUEST))]
    private void ReceiveRequestNextClosestQuest(WIZARD_12_PROTOCOL.MSG_REQUESTNEXTCLOSESTQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_REQUESTNEXTCLOSESTQUEST {
            NPCList = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTQUESTDIALOG))]
    private void ReceiveRequestQuestDialog(WIZARD_12_PROTOCOL.MSG_REQUESTQUESTDIALOG message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTFALLBACK))]
    private void ReceiveRequestFallback(WIZARD3_56_PROTOCOL.MSG_REQUESTFALLBACK message) {
        SendToSocket(message);
    }
}
