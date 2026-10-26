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

internal sealed class DeckathalonService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new DeckathalonService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTCLASSPROJECTINFO))]
    private void ReceiveRequestClassProjectInfo(WIZARD2_53_PROTOCOL.MSG_REQUESTCLASSPROJECTINFO message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_REQUESTCLASSPROJECTINFO {
            Buffer = ByteString.Empty,
            TokenBuffer = ByteString.Empty,
            DisplayInfo = 1
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTUSETOKEN))]
    private void ReceiveClassProjectUseToken(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTUSETOKEN message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTSTATUS {
            Status = 1,
            ProjectID = message.ProjectID,
            Count = 3,
            Points = 100,
            HasUnclaimedReward = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTUSECROWNS))]
    private void ReceiveClassProjectUseCrowns(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTUSECROWNS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTSTATUS {
            Status = 1,
            ProjectID = message.ProjectID,
            Count = 5,
            Points = 250,
            HasUnclaimedReward = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTREQUESTTELEPORT))]
    private void ReceiveClassProjectRequestTeleport(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTREQUESTTELEPORT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTCLAIMREWARD))]
    private void ReceiveClassProjectClaimReward(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTCLAIMREWARD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTREQUESTLEADERBOARD))]
    private void ReceiveClassProjectRequestLeaderboard(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTREQUESTLEADERBOARD message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTREQUESTLEADERBOARD {
            ProjectID = message.ProjectID,
            TopBuffer = ByteString.Empty,
            Buffer = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSREQUESTFRIENDS))]
    private void ReceiveClassProjectsRequestFriends(WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSREQUESTFRIENDS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CLASSPROJECTSREQUESTFRIENDS {
            ProjectID = message.ProjectID,
            CharacterID = wizard.CharId,
            Data = ByteString.Empty,
            PlayerName = 0,
            LootTable = ByteString.Empty,
            BuddyListID = 0
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTBUTTON))]
    private void ReceiveUpdateClassProjectButton(WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTBUTTON message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTSTATUS))]
    private void ReceiveUpdateClassProjectStatus(WIZARD2_53_PROTOCOL.MSG_UPDATECLASSPROJECTSTATUS message) {
        SendToSocket(message);
    }
}
