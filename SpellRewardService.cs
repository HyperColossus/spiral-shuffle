using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Services;

internal sealed class SpellRewardService(SessionActor sessionActor) : MessageService(sessionActor) {
    internal const uint JobId = 770103;
    private readonly SpellRewardQueue _rewards = new();
    private DateTime _lastPoll;
    private Wizard _wizard;
    private bool _attached;

    [MessageHandler(typeof(CHARACTER_103_PROTOCOL.MSG_SETACTIVEWIZARD))]
    private void ReceiveActiveWizard(CHARACTER_103_PROTOCOL.MSG_SETACTIVEWIZARD message) {
        _wizard = message.Wizard;
        _attached = false;
    }

    [MessageHandler(typeof(SERVICE_101_PROTOCOL.MSG_ATTACHCOMPLETE))]
    private void StartRewards() {
        if (!RandomizerRuntime.Enabled || _wizard is null) {
            return;
        }

        _attached = true;

        SendToSocket(new GAME_5_PROTOCOL.MSG_KILL_CLIENT_PROCESS { JobID = JobId });
        SendToSocket(new GAME_5_PROTOCOL.MSG_START_CLIENT_PROCESS {
            OwnerGID = _wizard.GameObjectID,
            JobID = JobId,
            ScriptName = "Randomizer/SpellRewards.lua"
        });
    }

    [MessageHandler(typeof(SpellDropRewards))]
    private void ReceiveRewards(SpellDropRewards rewards) {
        var wizard = _wizard;
        if (!_attached || wizard?.SpellbookBehavior is null) {
            return;
        }
        uint[] learned = rewards.SpellIds.Where(id => wizard.SpellbookBehavior.HasSpell(id) &&
            RandomizerRuntime.DropSpellPool.Any(spell => spell.Variants.Contains(id))).Distinct().ToArray();
        RewardSpell[] notices = learned.Select(id => {
            var spell = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(id);
            var rank = spell.m_spellRank;
            return new RewardSpell(id, spell.m_displayName, spell.m_sMagicSchoolName,
                SpellRewardQueue.PipCost(rank.m_spellRank, rank.m_xPipSpell, rank.m_shadowPips,
                    new Dictionary<string, int> {
                        ["balance"] = rank.m_balancePips, ["death"] = rank.m_deathPips, ["fire"] = rank.m_firePips,
                        ["ice"] = rank.m_icePips, ["life"] = rank.m_lifePips, ["myth"] = rank.m_mythPips,
                        ["storm"] = rank.m_stormPips
                    }),
                spell.m_accuracy, rewards.Difficulty);
        }).ToArray();
        _rewards.Enqueue(notices);
    }

    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS))]
    private void ReceivePoll(GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS message) {
        // Client UI polls can arrive before attach completes or during teardown.
        // Never synchronously query sibling services from this periodic handler.
        if (!_attached || _wizard is null) {
            return;
        }

        if (message.JobID != JobId || !RandomizerRuntime.TryReadMenuMessage(message.Message, "SpellRewardUI", out int token)) {
            return;
        }

        if (token > 0) {
            _rewards.Acknowledge(token);
        }
        else if (DateTime.UtcNow - _lastPoll < TimeSpan.FromMilliseconds(250)) {
            return;
        }

        _lastPoll = DateTime.UtcNow;
        string payload = _wizard.IsInDuel ? "SpellRewardCombat" : _rewards.Current?.Payload ?? "SpellRewardEmpty";
        byte[] encoded = MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS { score = 0, gameName = payload });
        SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS { JobID = JobId, Message = (ByteString)encoded[8..] });
    }
}
