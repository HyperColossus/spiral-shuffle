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
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class GardeningService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new GardeningService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCASTSPELL))]
    private void ReceiveGardeningCastSpell(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCASTSPELL message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var petOwnerBehavior = wizard.PetOwnerBehavior;
        int currentEnergy = petOwnerBehavior?.Energy ?? 0;
        if (currentEnergy >= 2) {
            wizard.UpdateEnergy(currentEnergy - 2);
        }

        wizard.GameStats.m_gardeningXP += 25;

        int oldLevel = wizard.GameStats.m_gardeningLevel;
        byte newLevel = (byte) Math.Min(20, 1 + (wizard.GameStats.m_gardeningXP / 100));
        if (newLevel > oldLevel) {
            wizard.GameStats.m_gardeningLevel = newLevel;
            SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENLEVELUP {
                GlobalID = wizard.GameObjectID,
                NewLevel = newLevel
            });
        }

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEGARDENINGXP {
            GlobalID = wizard.GameObjectID,
            XP = wizard.GameStats.m_gardeningXP,
            Level = wizard.GameStats.m_gardeningLevel,
            IsPet = 0
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCOMMANDRESPONSE());
        Logger.Information("Gardening spell {0} cast at ({1}, {2}, {3}). XP: {4}, Level: {5}",
            Logger.Args(message.SpellTemplateID, message.PositionX, message.PositionY, message.PositionZ,
                wizard.GameStats.m_gardeningXP, wizard.GameStats.m_gardeningLevel));
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGHARVESTPLANT))]
    private void ReceiveHarvestPlant(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGHARVESTPLANT message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.AddGold(150);
        wizard.GameStats.m_gardeningXP += 50;

        int oldLevel = wizard.GameStats.m_gardeningLevel;
        byte newLevel = (byte) Math.Min(20, 1 + (wizard.GameStats.m_gardeningXP / 100));
        if (newLevel > oldLevel) {
            wizard.GameStats.m_gardeningLevel = newLevel;
            SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENLEVELUP {
                GlobalID = wizard.GameObjectID,
                NewLevel = newLevel
            });
        }

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_UPDATEGARDENINGXP {
            GlobalID = wizard.GameObjectID,
            XP = wizard.GameStats.m_gardeningXP,
            Level = wizard.GameStats.m_gardeningLevel,
            IsPet = 0
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCOMMANDRESPONSE());
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCOMMAND))]
    private void ReceiveGardeningCommand(WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCOMMAND message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_GARDENINGCOMMANDRESPONSE());
    }
}
