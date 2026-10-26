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

internal sealed class GuildIslandPermissionService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GuildIslandPermissionService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_GUILDITEMCONTROL))]
    private void ReceiveGuildItemControl(WIZARD3_56_PROTOCOL.MSG_GUILDITEMCONTROL message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_GUILDITEMCONTROL {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PartyGID = message.PartyGID,
            ItemGID = message.ItemGID,
            IsTrash = message.IsTrash
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDHOUSEFLAGS))]
    private void ReceiveUpdateGuildHouseFlags(WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDHOUSEFLAGS message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDHOUSEFLAGS {
            GuildGID = message.GuildGID,
            Flags = message.Flags
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_PLAYERINGUILDCOOLDOWN))]
    private void ReceivePlayerInGuildCooldown(WIZARD3_56_PROTOCOL.MSG_PLAYERINGUILDCOOLDOWN message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_PLAYERINGUILDCOOLDOWN {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PlayerName = message.PlayerName,
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale,
            CooldownTime = message.CooldownTime,
            Status = message.Status,
            RequestingPlayerGID = wizard?.GameObjectID ?? message.RequestingPlayerGID
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDNAME))]
    private void ReceiveUpdateGuildName(WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDNAME message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEGUILDNAME {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PartyName = message.PartyName,
            PartyNameLocale = message.PartyNameLocale
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REMOVEGUILDINVENTORYITEM))]
    private void ReceiveRemoveGuildInventoryItem(WIZARD3_56_PROTOCOL.MSG_REMOVEGUILDINVENTORYITEM message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REMOVEGUILDINVENTORYITEM {
            PartyGID = message.PartyGID,
            ItemGID = message.ItemGID,
            IsTrash = message.IsTrash
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDOBJECT))]
    private void ReceiveRequestGuildObject(WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDOBJECT message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTGUILDOBJECT {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PartyGID = message.PartyGID,
            GuildObjectGID = message.GuildObjectGID,
            HousingInstanceGID = message.HousingInstanceGID,
            InteriorInstanceGID = message.InteriorInstanceGID,
            HousingTemplateID = message.HousingTemplateID,
            InteriorTemplateID = message.InteriorTemplateID
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UNLOADGUILDOBJECT))]
    private void ReceiveUnloadGuildObject(WIZARD3_56_PROTOCOL.MSG_UNLOADGUILDOBJECT message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UNLOADGUILDOBJECT {
            PartyGID = message.PartyGID
        });
    }
}
