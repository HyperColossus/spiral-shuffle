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

internal sealed class GuildMuseumService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GuildMuseumService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTMUSEUMESSENCEDATA))]
    private void ReceiveRequestMuseumEssenceData(WIZARD3_56_PROTOCOL.MSG_REQUESTMUSEUMESSENCEDATA message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTMUSEUMESSENCEDATA {
            PartyGID = message.PartyGID,
            Status = 0,
            Buffer = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEMUSEUMESSENCEDATA))]
    private void ReceiveUpdateMuseumEssenceData(WIZARD3_56_PROTOCOL.MSG_UPDATEMUSEUMESSENCEDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEMUSEUMEXHIBIT))]
    private void ReceiveUpdateMuseumExhibit(WIZARD3_56_PROTOCOL.MSG_UPDATEMUSEUMEXHIBIT message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEMUSEUMEXHIBIT {
            WorldID = message.WorldID,
            ExhibitID = message.ExhibitID,
            State = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_MOVEGUILDMUSEUMDOOR))]
    private void ReceiveMoveGuildMuseumDoor(WIZARD3_56_PROTOCOL.MSG_MOVEGUILDMUSEUMDOOR message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CREATEGUILDTESTOBJECT))]
    private void ReceiveCreateGuildTestObject(WIZARD3_56_PROTOCOL.MSG_CREATEGUILDTESTOBJECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CREATETESTISLAND))]
    private void ReceiveCreateTestIsland(WIZARDHOUSING_50_PROTOCOL.MSG_CREATETESTISLAND message) {
        SendToSocket(message);
    }
}
