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
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class HatchmakingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new HatchmakingService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKING))]
    private void ReceiveRequestHatchmaking(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKING message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKING {
            GlobalID = wizard.GameObjectID,
            PetData = ByteString.Empty,
            CrownCost = 0f,
            HatchDayCode = message.HatchDayCode,
            NoInPersonHatching = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETS))]
    private void ReceiveRequestHatchmakingPets(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGPETS {
            PetData = ByteString.Empty,
            PetTemplateID = message.PetTemplateID,
            Skip = message.Skip
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGSEARCHPETS))]
    private void ReceiveRequestHatchmakingSearchPets(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGSEARCHPETS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGSEARCHPETS {
            GlobalID = wizard.GameObjectID,
            Data = ByteString.Empty,
            SearchType = message.SearchType,
            InPerson = message.InPerson
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGADDPET))]
    private void ReceiveRequestHatchmakingAddPet(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGADDPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGREMOVEPET))]
    private void ReceiveRequestHatchmakingRemovePet(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGREMOVEPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCH2))]
    private void ReceiveRequestHatchmakingHatch2(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCH2 message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCH2 {
            PetID1 = message.PetID1,
            PetID2 = message.PetID2,
            PetIndex = message.PetIndex
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGUSETOME))]
    private void ReceiveHatchmakingUseTome(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGUSETOME message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSUPDATEHATCHDAYPET))]
    private void ReceiveHatchmakingsUpdateHatchDayPet(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSUPDATEHATCHDAYPET message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTDONATEDPETLIST))]
    private void ReceiveRequestDonatedPetList(WIZARD3_56_PROTOCOL.MSG_REQUESTDONATEDPETLIST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTDONATEDPETLIST {
            PlayerGID = wizard.GameObjectID,
            Buffer = ByteString.Empty,
            IsDonate = message.IsDonate
        });
    }
}
