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

internal sealed class ChatChannelSubscriptionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ChatChannelSubscriptionService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NOTIFYCHANNELINSTANCE))]
    private void ReceiveNotifyChannelInstance(GAME_5_PROTOCOL.MSG_NOTIFYCHANNELINSTANCE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTADVANCECHANNELINSTANCE))]
    private void ReceiveRequestAdvanceChannelInstance(GAME_5_PROTOCOL.MSG_REQUESTADVANCECHANNELINSTANCE message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_NOTIFYCHANNELINSTANCE {
            RecipientID = message.SourceID,
            ParentID = 0,
            ID = message.ChannelID,
            Name = new WideByteString("Realm Channel"),
            Create = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_SETBASICCHATCHANNELPUBLIC))]
    private void ReceiveSetBasicChatChannelPublic(WIZARD2_53_PROTOCOL.MSG_SETBASICCHATCHANNELPUBLIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTLEAVECHANNELBYNAME))]
    private void ReceiveRequestLeaveChannelByName(GAME_5_PROTOCOL.MSG_REQUESTLEAVECHANNELBYNAME message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_LEAVECHANNELRESPONSE {
            SourceID = message.SourceID,
            ParentID = 0,
            ChannelID = 0,
            ChannelName = message.ChannelName,
            ChannelType = 0,
            ErrorCode = 0,
            RecipientID = message.SourceID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CHANNELSUBSCRIPTION))]
    private void ReceiveChannelSubscription(GAME_5_PROTOCOL.MSG_CHANNELSUBSCRIPTION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CHECKSUBSCRIPTION))]
    private void ReceiveCheckSubscription(GAME_5_PROTOCOL.MSG_CHECKSUBSCRIPTION message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_CHECKSUBSCRIPTION {
            Success = 1
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PERIODICSUBSCRIPTIONS))]
    private void ReceivePeriodicSubscriptions(WIZARD2_53_PROTOCOL.MSG_PERIODICSUBSCRIPTIONS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PERIODICSUBSCRIPTIONS {
            Success = 1,
            Data = ByteString.Empty
        });
    }
}
