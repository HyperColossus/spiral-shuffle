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

internal sealed class BeastmoonBattlegroundService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BeastmoonBattlegroundService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGOPENPOLYMORPHSELECT))]
    private void ReceiveBGOpenPolymorphSelect(WIZARD2_53_PROTOCOL.MSG_BGOPENPOLYMORPHSELECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGPOIUpdate))]
    private void ReceiveBGPOIUpdate(WIZARD2_53_PROTOCOL.MSG_BGPOIUpdate message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGPOIUpdate {
            Data = message.Data
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDSHOWCOMPASS))]
    private void ReceiveBattlegroundShowCompass(WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDSHOWCOMPASS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BATTLEGROUNDSHOWCOMPASS {
            ShowCompass = message.ShowCompass
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTBATTLEGROUNDSLOOT))]
    private void ReceiveRequestBattlegroundsLoot(WIZARD2_53_PROTOCOL.MSG_REQUESTBATTLEGROUNDSLOOT message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTBATTLEGROUNDSLOOT());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_SetBGPolymorphLevel))]
    private void ReceiveSetBGPolymorphLevel(WIZARD2_53_PROTOCOL.MSG_SetBGPolymorphLevel message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGPlayerSync))]
    private void ReceiveBGPlayerSync(WIZARD2_53_PROTOCOL.MSG_BGPlayerSync message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGPlayerSync {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            Data = message.Data,
            Flags = message.Flags
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGPlayerSyncStatus))]
    private void ReceiveBGPlayerSyncStatus(WIZARD2_53_PROTOCOL.MSG_BGPlayerSyncStatus message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGPlayerSyncStatus {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            TargetID = message.TargetID,
            Flags = message.Flags,
            Config = message.Config
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGQueueStatus))]
    private void ReceiveBGQueueStatus(WIZARD2_53_PROTOCOL.MSG_BGQueueStatus message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGQueueStatus {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            State = message.State,
            Lines = message.Lines
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGResetStats))]
    private void ReceiveBGResetStats(WIZARD2_53_PROTOCOL.MSG_BGResetStats message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGResetStats {
            Stats = message.Stats
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGSigilProxyEvent))]
    private void ReceiveBGSigilProxyEvent(WIZARD2_53_PROTOCOL.MSG_BGSigilProxyEvent message) {
        SendToSocket(message);
    }
}
