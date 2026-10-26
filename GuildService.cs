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

internal sealed class GuildService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GuildService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_CREATEGUILDREQUEST))]
    private void ReceiveCreateGuildRequest(WIZARD3_56_PROTOCOL.MSG_CREATEGUILDREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_CREATEGUILDREQUEST {
            PlayerGID = wizard.GameObjectID,
            OwnerGID = wizard.GameObjectID,
            OwnerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            Status = 0,
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale
        });

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_GUILDINFO {
            PlayerGID = wizard.GameObjectID,
            OwnerGID = wizard.GameObjectID,
            OwnerName = wizard.PlayerNameBehavior.GetWizardNameAsByteHexString(),
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_VISITGUILDHOUSE))]
    private void ReceiveVisitGuildHouse(WIZARD3_56_PROTOCOL.MSG_VISITGUILDHOUSE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_LOADGUILDOBJECT {
            PlayerGID = wizard.GameObjectID,
            PartyGID = message.PartyGID > 0 ? message.PartyGID : 1,
            GuildObjectGID = message.GuildObjectGID,
            HousingTemplateID = 10001,
            GuildHouseFlags = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDINVENTORY))]
    private void ReceiveRequestGuildInventory(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDINVENTORY message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_ADDGUILDINVENTORYITEM {
            PartyGID = message.PartyGID,
            Buffer = "Azoth",
            IsTrash = 0
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDACHIEVEMENTS))]
    private void ReceiveRequestGuildAchievements(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDACHIEVEMENTS message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_INCREMENTGUILDACHIEVEMENT {
            GuildGID = message.PlayerGID,
            CategoryID = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDMUSEUMDOOR))]
    private void ReceiveRequestGuildMuseumDoor(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDMUSEUMDOOR message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_MOVEGUILDMUSEUMDOOR {
            DoorGID = 1,
            Status = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTMUSEUMCURATOR))]
    private void ReceiveRequestMuseumCurator(WIZARD3_56_PROTOCOL.MSG_REQUESTMUSEUMCURATOR message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDUNLOCKDATA))]
    private void ReceiveRequestGuildUnlockData(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDUNLOCKDATA message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDUNLOCKTRIGGER {
            PartyGID = 1,
            ReagentIndex = 0,
            Quantity = 10,
            TemplateID = 10001,
            State = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDUNLOCKREQUEST))]
    private void ReceiveGuildUnlockRequest(WIZARD3_56_PROTOCOL.MSG_GUILDUNLOCKREQUEST message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDUNLOCKTRIGGER {
            PartyGID = message.PartyGID,
            ReagentIndex = message.ReagentIndex,
            Quantity = message.Quantity,
            TemplateID = 10001,
            State = 1
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDOPENHOUSE))]
    private void ReceiveGuildOpenHouse(WIZARD3_56_PROTOCOL.MSG_GUILDOPENHOUSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDOPENHOUSEENDS))]
    private void ReceiveGuildOpenHouseEnds(WIZARD3_56_PROTOCOL.MSG_GUILDOPENHOUSEENDS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDMESSAGE))]
    private void ReceiveGuildMessage(WIZARD3_56_PROTOCOL.MSG_GUILDMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDVISITORREQUEST))]
    private void ReceiveGuildVisitorRequest(WIZARD3_56_PROTOCOL.MSG_GUILDVISITORREQUEST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDMEMBERSHIP))]
    private void ReceiveGuildMembership(WIZARD3_56_PROTOCOL.MSG_GUILDMEMBERSHIP message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GRANTGUILDBADGE))]
    private void ReceiveGrantGuildBadge(WIZARD3_56_PROTOCOL.MSG_GRANTGUILDBADGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_EMPTYGUILDTRASH))]
    private void ReceiveEmptyGuildTrash(WIZARD3_56_PROTOCOL.MSG_EMPTYGUILDTRASH message) {
        SendToSocket(message);
    }
}
