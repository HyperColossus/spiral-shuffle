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

internal sealed class AdventureKioskAndFocusService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new AdventureKioskAndFocusService(parentActor));

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTASKADDTOKIOSK))]
    private void ReceiveAdventurePartyAskAddToKiosk(WIZARD3_56_PROTOCOL.MSG_ADVENTUREPARTASKADDTOKIOSK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CANSEETIEREDSPELLGROUPS))]
    private void ReceiveCanSeeTieredSpellGroups(WIZARD2_53_PROTOCOL.MSG_CANSEETIEREDSPELLGROUPS message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CANSEETIEREDSPELLGROUPS {
            Visibility = message.Visibility,
            CheckTieredSpellNotifications = message.CheckTieredSpellNotifications,
            ShardType = message.ShardType
        });
    }

    [MessageHandler(typeof(LOGIN_7_PROTOCOL.MSG_CHANGECHARACTERNAME))]
    private void ReceiveChangeCharacterName(LOGIN_7_PROTOCOL.MSG_CHANGECHARACTERNAME message) {
        var wizard = GetActiveWizard();
        if (wizard is not null && !string.IsNullOrWhiteSpace(message.NewName.ToString())) {
            wizard.PlayerNameBehavior.NameOverride = message.NewName;
        }
        SendToSocket(message);
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_CHEERCOSTMAPUPDATE))]
    private void ReceiveCheerCostMapUpdate(PET_9_PROTOCOL.MSG_CHEERCOSTMAPUPDATE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_CHOOSEFOCUS))]
    private void ReceiveChooseFocus(WIZARD_12_PROTOCOL.MSG_CHOOSEFOCUS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_CREATESETFAILED))]
    private void ReceiveCreateSetFailed(WIZARD2_53_PROTOCOL.MSG_CREATESETFAILED message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_CREATESETFAILED());
    }
}
