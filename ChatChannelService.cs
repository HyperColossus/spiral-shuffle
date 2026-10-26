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

internal sealed class ChatChannelService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ChatChannelService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTCREATECHANNEL))]
    private void ReceiveCreateChannel(GAME_5_PROTOCOL.MSG_REQUESTCREATECHANNEL message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_CREATECHANNELRESPONSE {
            SourceID = message.SourceID,
            ChannelID = 1001,
            ChannelName = message.Name,
            ErrorCode = 0
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTJOINCHANNEL))]
    private void ReceiveJoinChannel(GAME_5_PROTOCOL.MSG_REQUESTJOINCHANNEL message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_JOINCHANNELRESPONSE {
            SourceID = message.SourceID,
            ChannelID = message.ChannelID,
            ChannelName = "General",
            ErrorCode = 0,
            ChannelType = 1
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTJOINCHANNELBYNAME))]
    private void ReceiveJoinChannelByName(GAME_5_PROTOCOL.MSG_REQUESTJOINCHANNELBYNAME message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_JOINCHANNELRESPONSE {
            SourceID = message.SourceID,
            ChannelID = 1001,
            ChannelName = message.ChannelName,
            ErrorCode = 0,
            ChannelType = 1
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTLEAVECHANNEL))]
    private void ReceiveLeaveChannel(GAME_5_PROTOCOL.MSG_REQUESTLEAVECHANNEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_JOINBASICCHATCHANNEL))]
    private void ReceiveJoinBasicChatChannel(WIZARD2_53_PROTOCOL.MSG_JOINBASICCHATCHANNEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_INVITETOBASICCHATCHANNEL))]
    private void ReceiveInviteBasicChatChannel(WIZARD2_53_PROTOCOL.MSG_INVITETOBASICCHATCHANNEL message) {
        SendToSocket(message);
    }
}
