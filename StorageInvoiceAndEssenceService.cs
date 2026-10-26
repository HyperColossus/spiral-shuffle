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

internal sealed class StorageInvoiceAndEssenceService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new StorageInvoiceAndEssenceService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_DELIVERYINVOICETRANSFEROBJECTSFROMSTORAGETOBANK))]
    private void ReceiveDeliveryInvoiceTransferObjectsFromStorageToBank(WIZARD_12_PROTOCOL.MSG_DELIVERYINVOICETRANSFEROBJECTSFROMSTORAGETOBANK message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_DELIVERYINVOICETRANSFEROBJECTSFROMSTORAGETOBANK());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DELETESETSWITHITEM))]
    private void ReceiveDeleteSetsWithItem(WIZARD2_53_PROTOCOL.MSG_DELETESETSWITHITEM message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DISPLAYCOLLECTEDESSENCES))]
    private void ReceiveDisplayCollectedEssences(WIZARD2_53_PROTOCOL.MSG_DISPLAYCOLLECTEDESSENCES message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_DISPLAYEQUIPMENTSLOWDOWN))]
    private void ReceiveDisplayEquipmentSlowdown(WIZARD3_56_PROTOCOL.MSG_DISPLAYEQUIPMENTSLOWDOWN message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_DISPLAYEQUIPMENTSLOWDOWN());
    }
}
