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

internal sealed class CrownServicesAndExpansionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CrownServicesAndExpansionService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_BUYENERGYCONFIRM))]
    private void ReceiveBuyEnergyConfirm(WIZARD_12_PROTOCOL.MSG_BUYENERGYCONFIRM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_BUYISLANDSPACECONFIRM))]
    private void ReceiveBuyIslandSpaceConfirm(WIZARDHOUSING_50_PROTOCOL.MSG_BUYISLANDSPACECONFIRM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CROWNSBUYCONFIRM))]
    private void ReceiveCrownsBuyConfirm(WIZARD_12_PROTOCOL.MSG_CROWNSBUYCONFIRM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CROWNSERVICESOPEN))]
    private void ReceiveCrownServicesOpen(WIZARD_12_PROTOCOL.MSG_CROWNSERVICESOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CSRCROWNBALANCE))]
    private void ReceiveCSRCrownBalance(WIZARD_12_PROTOCOL.MSG_CSRCROWNBALANCE message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CSRCROWNBALANCE {
            Success = 1,
            Balance = 999999,
            CharacterID = wizard?.CharId ?? message.CharacterID
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_ADCLICKTHROUGH))]
    private void ReceiveAdClickThrough(GAME_5_PROTOCOL.MSG_ADCLICKTHROUGH message) {
        SendToSocket(message);
    }
}
