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

internal sealed class CastleToursModerationService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CastleToursModerationService(parentActor));

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSCSRRESULTS))]
    private void ReceiveCastleToursCSRResults(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSCSRRESULTS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSDELETECHARACTER))]
    private void ReceiveCastleToursDeleteCharacter(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSDELETECHARACTER message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CASTLETOURSGOTOFRIEND))]
    private void ReceiveCastleToursGoToFriend(WIZARD2_53_PROTOCOL.MSG_CASTLETOURSGOTOFRIEND message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSLEADERBOARDRESPONSE))]
    private void ReceiveCastleToursLeaderboardResponse(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSLEADERBOARDRESPONSE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPLAYERBANNED))]
    private void ReceiveCastleToursPlayerBanned(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSPLAYERBANNED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVERESULT))]
    private void ReceiveCastleToursRemoveResult(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSREMOVERESULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITIGNORED))]
    private void ReceiveCastleToursVisitIgnored(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLETOURSVISITIGNORED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESTELEPORTREJECTED))]
    private void ReceiveCastleGamesTeleportRejected(WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESTELEPORTREJECTED message) {
        SendToSocket(new WIZARDHOUSING_50_PROTOCOL.MSG_CASTLEGAMESTELEPORTREJECTED());
    }
}
