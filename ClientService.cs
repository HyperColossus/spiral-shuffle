/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * ========================================================================
 * CLIENT SERVICE
 * ========================================================================
 * 
 * PURPOSE:
 * Manages client connection lifecycle, handling disconnection, logout,
 * server requirement queries, and item finder services.
 */

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Requirements;
using Imlight.CoreLib.Game.Requirements.Contexts;
using Imlight.CoreLib.Shared.Networking;

namespace Imlight.CoreLib.Game.Services;

internal class ClientService(SessionActor sessionActor) : MessageService(sessionActor) {
    
    protected static Props Props(SessionActor parentActor) 
        => Akka.Actor.Props.Create(() => new ClientService(parentActor));

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_CLIENT_DISCONNECT))]
    private void ReceiveClientDisconnect() {
        SendToSocket(new GAME_5_PROTOCOL.MSG_CLIENT_DISCONNECT());
        CloseSession();
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_QUERY_LOGOUT))]
    private void ReceiveQueryLogout(GAME_5_PROTOCOL.MSG_QUERY_LOGOUT message) =>
        SendToSocket(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT());

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_REQASKSERVER))]
    private void ReceiveReqServer(GAME_5_PROTOCOL.MSG_REQASKSERVER message) {
        var response = 1u;
        try {
            if (message.Requirement.Length > 0) {
                var wizard = GetActiveWizard();
                var playerObj = GetActiveGameObject();
                var serializer = new BindSerializer();
                if (serializer.Deserialize<RequirementList>((byte[]) message.Requirement, out var reqList) && reqList is not null) {
                    var context = new GenericRequirementContext(reqList, SessionActor.ActorRef, playerObj, wizard);
                    bool met = RequirementDispatcher.EvaluateRequirements(reqList, context);
                    response = met ? 1u : 0u;
                }
            }
        }
        catch (Exception ex) {
            Logger.Warning("Error evaluating MSG_REQASKSERVER {0}: {1}", Logger.Args(message.RequestID, ex.Message));
            response = 1u;
        }

        SendToSocket(new GAME_5_PROTOCOL.MSG_REQASKSERVER {
            RequestID = message.RequestID,
            Requirement = message.Requirement,
            Response = response,
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_OPENITEMFINDER))]
    private void ReceiveOpenItemFinder(WIZARD2_53_PROTOCOL.MSG_OPENITEMFINDER message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_OPENITEMFINDER {
            GlobalID = wizard?.GameObjectID ?? message.GlobalID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERREQUESTBANK))]
    private void ReceiveItemFinderRequestBank(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERREQUESTBANK message) {
        // Bank inventory is already synchronized to client on bank open.
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERREQUESTHOUSE))]
    private void ReceiveItemFinderRequestHouse(WIZARD2_53_PROTOCOL.MSG_ITEMFINDERREQUESTHOUSE message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMFINDERHOUSECOMPLETE());
    }

}
