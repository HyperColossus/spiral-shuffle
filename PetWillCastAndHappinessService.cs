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

internal sealed class PetWillCastAndHappinessService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PetWillCastAndHappinessService(parentActor));

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_PETWILLCAST))]
    private void ReceivePetWillCast(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_PETWILLCAST message) {
        // Player commands pet to cast their equipped will-cast talent during combat
        SendToSocket(new DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_PETWILLCAST {
            PetCastingSpell = message.PetCastingSpell,
            Target = message.Target
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETFEEDHAPPINESS))]
    private void ReceivePetFeedHappiness(WIZARD2_53_PROTOCOL.MSG_PETFEEDHAPPINESS message) {
        // Feeding pet snack to replenish Pet Happiness
        var wizard = GetActiveWizard();
        uint newHappiness = Math.Min(100u, message.Happiness + 15u);

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PETFEEDHAPPINESS {
            SnackTemplateID = message.SnackTemplateID,
            FeedStack = message.FeedStack,
            Happiness = newHappiness
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_UPDATEOVERFLOWXP))]
    private void ReceiveUpdateOverflowXP(WIZARD3_56_PROTOCOL.MSG_UPDATEOVERFLOWXP message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_UPDATEOVERFLOWXP {
            OverflowXP = message.OverflowXP
        });
    }
}
