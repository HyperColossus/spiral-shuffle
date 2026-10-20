using System.Reflection;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.WizardData.Models.Player;
using Type = System.Type;

internal static class InfiniteManaTests {
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;
    private static int _checks;

    internal static int Run(Assembly assembly) {
        Type feature = assembly.GetType("Imlight.CoreLib.Game.InfiniteMana")!;
        MethodInfo menu = feature.GetMethod("HandleMenuSetting", Static)!;
        MethodInfo resolve = feature.GetMethod("Resolve", Static)!;
        MethodInfo refill = feature.GetMethod("Refill", Static)!;
        ulong account = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray());
        Wizard wizard = new() { AccountId = account, GameStats = new ServerWizGameStats(MagicSchool.Fire, 1) { m_baseMana = 437, m_currentMana = 0 } };
        Require(Setting(menu, account, 0) == -1, "Infinite mana must start off.");
        Require((int)resolve.Invoke(null, [wizard, 12])! == 12, "Disabled infinite mana changed the cost.");
        refill.Invoke(null, [wizard]);
        Require(wizard.GameStats.m_currentMana == 0, "Disabled infinite mana refilled the wizard.");
        foreach (int invalid in new[] { 1, -1, 100, int.MinValue, int.MaxValue }) {
            object?[] arguments = [account, Packet(invalid), Array.Empty<byte>()];
            Require(!(bool)menu.Invoke(null, arguments)!, "Invalid mana settings were accepted.");
        }
        Require(Setting(menu, account, 2) == 1, "Infinite mana could not enable.");
        Require(Setting(menu, account, 0) == 1, "Reading infinite mana reset the choice.");
        foreach (int requested in new[] { 0, 1, 30, 436, 437, 10000, -10 }) {
            Require((int)resolve.Invoke(null, [wizard, requested])! == 437, "Infinite mana did not hold the equipment-adjusted maximum.");
        }
        refill.Invoke(null, [wizard]);
        Require(wizard.GameStats.m_currentMana == 437, "Entering with no mana did not refill.");
        wizard.GameStats.m_baseMana = 25;
        refill.Invoke(null, [wizard]);
        Require(wizard.GameStats.m_currentMana == 25, "Changing equipment left mana above the new maximum.");
        Require(Setting(menu, account + 1, 0) == -1, "Infinite mana leaked into another account.");
        Require(Setting(menu, account, -2) == -1, "Infinite mana could not disable.");
        Require((int)resolve.Invoke(null, [wizard, 0])! == 0, "Turning the option off did not restore normal mana use.");
        Console.WriteLine($"Passed {_checks} infinite mana checks: menu validation, account isolation, full mana, adjusted caps and ordinary costs when off.");
        return _checks;
    }

    private static int Setting(MethodInfo menu, ulong account, int score) {
        object?[] arguments = [account, Packet(score), Array.Empty<byte>()];
        Require((bool)menu.Invoke(null, arguments)!, "Mana setting request failed.");
        MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS response = new();
        response.Decode(new BitReader(((byte[])arguments[2]!)[4..]));
        Require(response.gameName == "SpiralInfiniteMana", "Mana setting response used the wrong menu channel.");
        return response.score;
    }

    private static ByteString Packet(int score) => (ByteString)MessageEncoder.Encode(new MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS {
        score = score, gameName = "SpiralInfiniteMana"
    })[8..];

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}
