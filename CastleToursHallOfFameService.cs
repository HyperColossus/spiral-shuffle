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

internal sealed class CastleToursHallOfFameService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleToursHallOfFameService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO))]
    private void ReceiveCastleToursFavoriteInfo(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            FavoriteData = message.FavoriteData
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO2))]
    private void ReceiveCastleToursFavoriteInfo2(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO2 message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSFAVORITEINFO2 {
            CharacterID = wizard?.CharId ?? message.CharacterID,
            FavoriteData = message.FavoriteData
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAME))]
    private void ReceiveCastleToursHallOfFame(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAME message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAMERESPONSE {
            State = 1,
            Buffer = ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAMERATINGS))]
    private void ReceiveCastleToursHallOfFameRatings(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAMERATINGS message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSHALLOFFAMERATINGS {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            Buffer = message.Buffer
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSENABLERATINGDISPLAY))]
    private void ReceiveCastleToursEnableRatingDisplay(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSENABLERATINGDISPLAY message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSENABLERATINGDISPLAY {
            CastleType = message.CastleType,
            Rating = message.Rating,
            FirstDisplay = message.FirstDisplay,
            TemplateID = message.TemplateID,
            LotGID = message.LotGID,
            StructureGID = message.StructureGID
        });
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSRATINGDISPLAY))]
    private void ReceiveCastleToursRatingDisplay(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSRATINGDISPLAY message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSRATINGDISPLAY());
    }
}
