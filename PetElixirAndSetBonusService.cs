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

internal sealed class PetElixirAndSetBonusService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetElixirAndSetBonusService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETELIXIRBUYREQUEST))]
    private void ReceivePetElixirBuyRequest(WIZARD2_53_PROTOCOL.MSG_PETELIXIRBUYREQUEST message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BUYPETELIXIRCONFIRM {
            Credits = message.Credits,
            Failure = 0,
            WebFailure = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETELIXIROPEN))]
    private void ReceivePetElixirOpen(WIZARD2_53_PROTOCOL.MSG_PETELIXIROPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_NEWTITLE))]
    private void ReceiveNewTitle(WIZARD_12_PROTOCOL.MSG_NEWTITLE message) {
        var wizard = GetActiveWizard();
        if (wizard is not null && message.Title.Length > 0) {
            wizard.SetBadgeOverride(message.Title.ToString());
        }
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME2_55_PROTOCOL.MSG_NOTIFYITEMFORSETBONUS))]
    private void ReceiveNotifyItemForSetBonus(GAME2_55_PROTOCOL.MSG_NOTIFYITEMFORSETBONUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_NOTIFYSCHOOLFOCUS))]
    private void ReceiveNotifySchoolFocus(WIZARD_12_PROTOCOL.MSG_NOTIFYSCHOOLFOCUS message) {
        SendToSocket(message);
    }
}
