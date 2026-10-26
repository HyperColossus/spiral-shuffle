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

internal sealed class NpcSpeechAndPersonaService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new NpcSpeechAndPersonaService(parentActor));

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_NPCINFO))]
    private void ReceiveNPCInfo(QUEST_MESSAGES_52_PROTOCOL.MSG_NPCINFO message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NPCSPEECH))]
    private void ReceiveNPCSpeech(GAME_5_PROTOCOL.MSG_NPCSPEECH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_PERSONAINFO))]
    private void ReceivePersonaInfo(QUEST_MESSAGES_52_PROTOCOL.MSG_PERSONAINFO message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_OBJECT_INFO_QUERY))]
    private void ReceiveObjectInfoQuery(GAME_5_PROTOCOL.MSG_OBJECT_INFO_QUERY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_PARTYLEVELUP))]
    private void ReceivePartyLevelUp(GAME_5_PROTOCOL.MSG_PARTYLEVELUP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_OPENCLASSPROJECTPORTAL))]
    private void ReceiveOpenClassProjectPortal(WIZARD2_53_PROTOCOL.MSG_OPENCLASSPROJECTPORTAL message) {
        SendToSocket(message);
    }
}
