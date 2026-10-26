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

internal sealed class BuddyListService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BuddyListService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETINVISIBLETOFRIENDS))]
    private void ReceiveSetInvisibleToFriends(WIZARDHOUSING_50_PROTOCOL.MSG_SETINVISIBLETOFRIENDS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUDDYDROP))]
    private void ReceiveBuddyDrop(GAME_5_PROTOCOL.MSG_BUDDYDROP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUDDYSTATUSUPDATE))]
    private void ReceiveBuddyStatusUpdate(GAME_5_PROTOCOL.MSG_BUDDYSTATUSUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_BUDDYREQUESTACCEPTFWD))]
    private void ReceiveBuddyRequestAcceptFwd(GAME_5_PROTOCOL.MSG_BUDDYREQUESTACCEPTFWD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTMAXFRIENDS))]
    private void ReceiveRequestMaxFriends(GAME_5_PROTOCOL.MSG_REQUESTMAXFRIENDS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new GAME_5_PROTOCOL.MSG_REQUESTMAXFRIENDS {
            RequestingPlayerGID = wizard.GameObjectID,
            MaximumFriends = 100,
            MaximumSubscriberFriends = 150
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_DECORATORBUDDYREQUEST))]
    private void ReceiveDecoratorBuddyRequest(WIZARD3_56_PROTOCOL.MSG_DECORATORBUDDYREQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_DECORATORBUDDYREQUEST {
            Buffer = ByteString.Empty,
            Status = 0,
            TemplateID = message.TemplateID
        });
    }
}
