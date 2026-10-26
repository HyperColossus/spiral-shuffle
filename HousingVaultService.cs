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

internal sealed class HousingVaultService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingVaultService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEGEARTOGEARVAULT))]
    private void ReceiveMoveGearToGearVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEGEARTOGEARVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEGEARFROMGEARVAULT))]
    private void ReceiveMoveGearFromGearVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEGEARFROMGEARVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEJEWELTOJEWELVAULT))]
    private void ReceiveMoveJewelToJewelVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEJEWELTOJEWELVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEJEWELFROMJEWELVAULT))]
    private void ReceiveMoveJewelFromJewelVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVEJEWELFROMJEWELVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVESEEDTOGARDENINGSHED))]
    private void ReceiveMoveSeedToGardeningShed(WIZARDHOUSING_50_PROTOCOL.MSG_MOVESEEDTOGARDENINGSHED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVESEEDFROMGARDENINGSHED))]
    private void ReceiveMoveSeedFromGardeningShed(WIZARDHOUSING_50_PROTOCOL.MSG_MOVESEEDFROMGARDENINGSHED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETCTOTCVAULT))]
    private void ReceiveMoveTcToTcVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETCTOTCVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETCFROMTCVAULT))]
    private void ReceiveMoveTcFromTcVault(WIZARDHOUSING_50_PROTOCOL.MSG_MOVETCFROMTCVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEFROMATTIC))]
    private void ReceiveDeleteFromAttic(WIZARDHOUSING_50_PROTOCOL.MSG_DELETEFROMATTIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHADDATTIC))]
    private void ReceivePatchAddAttic(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHADDATTIC message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHDELETEATTIC))]
    private void ReceivePatchDeleteAttic(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHDELETEATTIC message) {
        SendToSocket(message);
    }
}
