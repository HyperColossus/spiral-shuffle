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

internal sealed class ItemStackAndStorageSyncService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new ItemStackAndStorageSyncService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_COMBINEINVENTORYITEMS))]
    private void ReceiveCombineInventoryItems(GAME_5_PROTOCOL.MSG_COMBINEINVENTORYITEMS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REMOVETREASURESPELLFROMVAULT))]
    private void ReceiveRemoveTreasureSpellFromVault(WIZARD_12_PROTOCOL.MSG_REMOVETREASURESPELLFROMVAULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_STORAGECLIENTREMOVE))]
    private void ReceiveStorageClientRemove(WIZARD_12_PROTOCOL.MSG_STORAGECLIENTREMOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_WIZINVENTORYCLIENTREMOVE))]
    private void ReceiveWizInventoryClientRemove(WIZARD_12_PROTOCOL.MSG_WIZINVENTORYCLIENTREMOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_ADDPARTICLE))]
    private void ReceiveAddParticle(GAME_5_PROTOCOL.MSG_ADDPARTICLE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REMOVEPARTICLE))]
    private void ReceiveRemoveParticle(GAME_5_PROTOCOL.MSG_REMOVEPARTICLE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REMOVEDSHOPPER))]
    private void ReceiveRemovedShopper(GAME_5_PROTOCOL.MSG_REMOVEDSHOPPER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_RECIPEREMOVE))]
    private void ReceiveRecipeRemove(WIZARD_12_PROTOCOL.MSG_RECIPEREMOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_DERBYEFFECTREMOVE))]
    private void ReceiveDerbyEffectRemove(PET_9_PROTOCOL.MSG_DERBYEFFECTREMOVE message) {
        SendToSocket(message);
    }
}
