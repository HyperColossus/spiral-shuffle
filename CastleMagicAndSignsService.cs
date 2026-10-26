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

internal sealed class CastleMagicAndSignsService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleMagicAndSignsService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTPVPSTATE))]
    private void ReceiveCastleMagicRequestPvPState(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTPVPSTATE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTPVPSTATE {
            State = message.State,
            ModifierName = message.ModifierName
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTRANDOM))]
    private void ReceiveCastleMagicRequestRandom(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTRANDOM message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEMAGICREQUESTRANDOM {
            Buffer = message.Buffer
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSESIGN))]
    private void ReceiveSetHouseSign(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSESIGN message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSESIGN {
            ObjectID = message.ObjectID,
            SignText = message.SignText,
            NameText = message.NameText
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_SETHOUSECUSTOMIZATION))]
    private void ReceiveSetHouseCustomization(GAME_5_PROTOCOL.MSG_SETHOUSECUSTOMIZATION message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_SETHOUSECUSTOMIZATION {
            GameObjectID = message.GameObjectID,
            TagName = message.TagName,
            TextureFilename = message.TextureFilename
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSINGPALETTE))]
    private void ReceiveSetHousingPalette(WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSINGPALETTE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SETHOUSINGPALETTE {
            ObjectGID = message.ObjectGID,
            PrimaryColorIndex = message.PrimaryColorIndex
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_SETTILEWALLPAPER))]
    private void ReceiveSetTileWallpaper(WIZARDHOUSING_50_PROTOCOL.MSG_SETTILEWALLPAPER message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SETTILEWALLPAPER {
            ObjectID = message.ObjectID,
            RemapIndex = message.RemapIndex,
            IsWallpaper = message.IsWallpaper
        });
    }
}
