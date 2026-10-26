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

internal sealed class MinigameRewardService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new MinigameRewardService(parentActor));

    private void GrantScoreReward(int score) {
        if (GetActiveWizard() is not { } wizard) return;

        wizard.GameStats.m_potionCharge = wizard.GameStats.m_potionMax;
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEPOTIONS {
            PotionMax = wizard.GameStats.m_potionMax,
            PotionCharge = wizard.GameStats.m_potionCharge
        });

        int goldReward = Math.Max(50, score / 10);
        wizard.AddGold(goldReward);
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_CONNECT))]
    private void ReceiveDoodleDougConnect(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_MOVED))]
    private void ReceiveDoodleDougMoved(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_REWARDS))]
    private void ReceiveDoodleDougRewards(DOODLEDOUG_MESSAGES_41_PROTOCOL.MSG_DOODLEDOUG_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_CONNECT))]
    private void ReceiveSkullRidersConnect(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_MOVED))]
    private void ReceiveSkullRidersMoved(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_REWARDS))]
    private void ReceiveSkullRidersRewards(SKULLRIDERS_MESSAGES_40_PROTOCOL.MSG_SKULLRIDERS_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_CONNECT))]
    private void ReceiveMG1Connect(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_MOVED))]
    private void ReceiveMG1Moved(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_REWARDS))]
    private void ReceiveMG1Rewards(MG1_MESSAGES_42_PROTOCOL.MSG_MG1_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG2_MESSAGES_43_PROTOCOL.MSG_MG2_CONNECT))]
    private void ReceiveMG2Connect(MG2_MESSAGES_43_PROTOCOL.MSG_MG2_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG2_MESSAGES_43_PROTOCOL.MSG_MG2_MOVED))]
    private void ReceiveMG2Moved(MG2_MESSAGES_43_PROTOCOL.MSG_MG2_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_CONNECT))]
    private void ReceiveMG3Connect(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_MOVED))]
    private void ReceiveMG3Moved(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_REWARDS))]
    private void ReceiveMG3Rewards(MG3_MESSAGES_44_PROTOCOL.MSG_MG3_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_CONNECT))]
    private void ReceiveMG4Connect(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_MOVED))]
    private void ReceiveMG4Moved(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_REWARDS))]
    private void ReceiveMG4Rewards(MG4_MESSAGES_45_PROTOCOL.MSG_MG4_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_CONNECT))]
    private void ReceiveMG5Connect(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_MOVED))]
    private void ReceiveMG5Moved(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_REWARDS))]
    private void ReceiveMG5Rewards(MG5_MESSAGES_46_PROTOCOL.MSG_MG5_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_CONNECT))]
    private void ReceiveMG6Connect(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_CONNECT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_MOVED))]
    private void ReceiveMG6Moved(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_MOVED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_REWARDS))]
    private void ReceiveMG6Rewards(MG6_MESSAGES_47_PROTOCOL.MSG_MG6_REWARDS message) {
        GrantScoreReward(message.score);
        SendToSocket(message);
    }
}
