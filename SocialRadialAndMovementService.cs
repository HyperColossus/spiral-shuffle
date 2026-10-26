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

internal sealed class SocialRadialAndMovementService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new SocialRadialAndMovementService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_RADIALZONECLUSTERQUICKCHAT))]
    private void ReceiveRadialZoneClusterQuickChat(GAME_5_PROTOCOL.MSG_RADIALZONECLUSTERQUICKCHAT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_RADIALZONECLUSTERQUICKCHATEXT))]
    private void ReceiveRadialZoneClusterQuickChatExt(WIZARD2_53_PROTOCOL.MSG_RADIALZONECLUSTERQUICKCHATEXT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_RADIALQUICKCHATEXT))]
    private void ReceiveRadialQuickChatExt(GAME_5_PROTOCOL.MSG_RADIALQUICKCHATEXT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTRADIALFRIENDQUICKCHAT))]
    private void ReceiveRequestRadialFriendQuickChat(GAME_5_PROTOCOL.MSG_REQUESTRADIALFRIENDQUICKCHAT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQUESTRADIALOWNERCHAT))]
    private void ReceiveRequestRadialOwnerChat(GAME_5_PROTOCOL.MSG_REQUESTRADIALOWNERCHAT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ADDPLAYERTOBASICCHATCHANNEL))]
    private void ReceiveAddPlayerToBasicChatChannel(WIZARD2_53_PROTOCOL.MSG_ADDPLAYERTOBASICCHATCHANNEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REMOVEPLAYERFROMBASICCHATCHANNEL))]
    private void ReceiveRemovePlayerFromBasicChatChannel(WIZARD2_53_PROTOCOL.MSG_REMOVEPLAYERFROMBASICCHATCHANNEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REMOVEPLAYERFROMBASICCHATCHANNELUPDATE))]
    private void ReceiveRemovePlayerFromBasicChatChannelUpdate(WIZARD2_53_PROTOCOL.MSG_REMOVEPLAYERFROMBASICCHATCHANNELUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_INVITEALLTOBASICCHATCHANNEL))]
    private void ReceiveInviteAllToBasicChatChannel(WIZARD2_53_PROTOCOL.MSG_INVITEALLTOBASICCHATCHANNEL message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BASICCHATCHANNELUPDATECOOLDOWN))]
    private void ReceiveBasicChatChannelUpdateCooldown(WIZARD2_53_PROTOCOL.MSG_BASICCHATCHANNELUPDATECOOLDOWN message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BASICCHATCHANNELERRORMESSAGE))]
    private void ReceiveBasicChatChannelErrorMessage(WIZARD2_53_PROTOCOL.MSG_BASICCHATCHANNELERRORMESSAGE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYERTELEPORTINGTOFRIEND))]
    private void ReceivePlayerTeleportingToFriend(WIZARDHOUSING_50_PROTOCOL.MSG_PLAYERTELEPORTINGTOFRIEND message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTTOSTART))]
    private void ReceiveTeleportToStart(WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTTOSTART message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTTOSTART {
            Inside = message.Inside
        });
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MOVECORRECTION))]
    private void ReceiveMoveCorrection(GAME_5_PROTOCOL.MSG_MOVECORRECTION message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVE))]
    private void ReceiveMBMove(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVESTATE))]
    private void ReceiveMBMoveState(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVESTATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVE_T))]
    private void ReceiveMBMoveT(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_MOVE_T message) {
        SendToSocket(message);
    }
}
