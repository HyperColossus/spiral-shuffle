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

internal sealed class CastleToursService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleToursService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUEST))]
    private void ReceiveCastleToursRequest(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITCASTLE))]
    private void ReceiveVisitCastle(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITCASTLE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        Teleport(
            destinationZone: "WizardCity/Interiors/WC_Housing_Dorm_Interior",
            destinationLocation: "Start",
            makePrivate: true
        );
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSSENDRATING))]
    private void ReceiveSendRating(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSSENDRATING message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTLEADERBOARD))]
    private void ReceiveRequestLeaderboard(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTLEADERBOARD message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTLEADERBOARD {
            LeaderboardType = message.LeaderboardType,
            SortType = message.SortType,
            PageNumber = message.PageNumber,
            LeaderboardData = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTNEXTLEADERBOARD))]
    private void ReceiveRequestNextLeaderboard(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTNEXTLEADERBOARD message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTNEXTLEADERBOARD {
            LeaderboardType = message.LeaderboardType,
            ClusterGID = message.ClusterGID,
            OwnerGID = message.OwnerGID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLES))]
    private void ReceiveRequestMyCastles(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLES message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLES {
            GlobalID = wizard.GameObjectID,
            Data = ByteString.Empty,
            Days = 30
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLEDATA))]
    private void ReceiveRequestMyCastleData(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLEDATA message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTMYCASTLEDATA {
            GlobalID = wizard.GameObjectID,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTFRIENDS))]
    private void ReceiveRequestFriends(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTFRIENDS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREQUESTFRIENDS {
            CharacterID = wizard.CharId,
            Data = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADD))]
    private void ReceiveCastleToursAdd(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADD message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVE))]
    private void ReceiveCastleToursRemove(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDRESULT {
            Status = 0
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPOSTRATEHOUSE))]
    private void ReceivePostRateHouse(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPOSTRATEHOUSE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPOSTRATEHOUSE {
            CharacterID = wizard.CharId,
            BadgeCredit = 1,
            MaxBadgeCredit = 10
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDFAVORITE))]
    private void ReceiveAddFavorite(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDFAVORITE message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSADDFAVORITE {
            GlobalID = message.GlobalID,
            Success = 1
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVEFAVORITE))]
    private void ReceiveRemoveFavorite(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVEFAVORITE message) {
        SendToSocket(message);
    }
}
