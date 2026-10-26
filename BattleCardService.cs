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

internal sealed class BattleCardService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BattleCardService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLECARDBUY))]
    private void ReceiveBattleCardBuy(WIZARD2_53_PROTOCOL.MSG_BATTLECARDBUY message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BATTLECARDBUYCONFIRM {
            Failure = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLECARDSHOPLIST))]
    private void ReceiveBattleCardShopList(WIZARD2_53_PROTOCOL.MSG_BATTLECARDSHOPLIST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGSELECTPOLYMORPHREQUEST))]
    private void ReceiveSelectPolymorphRequest(WIZARD2_53_PROTOCOL.MSG_BGSELECTPOLYMORPHREQUEST message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGSELECTPOLYMORPHRESPONSE {
            PolymorphCategory = "Beastmoon",
            PolymorphCategoryAvailable = 1,
            ReleasedPolymorphCategory = ""
        });
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGSELECTPOLYMORPHDONE());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGISPOLYMORPHUNLOCKEDREQUEST))]
    private void ReceiveIsPolymorphUnlocked(WIZARD2_53_PROTOCOL.MSG_BGISPOLYMORPHUNLOCKEDREQUEST message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BGISPOLYMORPHUNLOCKEDRESPONSE {
            PolymorphTemplateID = message.PolymorphTemplateID,
            IsPolymorphUnlocked = 1,
            PolymorphRating = 100
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BGPlayerStatsUpdate))]
    private void ReceiveBGPlayerStatsUpdate(WIZARD2_53_PROTOCOL.MSG_BGPlayerStatsUpdate message) {
        SendToSocket(message);
    }
}
