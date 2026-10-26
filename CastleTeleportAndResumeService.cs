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

internal sealed class CastleTeleportAndResumeService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleTeleportAndResumeService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTPLAYER))]
    private void ReceiveCastleToursTeleportPlayer(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTPLAYER message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTPLAYER {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            TeleportData = message.TeleportData,
            Source = message.Source,
            LeaderboardTeleport = message.LeaderboardTeleport,
            OwnerID = message.OwnerID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTREJECTED))]
    private void ReceiveCastleToursTeleportRejected(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTREJECTED message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSTELEPORTREJECTED());
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_RESUMEINSTANCETELEPORTPLAYER))]
    private void ReceiveResumeInstanceTeleportPlayer(WIZARD2_53_PROTOCOL.MSG_RESUMEINSTANCETELEPORTPLAYER message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_RESUMEINSTANCETELEPORTPLAYER {
            ClusterGID = message.ClusterGID
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_REQUESTHOUSETELEPORT))]
    private void ReceiveRequestHouseTeleport(WIZARD_12_PROTOCOL.MSG_REQUESTHOUSETELEPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_EXITCONFIRMTELEPORT))]
    private void ReceiveExitConfirmTeleport(WIZARD_12_PROTOCOL.MSG_EXITCONFIRMTELEPORT message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_EXITCONFIRMTELEPORT {
            ZoneCRC = message.ZoneCRC,
            PromptKey = message.PromptKey
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETTELEPORT))]
    private void ReceivePetTeleport(WIZARD2_53_PROTOCOL.MSG_PETTELEPORT message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_PETTELEPORT {
            PositionX = message.PositionX,
            PositionY = message.PositionY,
            PositionZ = message.PositionZ,
            Yaw = message.Yaw
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTEDTODELETEDLOT))]
    private void ReceiveTeleportedToDeletedLot(WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTEDTODELETEDLOT message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_TELEPORTEDTODELETEDLOT {
            Quarantined = message.Quarantined
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_EMPTYLOTCHECK))]
    private void ReceiveEmptyLotCheck(WIZARDHOUSING_50_PROTOCOL.MSG_EMPTYLOTCHECK message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_EMPTYLOTCHECK {
            EmptyLotGID = message.EmptyLotGID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_DILEVELUPELIXIRTELEPORT))]
    private void ReceiveDILevelUpElixirTeleport(WIZARD2_53_PROTOCOL.MSG_DILEVELUPELIXIRTELEPORT message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_DILEVELUPELIXIRTELEPORT {
            ItemGlobalID = message.ItemGlobalID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PATCHBEFORETELEPORT))]
    private void ReceivePatchBeforeTeleport(WIZARD2_53_PROTOCOL.MSG_PATCHBEFORETELEPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_TELEPORT))]
    private void ReceiveMBTeleport(MOVEBEHAVIOR_15_PROTOCOL.MSG_MB_TELEPORT message) {
        SendToSocket(message);
    }
}
