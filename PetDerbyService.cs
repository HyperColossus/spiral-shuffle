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

internal sealed class PetDerbyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetDerbyService(parentActor));

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYSTART))]
    private void ReceivePetDerbyStart(PET_9_PROTOCOL.MSG_PETDERBYSTART message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETDERBYSTART { GoGoGo = 1 });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYSWITCHLANE))]
    private void ReceivePetDerbySwitchLane(PET_9_PROTOCOL.MSG_PETDERBYSWITCHLANE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYJUMPDUCK))]
    private void ReceivePetDerbyJumpDuck(PET_9_PROTOCOL.MSG_PETDERBYJUMPDUCK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYCHEER))]
    private void ReceivePetDerbyCheer(PET_9_PROTOCOL.MSG_PETDERBYCHEER message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETDERBYSPEED {
            PetIndex = message.PetIndex,
            Speed = 100
        });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYUSETALENT))]
    private void ReceivePetDerbyUseTalent(PET_9_PROTOCOL.MSG_PETDERBYUSETALENT message) {
        SendToSocket(new PET_9_PROTOCOL.MSG_PETDERBYBUFF {
            Add = 1,
            Type = 1,
            PetID = 0
        });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETDERBYLAP))]
    private void ReceivePetDerbyLap(PET_9_PROTOCOL.MSG_PETDERBYLAP message) {
        SendToSocket(message);
        if (message.Count >= 3) {
            SendToSocket(new PET_9_PROTOCOL.MSG_PETDERBYPHOTOFINISH {
                Rank = 1,
                PetIndex = message.PetIndex
            });
            SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMEDERBYRESULTS {
                Data = "Winner"
            });
        }
    }
}
