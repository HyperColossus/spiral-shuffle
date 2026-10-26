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
using Imcodec.MessageLayer.Generated;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class CastleGameService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleGameService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESREQUEST))]
    private void ReceiveCastleGamesRequest(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESREQUEST message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESREQUESTGAMEDATA {
            GlobalID = message.GlobalID,
            PageNumber = 1,
            Data = ""
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESREQUESTGAMEDATA))]
    private void ReceiveCastleGamesGameData(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESREQUESTGAMEDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_STARTHOUSINGGAME))]
    private void ReceiveStartHousingGame(WIZARDHOUSING_50_PROTOCOL.MSG_STARTHOUSINGGAME message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGGAMESTATUSUPDATE {
            Flags = 1
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGGAMEUSEPOWERUP))]
    private void ReceiveHousingGameUsePowerup(WIZARDHOUSING_50_PROTOCOL.MSG_HOUSINGGAMEUSEPOWERUP message) {
        SendToSocket(message);
    }
}
