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

internal sealed class HousingItemFinderService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingItemFinderService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERHOUSECOMPLETE))]
    private void ReceiveItemFinderHouseComplete(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERHOUSECOMPLETE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ISFRIENDSWITHHOUSEOWNER))]
    private void ReceiveIsFriendsWithHouseOwner(WIZARD2_53_PROTOCOL.MSG_ISFRIENDSWITHHOUSEOWNER message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ISFRIENDSWITHHOUSEOWNER {
            OriginatorID = message.OriginatorID,
            HouseOwnerID = message.HouseOwnerID,
            IsFriend = 1
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSEOWNERCHARACTERID))]
    private void ReceiveRequestHouseOwnerCharacterID(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSEOWNERCHARACTERID message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSEOWNERCHARACTERID {
            CharacterID = wizard.CharId,
            DeedTemplateID = message.DeedTemplateID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSETRANSFERLIST))]
    private void ReceiveRequestHouseTransferList(WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSETRANSFERLIST message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_REQUESTHOUSETRANSFERLIST {
            Data = ByteString.Empty
        });
    }
}
