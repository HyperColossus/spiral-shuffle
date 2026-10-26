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

internal sealed class JewelUnlockAndTradeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new JewelUnlockAndTradeService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETUNLOCKCOST))]
    private void ReceiveJewelSocketUnlockCost(WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETUNLOCKCOST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_ISLANDSPACESHOPOPEN))]
    private void ReceiveIslandSpaceShopOpen(WIZARDHOUSING_50_PROTOCOL.MSG_ISLANDSPACESHOPOPEN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_TRADEITEM))]
    private void ReceiveInventoryBehaviorTradeItem(GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_TRADEITEM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ITEMIDENTIFIERINFO))]
    private void ReceiveItemIdentifierInfo(WIZARD2_53_PROTOCOL.MSG_ITEMIDENTIFIERINFO message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ITEMLOCK))]
    private void ReceiveItemLock(WIZARD_12_PROTOCOL.MSG_ITEMLOCK message) {
        SendToSocket(message);
    }
}
