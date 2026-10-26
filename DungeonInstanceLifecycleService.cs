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

internal sealed class DungeonInstanceLifecycleService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DungeonInstanceLifecycleService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLEARRESUMEINSTANCE))]
    private void ReceiveClearResumeInstance(WIZARD2_53_PROTOCOL.MSG_CLEARRESUMEINSTANCE message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CLEARRESUMEINSTANCE());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_COMPLETEINSTANCE))]
    private void ReceiveCompleteInstance(WIZARD2_53_PROTOCOL.MSG_COMPLETEINSTANCE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_COMPLETEINSTANCE2))]
    private void ReceiveCompleteInstance2(WIZARD2_53_PROTOCOL.MSG_COMPLETEINSTANCE2 message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLIENTZONED))]
    private void ReceiveClientZoned(WIZARD2_53_PROTOCOL.MSG_CLIENTZONED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_CANACQUIREWORLDELIXIR))]
    private void ReceiveCanAcquireWorldElixir(QUEST_MESSAGES_52_PROTOCOL.MSG_CANACQUIREWORLDELIXIR message) {
        SendToSocket(new QUEST_MESSAGES_52_PROTOCOL.MSG_CANACQUIREWORLDELIXIR {
            ElixirTemplateGID = message.ElixirTemplateGID,
            Result = 1
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLEARALLINTERACTIONS))]
    private void ReceiveClearAllInteractions(WIZARD2_53_PROTOCOL.MSG_CLEARALLINTERACTIONS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CLEARALLINTERACTIONS());
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_COMPANIONEFFECTS))]
    private void ReceiveCompanionEffects(GAME_5_PROTOCOL.MSG_COMPANIONEFFECTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CONTROLVISIBLITY))]
    private void ReceiveControlVisibility(CANTRIPSMESSAGES_57_PROTOCOL.MSG_CONTROLVISIBLITY message) {
        SendToSocket(message);
    }
}
