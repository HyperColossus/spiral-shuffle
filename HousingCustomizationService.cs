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

internal sealed class HousingCustomizationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingCustomizationService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTEXTUREREMAP))]
    private void ReceivePatchTextureRemap(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTEXTUREREMAP message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTEXTUREREMAP {
            Data = message.Data,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPDATEHOUSINGOBJECT))]
    private void ReceivePatchUpdateHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPDATEHOUSINGOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPDATEHOUSINGOBJECT {
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ,
            Yaw = message.Yaw,
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID,
            ObjectID = message.ObjectID,
            SwitchTemplateID = message.SwitchTemplateID,
            UseExtendedYaw = message.UseExtendedYaw,
            Scale = message.Scale,
            Brightness = message.Brightness
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSE))]
    private void ReceivePatchHouse(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHHOUSE {
            DeedTemplateID = message.DeedTemplateID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_AUDITATTICRESULTS))]
    private void ReceiveAuditAtticResults(WIZARDHOUSING_50_PROTOCOL.MSG_AUDITATTICRESULTS message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_AUDITATTICRESULTS {
            ItemsAudited = message.ItemsAudited,
            ItemsRemoved = message.ItemsRemoved
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETATTICID))]
    private void ReceiveSetAtticID(WIZARDHOUSING_50_PROTOCOL.MSG_SETATTICID message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SETATTICID {
            GlobalID = message.GlobalID,
            AtticID = message.AtticID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTINTHOUSINGOBJECT))]
    private void ReceivePatchTintHousingObject(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTINTHOUSINGOBJECT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHTINTHOUSINGOBJECT {
            SubType = message.SubType,
            ColorBits = message.ColorBits,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID,
            ObjectID = message.ObjectID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPEXTENDEDTILE))]
    private void ReceivePatchUpExtendedTile(WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPEXTENDEDTILE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_PATCHUPEXTENDEDTILE {
            SubType = message.SubType,
            PackageNumber = message.PackageNumber,
            VersionNumber = message.VersionNumber,
            GIDID = message.GIDID,
            RemapIndex = message.RemapIndex,
            IsWallpaper = message.IsWallpaper,
            ObjectID = message.ObjectID
        });
    }
}
