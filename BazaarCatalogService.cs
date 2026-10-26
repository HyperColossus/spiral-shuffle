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
using Imlight.CoreLib.WizardData.Collections;

namespace Imlight.CoreLib.Game.Services;

internal sealed class BazaarCatalogService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BazaarCatalogService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_AUCTIONHOUSEMOREACKNOWLEDGEMENT))]
    private void ReceiveAuctionHouseMoreAcknowledgement(WIZARD_12_PROTOCOL.MSG_AUCTIONHOUSEMOREACKNOWLEDGEMENT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        // Acknowledge bulk buying or selling items at the Bazaar
        if (message.IsSell != 0) {
            wizard.AddGold(message.TotalPrice);
        } else {
            wizard.RemoveGold(message.TotalPrice);
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold,
            MaxGold = wizard.GameStats.m_baseGoldPouch
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_AUCTIONRESPONSE {
            Command = message.IsSell != 0 ? (sbyte)2 : (sbyte)3,
            ItemTemplateID = message.ItemTemplateID,
            Cost = message.TotalPrice,
            ReturnCode = 0
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_AUCTIONHOUSECSRMESSAGE))]
    private void ReceiveAuctionHouseCSRMessage(WIZARD3_56_PROTOCOL.MSG_AUCTIONHOUSECSRMESSAGE message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_AUCTIONHOUSECSRMESSAGE {
            Buffer = message.Buffer
        });
    }
}
