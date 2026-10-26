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

internal sealed class CombatMechanicsService(SessionActor sessionActor) : MessageService(sessionActor) {

    public static Props Props(SessionActor parentActor)
        => Akka.Actor.Props.Create(() => new CombatMechanicsService(parentActor));

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_AGGRO))]
    private void ReceiveAggro(WIZARD_12_PROTOCOL.MSG_AGGRO message) {
        // Roaming mob encounters player or combat sigil triggers
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_AGGRO {
            GlobalID = message.GlobalID,
            LocX = message.LocX,
            LocY = message.LocY,
            LocZ = message.LocZ,
            Yaw = message.Yaw,
            SigilGID = message.SigilGID,
            SigilX = message.SigilX,
            SigilY = message.SigilY,
            SigilZ = message.SigilZ,
            SigilYaw = message.SigilYaw
        });
    }

    [MessageHandler(typeof(AISCLIENT_19_PROTOCOL.MSG_AISMESSAGE))]
    private void ReceiveAISMessage(AISCLIENT_19_PROTOCOL.MSG_AISMESSAGE message) {
        SendToSocket(new AISCLIENT_19_PROTOCOL.MSG_AISMESSAGE {
            Message = message.Message,
            Modal = message.Modal
        });
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATADD))]
    private void ReceiveCombatAdd(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATADD message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATREMOVE))]
    private void ReceiveCombatRemove(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATREMOVE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPAUSED))]
    private void ReceiveCombatPaused(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPAUSED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATCHEAT))]
    private void ReceiveCombatCheat(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATCHEAT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATLOADED))]
    private void ReceiveCombatLoaded(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATLOADED message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATMATCHRESULT))]
    private void ReceiveCombatMatchResult(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATMATCHRESULT message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPHASE))]
    private void ReceiveCombatPhase(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPHASE message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPHASEFORSPECTATORS))]
    private void ReceiveCombatPhaseForSpectators(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPHASEFORSPECTATORS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPIPS))]
    private void ReceiveCombatPips(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATPIPS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATREVEALHANGING))]
    private void ReceiveCombatRevealHanging(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATREVEALHANGING message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATSTATS))]
    private void ReceiveCombatStats(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATSTATS message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATUPFIRST))]
    private void ReceiveCombatUpFirst(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATUPFIRST message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATVICTORY))]
    private void ReceiveCombatVictory(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATVICTORY message) {
        SendToSocket(new DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATVICTORY());
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SHOWCOMBATUI))]
    private void ReceiveShowCombatUI(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_SHOWCOMBATUI message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_UPDATECOMBATPARTICIPANT))]
    private void ReceiveUpdateCombatParticipant(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_UPDATECOMBATPARTICIPANT message) {
        SendToSocket(message);
    }
}
