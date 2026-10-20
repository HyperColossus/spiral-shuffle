using System;
using Akka.Actor;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game;

internal static class InfiniteMana {
    private static readonly Lazy<AccountToggleStore> Settings = new(() => new("Config/Randomizer-infinite-mana.json"));

    internal static bool IsEnabled(ulong accountId) => RandomizerRuntime.Enabled && Settings.Value.Get(accountId);

    internal static int Resolve(Wizard wizard, int requested) => IsEnabled(wizard.AccountId)
        ? Math.Max(0, wizard.GameStats.m_baseMana) : requested;

    internal static void Refill(Wizard wizard) {
        if (IsEnabled(wizard.AccountId)) {
            wizard.GameStats.m_currentMana = Resolve(wizard, wizard.GameStats.m_currentMana);
        }
    }

    internal static void Synchronize(Wizard wizard, IActorRef player) {
        if (IsEnabled(wizard.AccountId)) {
            Refill(wizard);
            player.Tell(new WIZARD_12_PROTOCOL.MSG_UPDATEMANA {
                Mana = wizard.GameStats.m_currentMana,
                MaxMana = wizard.GameStats.GetClientTypeAlternative().m_baseMana
            });
        }
    }

    internal static bool HandleMenuSetting(ulong accountId, ByteString message, out byte[] response) {
        response = [];
        if (!RandomizerRuntime.TryReadMenuMessage(message, "SpiralInfiniteMana", out int score) ||
            score is not (0 or 2 or -2)) {
            return false;
        }

        if (score != 0) {
            Settings.Value.Set(accountId, score > 0);
        }

        byte[] packet = MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS {
            score = IsEnabled(accountId) ? 1 : -1, gameName = "SpiralInfiniteMana"
        });
        response = packet[8..];
        return true;
    }
}
