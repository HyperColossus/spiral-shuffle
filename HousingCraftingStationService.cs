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

internal sealed class HousingCraftingStationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HousingCraftingStationService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ALCHEMYSTATION))]
    private void ReceiveAlchemyStation(WIZARD_12_PROTOCOL.MSG_ALCHEMYSTATION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_EQUIPRECIPE))]
    private void ReceiveEquipRecipe(WIZARD2_53_PROTOCOL.MSG_EQUIPRECIPE message) {
        SendToSocket(message);
    }
}
