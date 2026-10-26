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

internal sealed class SystemMetricsAndPatchTelemetryService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SystemMetricsAndPatchTelemetryService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NEWSYSSTATS))]
    private void ReceiveNewSysStats(GAME_5_PROTOCOL.MSG_NEWSYSSTATS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_NEWTICKCNT))]
    private void ReceiveNewTickCnt(GAME_5_PROTOCOL.MSG_NEWTICKCNT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PATCH_8_PROTOCOL.MSG_NEXT_VERSION))]
    private void ReceiveNextVersion(PATCH_8_PROTOCOL.MSG_NEXT_VERSION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PATCH_8_PROTOCOL.MSG_LATEST_FILE_LIST))]
    private void ReceiveLatestFileList(PATCH_8_PROTOCOL.MSG_LATEST_FILE_LIST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PCS_CACHESEGREQSSUMMARY_REQUEST))]
    private void ReceivePCSCacheSegReqsSummaryRequest(WIZARD_12_PROTOCOL.MSG_PCS_CACHESEGREQSSUMMARY_REQUEST message) {
        SendToSocket(message);
    }
}
