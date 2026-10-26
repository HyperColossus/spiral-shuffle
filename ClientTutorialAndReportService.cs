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

internal sealed class ClientTutorialAndReportService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ClientTutorialAndReportService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CLIENTTUTORIALEVENT))]
    private void ReceiveClientTutorialEvent(GAME_5_PROTOCOL.MSG_CLIENTTUTORIALEVENT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TUTORIALEVENT))]
    private void ReceiveTutorialEvent(WIZARD_12_PROTOCOL.MSG_TUTORIALEVENT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_DISMISSTUTORIALTIP))]
    private void ReceiveDismissTutorialTip(WIZARD_12_PROTOCOL.MSG_DISMISSTUTORIALTIP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_READTUTORIALTIP))]
    private void ReceiveReadTutorialTip(WIZARD2_53_PROTOCOL.MSG_READTUTORIALTIP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTTUTORIALTIPLOG))]
    private void ReceiveRequestTutorialTipLog(WIZARD2_53_PROTOCOL.MSG_REQUESTTUTORIALTIPLOG message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATETUTORIALTIPLOG {
            GlobalID = wizard?.CharId ?? message.GlobalID,
            TutorialTipData = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATETUTORIALTIPLOG))]
    private void ReceiveUpdateTutorialTipLog(WIZARD2_53_PROTOCOL.MSG_UPDATETUTORIALTIPLOG message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_SERVERTUTORIALCOMMAND))]
    private void ReceiveServerTutorialCommand(GAME_5_PROTOCOL.MSG_SERVERTUTORIALCOMMAND message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_TUTORIALS))]
    private void ReceiveTutorials(GAME_5_PROTOCOL.MSG_TUTORIALS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUGSUBMITRESPONSE))]
    private void ReceiveBugSubmitResponse(GAME_5_PROTOCOL.MSG_BUGSUBMITRESPONSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(EXTENDEDBASE_2_PROTOCOL.MSG_FORCE_DISCONNECT))]
    private void ReceiveForceDisconnect(EXTENDEDBASE_2_PROTOCOL.MSG_FORCE_DISCONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CLIENT_ERROR))]
    private void ReceiveClientError(GAME_5_PROTOCOL.MSG_CLIENT_ERROR message) {
        SendToSocket(message);
    }
}
