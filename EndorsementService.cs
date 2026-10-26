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

internal sealed class EndorsementService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new EndorsementService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ENDORSEMENTREQUEST))]
    private void ReceiveEndorsementRequest(WIZARD3_56_PROTOCOL.MSG_ENDORSEMENTREQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_ENDORSEMENTACK {
            PlayerGID = message.PlayerGID,
            Facet = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ENDORSEMENTACK))]
    private void ReceiveEndorsementAck(WIZARD3_56_PROTOCOL.MSG_ENDORSEMENTACK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTENDORSEMENTSCORE))]
    private void ReceiveRequestEndorsementScore(WIZARD3_56_PROTOCOL.MSG_REQUESTENDORSEMENTSCORE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTENDORSEMENTSCORE {
            PlayerGID = wizard.GameObjectID,
            EndorsedGID = message.EndorsedGID,
            Facet = message.Facet,
            RequestType = message.RequestType
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEREALLOWENDORSEMENT))]
    private void ReceiveUpdateReallowEndorsement(WIZARD3_56_PROTOCOL.MSG_UPDATEREALLOWENDORSEMENT message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEREALLOWENDORSEMENT {
            AllowEndorsement = 1
        });
    }
}
