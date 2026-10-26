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

internal sealed class AdventureCatchAndMailService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdventureCatchAndMailService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTADVENTUREPARTY))]
    private void ReceiveRequestAdventureParty(WIZARD_12_PROTOCOL.MSG_REQUESTADVENTUREPARTY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTCATCHSUCCESS))]
    private void ReceiveRequestCatchSuccess(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTCATCHSUCCESS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTCHATSTATS))]
    private void ReceiveRequestChatStats(WIZARD2_53_PROTOCOL.MSG_REQUESTCHATSTATS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTDELETEMESSAGE))]
    private void ReceiveRequestDeleteMessage(WIZARD3_56_PROTOCOL.MSG_REQUESTDELETEMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REPORTADTAKEOVER))]
    private void ReceiveReportAdTakeover(GAME_5_PROTOCOL.MSG_REPORTADTAKEOVER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REGISTRAR))]
    private void ReceiveRegistrar(WIZARD_12_PROTOCOL.MSG_REGISTRAR message) {
        SendToSocket(message);
    }
}
