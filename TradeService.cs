using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.CoreObject;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class TradeService(SessionActor sessionActor) : MessageService(sessionActor) {

    private sealed class TradeSession {
        public ulong Player1Gid { get; init; }
        public ulong Player2Gid { get; init; }
        public TradeService Service1 { get; init; }
        public TradeService Service2 { get; init; }
        public List<ulong> Player1Items { get; } = [];
        public List<ulong> Player2Items { get; } = [];
        public uint Player1Gold { get; set; }
        public uint Player2Gold { get; set; }
        public bool Player1Ready { get; set; }
        public bool Player2Ready { get; set; }
    }

    private static readonly ConcurrentDictionary<ulong, TradeService> s_services = new();
    private static readonly ConcurrentDictionary<ulong, TradeSession> s_trades = new();

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new TradeService(parentActor));

    private void RegisterActive() {
        var wizard = GetActiveWizard();
        if (wizard != null) {
            s_services[wizard.GameObjectID] = this;
        }
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_REQUEST))]
    private void ReceiveTradeRequest(WIZARD_12_PROTOCOL.MSG_TRADE_REQUEST message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (s_services.TryGetValue(message.TargetGID, out var targetService)) {
            targetService.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_REQUEST {
                TargetGID = wizard.GameObjectID
            });
        }
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_CREATE))]
    private void ReceiveTradeCreate(WIZARD_12_PROTOCOL.MSG_TRADE_CREATE message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (!s_services.TryGetValue(message.TargetGID, out var targetService)) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_RESULT {
                TargetGID = message.TargetGID,
                Status = 1
            });
            return;
        }

        var session = new TradeSession {
            Player1Gid = message.TargetGID,
            Player2Gid = wizard.GameObjectID,
            Service1 = targetService,
            Service2 = this
        };
        s_trades[message.TargetGID] = session;
        s_trades[wizard.GameObjectID] = session;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_JOIN_STATUS {
            TargetGID = message.TargetGID,
            TargetStatus = 1,
            PlayerStatus = 1
        });
        targetService.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_JOIN_STATUS {
            TargetGID = wizard.GameObjectID,
            TargetStatus = 1,
            PlayerStatus = 1
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_JOIN_STATUS))]
    private void ReceiveTradeJoinStatus(WIZARD_12_PROTOCOL.MSG_TRADE_JOIN_STATUS message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (message.PlayerStatus == 0) {
            // Declined / Cancelled
            if (s_trades.TryRemove(wizard.GameObjectID, out var trade)) {
                s_trades.TryRemove(trade.Player1Gid, out _);
                s_trades.TryRemove(trade.Player2Gid, out _);
                var other = trade.Player1Gid == wizard.GameObjectID ? trade.Service2 : trade.Service1;
                other?.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_RESULT { Status = 1 });
            }
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_RESULT { Status = 1 });
            return;
        }

        // Accepted join
        ReceiveTradeCreate(new WIZARD_12_PROTOCOL.MSG_TRADE_CREATE { TargetGID = message.TargetGID });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_ITEM))]
    private void ReceiveTradeChangeItem(WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_ITEM message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (!s_trades.TryGetValue(wizard.GameObjectID, out var trade)) return;

        bool isP1 = trade.Player1Gid == wizard.GameObjectID;
        var myItems = isP1 ? trade.Player1Items : trade.Player2Items;
        var targetService = isP1 ? trade.Service2 : trade.Service1;

        if (message.Action == 1) {
            if (!myItems.Contains(message.ChangeGID)) myItems.Add(message.ChangeGID);
        } else {
            myItems.Remove(message.ChangeGID);
        }

        trade.Player1Ready = false;
        trade.Player2Ready = false;

        targetService.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_ITEM {
            TargetGID = wizard.GameObjectID,
            ChangeGID = message.ChangeGID,
            ItemTemplate = message.ItemTemplate,
            ItemEnchant = message.ItemEnchant,
            Action = message.Action
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_MONEY))]
    private void ReceiveTradeChangeMoney(WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_MONEY message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (!s_trades.TryGetValue(wizard.GameObjectID, out var trade)) return;

        bool isP1 = trade.Player1Gid == wizard.GameObjectID;
        var targetService = isP1 ? trade.Service2 : trade.Service1;

        if (isP1) trade.Player1Gold = message.Amount;
        else trade.Player2Gold = message.Amount;

        trade.Player1Ready = false;
        trade.Player2Ready = false;

        targetService.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_CHANGE_MONEY {
            TargetGID = wizard.GameObjectID,
            Amount = message.Amount
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_TRADE_READY_STATUS))]
    private void ReceiveTradeReadyStatus(WIZARD_12_PROTOCOL.MSG_TRADE_READY_STATUS message) {
        RegisterActive();
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (!s_trades.TryGetValue(wizard.GameObjectID, out var trade)) return;

        bool isP1 = trade.Player1Gid == wizard.GameObjectID;
        var targetService = isP1 ? trade.Service2 : trade.Service1;

        if (isP1) trade.Player1Ready = (message.PlayerStatus != 0);
        else trade.Player2Ready = (message.PlayerStatus != 0);

        targetService.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_READY_STATUS {
            TargetGID = wizard.GameObjectID,
            TargetStatus = message.PlayerStatus,
            PlayerStatus = message.PlayerStatus
        });

        if (trade.Player1Ready && trade.Player2Ready) {
            ExecuteTrade(trade);
        }
    }

    private static void ExecuteTrade(TradeSession trade) {
        s_trades.TryRemove(trade.Player1Gid, out _);
        s_trades.TryRemove(trade.Player2Gid, out _);

        var wiz1 = trade.Service1.GetActiveWizard();
        var wiz2 = trade.Service2.GetActiveWizard();

        if (wiz1 != null && wiz2 != null) {
            // Gold transfer
            if (trade.Player1Gold > 0 && wiz1.GameStats.m_currentGold >= trade.Player1Gold) {
                wiz1.RemoveGold((int) trade.Player1Gold);
                wiz2.AddGold((int) trade.Player1Gold);
            }
            if (trade.Player2Gold > 0 && wiz2.GameStats.m_currentGold >= trade.Player2Gold) {
                wiz2.RemoveGold((int) trade.Player2Gold);
                wiz1.AddGold((int) trade.Player2Gold);
            }

            trade.Service1.SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wiz1.GameStats.m_currentGold,
                MaxGold = wiz1.GameStats.m_baseGoldPouch
            });
            trade.Service2.SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wiz2.GameStats.m_currentGold,
                MaxGold = wiz2.GameStats.m_baseGoldPouch
            });
        }

        trade.Service1.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_RESULT {
            TargetGID = trade.Player2Gid,
            ItemsGained = (uint) trade.Player2Items.Count,
            ItemsLost = (uint) trade.Player1Items.Count,
            Status = 0
        });
        trade.Service2.SendToSocket(new WIZARD_12_PROTOCOL.MSG_TRADE_RESULT {
            TargetGID = trade.Player1Gid,
            ItemsGained = (uint) trade.Player1Items.Count,
            ItemsLost = (uint) trade.Player2Items.Count,
            Status = 0
        });
    }
}
