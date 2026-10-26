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

internal sealed class PvPCurrencyAndStatueService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PvPCurrencyAndStatueService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEPVPCURRENCY))]
    private void ReceiveUpdatePvPCurrency(WIZARD3_56_PROTOCOL.MSG_UPDATEPVPCURRENCY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEPVPTOURNEYCURRENCY))]
    private void ReceiveUpdatePvPTourneyCurrency(WIZARD3_56_PROTOCOL.MSG_UPDATEPVPTOURNEYCURRENCY message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PvPStatueData))]
    private void ReceivePvPStatueData(WIZARD3_56_PROTOCOL.MSG_PvPStatueData message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPNOMATCHIGNORELIST))]
    private void ReceivePvPNoMatchIgnoreList(WIZARD3_56_PROTOCOL.MSG_PVPNOMATCHIGNORELIST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPBLOCKED))]
    private void ReceivePvPBlocked(WIZARD3_56_PROTOCOL.MSG_PVPBLOCKED message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PVPBLOCKED());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PVPDISABLED))]
    private void ReceivePvPDisabled(WIZARD3_56_PROTOCOL.MSG_PVPDISABLED message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PVPDISABLED());
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_RESPONSESHOWPVPOPTION))]
    private void ReceiveResponseShowPvPOption(WIZARD3_56_PROTOCOL.MSG_RESPONSESHOWPVPOPTION message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_RESPONSESHOWPVPOPTION {
            ShowPVPOption = message.ShowPVPOption
        });
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_PVPBLOCK))]
    private void ReceivePvPBlock(GAME2_55_PROTOCOL.MSG_PVPBLOCK message) {
        SendToSocket(new GAME2_55_PROTOCOL.MSG_PVPBLOCK_RSP {
            PvPBlockID = ByteString.Empty,
            PvPBlockTime = message.PvPBlockTime,
            Success = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PVPREGISTERFAILED))]
    private void ReceivePvPRegisterFailed(WIZARD_12_PROTOCOL.MSG_PVPREGISTERFAILED message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PVPREGISTERFAILED());
    }
}
