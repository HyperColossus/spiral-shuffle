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

internal sealed class FishingService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new FishingService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_BEGINFISHINGCAST))]
    private void ReceiveBeginFishingCast(WIZARDHOUSING_50_PROTOCOL.MSG_BEGINFISHINGCAST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SHOWFISHINGCAST {
            ObjectGID = wizard.GameObjectID,
            PositionX = wizard.Location.X,
            PositionY = wizard.Location.Y,
            PositionZ = wizard.Location.Z,
            Yaw = wizard.Orientation.Z,
            SpellTemplateID = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_FISHINGSPELLCAST))]
    private void ReceiveFishingSpellCast(WIZARDHOUSING_50_PROTOCOL.MSG_FISHINGSPELLCAST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var petOwnerBehavior = wizard.PetOwnerBehavior;
        int currentEnergy = petOwnerBehavior?.Energy ?? 0;
        if (currentEnergy >= 3) {
            wizard.UpdateEnergy(currentEnergy - 3);
        }

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_SHOWFISHINGCAST {
            ObjectGID = wizard.GameObjectID,
            PositionX = wizard.Location.X,
            PositionY = wizard.Location.Y,
            PositionZ = wizard.Location.Z,
            Yaw = wizard.Orientation.Z,
            SpellTemplateID = message.SpellTemplateID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CATCHFISH))]
    private void ReceiveCatchFish(WIZARDHOUSING_50_PROTOCOL.MSG_CATCHFISH message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        uint fishTemplate = message.FishID != 0 ? message.FishID : 1001;
        float fishLength = 22.5f;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DISPLAYCATCHFISH {
            ObjectGID = wizard.GameObjectID,
            FishTemplateID = fishTemplate,
            Size = fishLength,
            SpellTemplateID = message.SpellTemplateID
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_DISPLAYCAUGHTFISH {
            ObjectGID = wizard.GameObjectID,
            FishTemplateID = fishTemplate,
            Length = fishLength,
            IsSmallFry = 0,
            IsWopper = 0,
            Energy = 0
        });

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CATCHSUCCESS {
            FishID = fishTemplate,
            Assist = 0
        });

        wizard.AddGold(100);
        wizard.GameStats.m_fishingXP += 50;

        int oldLevel = wizard.GameStats.m_fishingLevel;
        byte newLevel = (byte) Math.Min(20, 1 + (wizard.GameStats.m_fishingXP / 100));
        if (newLevel > oldLevel) {
            wizard.GameStats.m_fishingLevel = newLevel;
            SendToSocket(new WIZARD2_53_PROTOCOL.MSG_FISHINGLEVELUP {
                GlobalID = wizard.GameObjectID,
                NewLevel = newLevel
            });
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEFISHINGXP {
            GlobalID = wizard.GameObjectID,
            XP = wizard.GameStats.m_fishingXP,
            Level = wizard.GameStats.m_fishingLevel
        });

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });
    }
}
