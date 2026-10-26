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

internal sealed class PixiePostService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PixiePostService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTMAIL))]
    private void ReceivePixiePostMail(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTMAIL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTMESSAGE))]
    private void ReceivePixiePostMessage(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTMESSAGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PLAYPIXIEPOSTANIMATION {
            PlayerGID = wizard.GameObjectID,
            IsSend = 1
        });

        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTPIXIEPOSTMESSAGE))]
    private void ReceiveRequestPixiePostMessage(WIZARD3_56_PROTOCOL.MSG_REQUESTPIXIEPOSTMESSAGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTPIXIEPOSTMESSAGE {
            TimeOffset = message.TimeOffset,
            SenderGID = 1001,
            Subject = "Welcome to Pixie Post",
            Message = "Greetings from Wizard City Postal Service!",
            NameBlob = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            MessageNumber = 1,
            TotalMessages = 1,
            MailGID = 1001,
            PartyName = 0,
            PartyNameLocale = 0,
            SentTime = 1,
            ExpireTime = 86400,
            Data = "",
            AllowRecentWizards = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PLAYPIXIEPOSTANIMATION))]
    private void ReceivePlayPixiePostAnimation(WIZARD3_56_PROTOCOL.MSG_PLAYPIXIEPOSTANIMATION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTGUILDMAIL))]
    private void ReceivePixiePostGuildMail(WIZARD3_56_PROTOCOL.MSG_PIXIEPOSTGUILDMAIL message) {
        SendToSocket(message);
    }
}
