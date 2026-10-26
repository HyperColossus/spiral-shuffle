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

internal sealed class HousingInteractiveNetworkService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingInteractiveNetworkService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGSIGNOBJECT))]
    private void ReceivePatchHousingSignObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGSIGNOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGSIGNOBJECT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTELEPORTEROBJECT))]
    private void ReceivePatchHousingTeleporterObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTELEPORTEROBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTELEPORTEROBJECT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGCONFIRM))]
    private void ReceiveRequestTeleporterHousingConfirm(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGCONFIRM message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTTELEPORTERHOUSINGCONFIRM {
            GlobalID = message.GlobalID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGMUSICOBJECT))]
    private void ReceivePatchHousingMusicObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGMUSICOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGMUSICOBJECT {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHMUSICLOOP))]
    private void ReceivePatchMusicLoop(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHMUSICLOOP message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHMUSICLOOP {
            SubType = message.SubType,
            MusicData = message.MusicData,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID,
            ObjectID = message.ObjectID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SENDHOUSINGOBJECTSTATES))]
    private void ReceiveSendHousingObjectStates(WIZARDHOUSING_50_PROTOCOL.MSG_SENDHOUSINGOBJECTSTATES message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SENDHOUSINGOBJECTSTATES {
            Data = message.Data
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTRAIN))]
    private void ReceivePatchHousingTrain(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTRAIN message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSINGTRAIN {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLACEHOUSINGFISH))]
    private void ReceivePlaceHousingFish(WIZARDHOUSING_50_PROTOCOL.MSG_PLACEHOUSINGFISH message) {
        SendToSocket(message);
    }
}
