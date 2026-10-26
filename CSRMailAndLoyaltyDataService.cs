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

internal sealed class CSRMailAndLoyaltyDataService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CSRMailAndLoyaltyDataService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSR_MAIL_DATA))]
    private void ReceiveCSRMailData(GAME_5_PROTOCOL.MSG_CSR_MAIL_DATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CSR_RECEIVE_GIFTS))]
    private void ReceiveCSRReceiveGifts(GAME_5_PROTOCOL.MSG_CSR_RECEIVE_GIFTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CSRDELETEGIFT))]
    private void ReceiveCSRDeleteGift(WIZARD2_53_PROTOCOL.MSG_CSRDELETEGIFT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CSRSETFRIENDSTATUS))]
    private void ReceiveCSRSetFriendStatus(WIZARD2_53_PROTOCOL.MSG_CSRSETFRIENDSTATUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CSRLOYALTYPAGEDATA))]
    private void ReceiveCSRLoyaltyPageData(WIZARD3_56_PROTOCOL.MSG_CSRLOYALTYPAGEDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSCSRRESULTS))]
    private void ReceiveClassProjectsCSRResults(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSCSRRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSTARTTIME))]
    private void ReceiveClassProjectStartTime(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSTARTTIME message) {
        SendToSocket(message);
    }
}
