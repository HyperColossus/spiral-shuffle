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

internal sealed class JewelSocketAndEquipmentService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new JewelSocketAndEquipmentService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_EQUIPNEWJEWELTOITEM))]
    private void ReceiveEquipNewJewelToItem(WIZARD2_53_PROTOCOL.MSG_EQUIPNEWJEWELTOITEM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_EQUIPMENTBEHAVIOR_UNEQUIPITEM))]
    private void ReceiveEquipmentBehaviorUnequipItem(GAME_5_PROTOCOL.MSG_EQUIPMENTBEHAVIOR_UNEQUIPITEM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_GETITEMINFO))]
    private void ReceiveGetItemInfo(WIZARD2_53_PROTOCOL.MSG_GETITEMINFO message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_GETSUBSCRIBERONLYITEMS))]
    private void ReceiveGetSubscriberOnlyItems(WIZARD_12_PROTOCOL.MSG_GETSUBSCRIBERONLYITEMS message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_GETSUBSCRIBERONLYITEMS());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HOLIDAYDATA))]
    private void ReceiveHolidayData(WIZARD2_53_PROTOCOL.MSG_HOLIDAYDATA message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(QUEST_MESSAGES_52_PROTOCOL.MSG_GETNEXTWORLD))]
    private void ReceiveGetNextWorld(QUEST_MESSAGES_52_PROTOCOL.MSG_GETNEXTWORLD message) {
        SendToSocket(message);
    }
}
