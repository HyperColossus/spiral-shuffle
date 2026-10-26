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
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class MonstrologyService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new MonstrologyService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTMONSTERTOME))]
    private void ReceiveRequestMonsterTome(WIZARD2_53_PROTOCOL.MSG_REQUESTMONSTERTOME message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTMONSTERTOME {
            GlobalID = wizard.GameObjectID,
            MonsterData = ByteString.Empty
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEMONSTERMAGICXP {
            GlobalID = wizard.GameObjectID,
            XP = wizard.GameStats.m_monsterMagicXP,
            Level = wizard.GameStats.m_monsterMagicLevel
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICREQUESTCREATE))]
    private void ReceiveMonsterMagicRequestCreate(WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICREQUESTCREATE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.GameStats.m_monsterMagicXP += 30;
        int oldLevel = wizard.GameStats.m_monsterMagicLevel;
        byte newLevel = (byte) Math.Min(20, 1 + (wizard.GameStats.m_monsterMagicXP / 100));
        if (newLevel > oldLevel) {
            wizard.GameStats.m_monsterMagicLevel = newLevel;
            SendToSocket(new WIZARD2_53_PROTOCOL.MSG_MONSTERMAGICLEVELUP {
                GlobalID = wizard.GameObjectID,
                NewLevel = newLevel
            });
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATEMONSTERMAGICXP {
            GlobalID = wizard.GameObjectID,
            XP = wizard.GameStats.m_monsterMagicXP,
            Level = wizard.GameStats.m_monsterMagicLevel
        });

        Logger.Information("Created monstrology craft type {0} for mob {1}. XP: {2}, Level: {3}",
            Logger.Args(message.RequestType, message.MobTemplate,
                wizard.GameStats.m_monsterMagicXP, wizard.GameStats.m_monsterMagicLevel));
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICADDMONSTER))]
    private void ReceiveAddMonster(WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICADDMONSTER message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICADDMONSTER {
            GlobalID = message.GlobalID,
            SpellTemplateID = message.SpellTemplateID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICERASEMONSTER))]
    private void ReceiveEraseMonster(WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICERASEMONSTER message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_MONSTERMAGICERASEMONSTER {
            GlobalID = message.GlobalID,
            MobTemplateID = message.MobTemplateID
        });
    }
}
