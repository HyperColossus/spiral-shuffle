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

internal sealed class MagicMirrorService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new MagicMirrorService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_MAGICMIRRORSHOPOPEN))]
    private void ReceiveMagicMirrorShopOpen(WIZARD2_53_PROTOCOL.MSG_MAGICMIRRORSHOPOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_MAGICMIRRORREQUESTCHANGE))]
    private void ReceiveMagicMirrorRequestChange(WIZARD2_53_PROTOCOL.MSG_MAGICMIRRORREQUESTCHANGE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_MAGICMIRRORCHANGEPLAYER {
            GlobalID = wizard.GameObjectID,
            HairStyleIndex = (int) message.HairStyleID,
            HairColorIndex = (int) message.HairColorID,
            SkinColorIndex = (int) message.SkinColorID,
            FaceStyleIndex = (int) message.FaceStyleID,
            FacePaintIndex = message.FacePaintID,
            EyeColorIndex = (int) message.EyeColorID,
            NoseStyleIndex = (int) message.NoseStyleID,
            MouthStyleIndex = (int) message.MouthStyleID,
            AccessoryStyleIndex = (int) message.AccessoryStyleID,
            EyeBrowColorIndex = (int) message.EyeBrowColorID,
            LipColorIndex = (int) message.LipColorID
        });
    }
}
