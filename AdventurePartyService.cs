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

internal sealed class AdventurePartyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdventurePartyService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTCREATEADVENTUREPARTY))]
    private void ReceiveCreateAdventureParty(WIZARD_12_PROTOCOL.MSG_REQUESTCREATEADVENTUREPARTY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_ADVENTUREPARTYMESSAGE {
            PlayerGID = wizard.GameObjectID,
            PartyGID = 101,
            Status = 0,
            PlayerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTJOINADVENTUREPARTY))]
    private void ReceiveJoinAdventureParty(WIZARD_12_PROTOCOL.MSG_REQUESTJOINADVENTUREPARTY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_ADVENTUREPARTYMESSAGE {
            PlayerGID = wizard.GameObjectID,
            PartyGID = message.PartyGID,
            Status = 0,
            PlayerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_USEPETADVENTUREPOWER))]
    private void ReceiveUsePetAdventurePower(WIZARD2_53_PROTOCOL.MSG_USEPETADVENTUREPOWER message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEADVENTUREPOWERCOOLDOWN {
            AdventurePowerCooldownTime = 300
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTSETPURPOSE))]
    private void ReceiveSetPurpose(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTSETPURPOSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTYADDTOKIOSK))]
    private void ReceiveAddToKiosk(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTYADDTOKIOSK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTYASKTOJOIN))]
    private void ReceiveAskToJoin(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTYASKTOJOIN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CHANGEPARTYNAME))]
    private void ReceiveChangePartyName(WIZARD3_56_PROTOCOL.MSG_CHANGEPARTYNAME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CHANGEPARTYOWNER))]
    private void ReceiveChangePartyOwner(WIZARD3_56_PROTOCOL.MSG_CHANGEPARTYOWNER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTLEAVEADVENTUREPARTY))]
    private void ReceiveRequestLeaveAdventureParty(WIZARD_12_PROTOCOL.MSG_REQUESTLEAVEADVENTUREPARTY message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_LEAVEADVENTUREPARTY {
            PlayerGID = wizard.GameObjectID,
            PlayerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale,
            Status = 0,
            Guild = message.Guild
        });
    }
}
