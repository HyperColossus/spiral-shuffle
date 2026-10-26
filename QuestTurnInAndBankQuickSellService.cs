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

internal sealed class QuestTurnInAndBankQuickSellService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new QuestTurnInAndBankQuickSellService(parentActor));

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_QUESTREADYTOTURNIN))]
    private void ReceiveQuestReadyToTurnIn(QUEST_MESSAGES_52_PROTOCOL.MSG_QUESTREADYTOTURNIN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_QUESTDIALOG))]
    private void ReceiveQuestDialog(WIZARD_12_PROTOCOL.MSG_QUESTDIALOG message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_QUICKSELLREQUESTBANK))]
    private void ReceiveQuickSellRequestBank(WIZARD_12_PROTOCOL.MSG_QUICKSELLREQUESTBANK message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_QUICKSELLREQUESTBANK());
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PREVIEW_ISLAND))]
    private void ReceivePreviewIsland(GAME_5_PROTOCOL.MSG_PREVIEW_ISLAND message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PRELEADERBOARD))]
    private void ReceivePreLeaderboard(WIZARD_12_PROTOCOL.MSG_PRELEADERBOARD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PREMIUMCONTENT))]
    private void ReceivePremiumContent(WIZARD_12_PROTOCOL.MSG_PREMIUMCONTENT message) {
        SendToSocket(message);
    }
}
