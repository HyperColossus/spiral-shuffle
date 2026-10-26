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

internal sealed class ChatCodeAndWhisperService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ChatCodeAndWhisperService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTCHATCODE))]
    private void ReceiveRequestChatCode(GAME_5_PROTOCOL.MSG_REQUESTCHATCODE message) {
        var wizard = GetActiveWizard();
        // Generate random 8-character alphanumeric friend chat code
        string codeStr = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        SendToSocket(new GAME_5_PROTOCOL.MSG_REQUESTCHATCODE {
            ListOwnerGID = wizard?.GameObjectID ?? message.ListOwnerGID,
            Code = new ByteString(codeStr),
            CodeTime = 172800, // 48 hours validity
            SpecialCode = 0,
            Forwarded = 0,
            NameBlob = message.NameBlob
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_SENDCHATCODE))]
    private void ReceiveSendChatCode(GAME_5_PROTOCOL.MSG_SENDCHATCODE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_SENDCHATCODE {
            ListOwnerGID = wizard?.GameObjectID ?? message.ListOwnerGID,
            Code = message.Code,
            Error = 0,
            UseSuccess = 1,
            CreatorName = message.CreatorName
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_USECHATCODE))]
    private void ReceiveUseChatCode(GAME_5_PROTOCOL.MSG_USECHATCODE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_SENDCHATCODE {
            ListOwnerGID = wizard?.GameObjectID ?? message.ListOwnerGID,
            Code = message.Code,
            Error = 0,
            UseSuccess = 1,
            CreatorName = new ByteString(wizard is not null ? wizard.PlayerNameBehavior.NameOverride.ToString() : "Friend")
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTDIRECTEDCHATBYNAME))]
    private void ReceiveRequestDirectedChatByName(GAME_5_PROTOCOL.MSG_REQUESTDIRECTEDCHATBYNAME message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_DIRECTEDCHATBYNAMERESPONSE {
            TargetID = 0,
            TargetName = message.TargetName
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CHATFILTERBLACK))]
    private void ReceiveChatFilterBlack(WIZARD_12_PROTOCOL.MSG_CHATFILTERBLACK message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CHATFILTERBLACK {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            Blacklist = message.Blacklist
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CHATFILTERWHITE))]
    private void ReceiveChatFilterWhite(WIZARD_12_PROTOCOL.MSG_CHATFILTERWHITE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CHATFILTERWHITE {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            Whitelist = message.Whitelist
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CHATERROR))]
    private void ReceiveChatError(GAME_5_PROTOCOL.MSG_CHATERROR message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CHATDIAGNOSTICS))]
    private void ReceiveChatDiagnostics(GAME_5_PROTOCOL.MSG_CHATDIAGNOSTICS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQCHATDIAGNOSTICS))]
    private void ReceiveReqChatDiagnostics(GAME_5_PROTOCOL.MSG_REQCHATDIAGNOSTICS message) {
        var wizard = GetActiveWizard();
        SendToSocket(new GAME_5_PROTOCOL.MSG_CHATDIAGNOSTICS {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            Data = message.Data
        });
    }
}
