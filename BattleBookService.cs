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

internal sealed class BattleBookService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new BattleBookService(parentActor));

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BattleBookAdd))]
    private void ReceiveBattleBookAdd(WIZARD2_53_PROTOCOL.MSG_BattleBookAdd message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BattleBookRemove))]
    private void ReceiveBattleBookRemove(WIZARD2_53_PROTOCOL.MSG_BattleBookRemove message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BattleBookClear))]
    private void ReceiveBattleBookClear(WIZARD2_53_PROTOCOL.MSG_BattleBookClear message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BattleBookRating))]
    private void ReceiveBattleBookRating(WIZARD2_53_PROTOCOL.MSG_BattleBookRating message) {
        SendToSocket(message);
    }
}
