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

internal sealed class AdvancedPetBreedingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdvancedPetBreedingService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSREQUESTPREMIUMPETEXEMPTIONS))]
    private void ReceiveRequestPremiumPetExemptions(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSREQUESTPREMIUMPETEXEMPTIONS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSREQUESTPREMIUMPETEXEMPTIONS {
            Buffer = message.Buffer,
            PremiumPetExceptionCode = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSUPDATEHATCHDAYPET2))]
    private void ReceiveUpdateHatchDayPet2(WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSUPDATEHATCHDAYPET2 message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_HATCHMAKINGSUPDATEHATCHDAYPET2 {
            PetTemplateID = message.PetTemplateID
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTINPERSONHATCHINGCHANGE))]
    private void ReceiveRequestInPersonHatchingChange(WIZARD3_56_PROTOCOL.MSG_REQUESTINPERSONHATCHINGCHANGE message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTINPERSONHATCHINGCHANGE {
            Status = message.Status
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_INPERSONHATCHINGCHECK))]
    private void ReceiveInPersonHatchingCheck(WIZARD3_56_PROTOCOL.MSG_INPERSONHATCHINGCHECK message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_INPERSONHATCHINGCHECK {
            Status = 0,
            OwnerGID = message.OwnerGID,
            PetGID = message.PetGID,
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            PackedName = message.PackedName,
            PetName = message.PetName
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PETTOMESCANATTIC))]
    private void ReceivePetTomeScanAttic(WIZARDHOUSING_50_PROTOCOL.MSG_PETTOMESCANATTIC message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PETTOMEPETADDED {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            PetTemplateID = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTPETTOME))]
    private void ReceiveRequestPetTome(WIZARD2_53_PROTOCOL.MSG_REQUESTPETTOME message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTPETTOME {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID,
            PetData = ByteString.Empty,
            Buffer = ByteString.Empty,
            PremiumPetExceptionCode = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICUSETOME))]
    private void ReceiveMonsterMagicUseTome(WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICUSETOME message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICUSETOME());
    }
}
