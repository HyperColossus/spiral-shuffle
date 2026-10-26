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

internal sealed class GardeningLifecycleService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GardeningLifecycleService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGHARVESTPLANTSECONDSPRING))]
    private void ReceiveHarvestPlantSecondSpring(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGHARVESTPLANTSECONDSPRING message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGHARVESTPLANTSECONDSPRING {
            PositionX = message.PositionX,
            PositionY = message.PositionY,
            PositionZ = message.PositionZ
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGSPELLINSPECT))]
    private void ReceiveGardeningSpellInspect(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGSPELLINSPECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGSPELLINSPECT {
            PositionX = message.PositionX,
            PositionY = message.PositionY,
            PositionZ = message.PositionZ,
            Progress = message.Progress
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGPELLFIZZLE))]
    private void ReceiveGardeningPellFizzle(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGPELLFIZZLE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGPELLFIZZLE {
            FizzleType = message.FizzleType
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHGARDENING))]
    private void ReceivePatchGardening(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHGARDENING message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHGARDENING {
            Command = message.Command,
            PositionX = message.PositionX,
            PositionY = message.PositionY,
            PositionZ = message.PositionZ,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID,
            TemplateID = message.TemplateID,
            Time = message.Time
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCSRRESULTS))]
    private void ReceiveGardeningCSRResults(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCSRRESULTS message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCSRRESULTS {
            Result = message.Result
        });
    }
}
