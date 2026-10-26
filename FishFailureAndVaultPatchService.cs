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

internal sealed class FishFailureAndVaultPatchService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FishFailureAndVaultPatchService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MISSFISH))]
    private void ReceiveMissFish(WIZARDHOUSING_50_PROTOCOL.MSG_MISSFISH message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_NOFISHSPACE))]
    private void ReceiveNoFishSpace(WIZARDHOUSING_50_PROTOCOL.MSG_NOFISHSPACE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHAQUARIUM))]
    private void ReceivePatchAquarium(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHAQUARIUM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTREASURECARDVAULT))]
    private void ReceivePatchTreasureCardVault(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTREASURECARDVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHMONSTERARENA))]
    private void ReceivePatchMonsterArena(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHMONSTERARENA message) {
        SendToSocket(message);
    }
}
