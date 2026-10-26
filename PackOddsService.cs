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

internal sealed class PackOddsService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new PackOddsService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTPACKODDS))]
    private void ReceiveRequestPackOdds(WIZARD3_56_PROTOCOL.MSG_REQUESTPACKODDS message) {
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTPACKODDS {
            SourceID = message.SourceID,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CREATEBOOSTERDISTRIBUTION))]
    private void ReceiveCreateBoosterDistribution(WIZARD_12_PROTOCOL.MSG_CREATEBOOSTERDISTRIBUTION message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_CREATEBOOSTERDISTRIBUTION {
            BoosterTemplateID = message.BoosterTemplateID,
            LootTableName = message.LootTableName,
            PlayerLevel = (uint) wizard.MagicSchoolBehavior.Level,
            PlayerSchool = wizard.MagicSchoolBehavior.MagicSchool.ToString(),
            PlayerGender = wizard.PlayerNameBehavior.Gender.ToString(),
            Samples = message.Samples
        });
    }
}
