using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Wad;
using Imlight.Common;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;
using Type = System.Type;

internal static class TrainerTests {
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static int _checks;

    internal static int Run(Assembly assembly, string root) {
        using FileStream patched = File.OpenRead(Path.Combine(root, ".runtime/patch/V_r806919.Wizard_1_610/Data/GameData/Root.wad"));
        assembly.GetType("Imlight.CoreLib.Shared.Resources.RootArchiveLoader")!.GetField("s_rootWad", Static)!
            .SetValue(null, ArchiveParser.Parse(patched)!);
        ulong account = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray());
        assembly.GetType("Imlight.CoreLib.Game.RandomizerRuntime")!.GetMethod("SetMode", Static)!.Invoke(null, [account, 3]);
        Wizard wizard = new() { AccountId = account, MagicSchoolBehavior = new ServerMagicSchoolBehavior {
            MagicSchool = MagicSchool.Fire, Level = 1, TrainingPoints = 0 }, SpellbookBehavior = new ServerWizSpellbookBehavior() };
        Type rules = assembly.GetType("Imlight.CoreLib.Game.TrainerRules")!;
        MethodInfo cost = rules.GetMethod("Cost", Static)!;
        MethodInfo canTrain = rules.GetMethod("CanTrain", Static)!;
        MethodInfo clientId = rules.GetMethod("ClientId", Static)!;
        NPCSpellEntry fire = new() { TemplateID = 103007158, Level = 1 };
        NPCSpellEntry storm = new() { TemplateID = 2062265892, Level = 1 };
        Require((int)cost.Invoke(null, [wizard, fire])! == 0 && (bool)canTrain.Invoke(null, [wizard, fire])!,
            "A Fire wizard cannot train its normal trainer's randomized starter for free.");
        Require((int)cost.Invoke(null, [wizard, storm])! == 1 && !(bool)canTrain.Invoke(null, [wizard, storm])!,
            "Another school's trainer became free based on a randomized spell's school.");
        uint alias = (uint)clientId.Invoke(null, [wizard, fire.TemplateID])!;
        SpellTemplate card = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(alias);
        Require(card.GetType() == typeof(SpellTemplate), "A randomized book card still requires an unregistered native upgrade group.");
        Type componentType = assembly.GetType("Imlight.CoreLib.Game.Zone.Components.InteractTrainerComponent")!;
        object trainer = Activator.CreateInstance(componentType, Instance, null,
            [RuntimeHelpers.GetUninitializedObject(typeof(ZoneEntity))], null)!;
        componentType.GetProperty("SpellInventory", Instance)!.SetValue(trainer,
            new List<NPCSpellEntry> { new() { TemplateID = 0 }, fire, storm });
        MethodInfo optionsMethod = componentType.GetMethod("GetServiceOptions", Instance)!;
        WizTrainingOption[] options = ((IEnumerable<ServiceOptionBase>)optionsMethod.Invoke(trainer, [wizard])!).Cast<WizTrainingOption>().ToArray();
        Require(options.Length == 2 && options[0].m_trainingIndex == 1 && options[1].m_trainingIndex == 2,
            "An invalid inventory row shifted the trainer's indices.");
        Require(options[0].m_spellName == card.m_name && options[0].m_trainingCost == 0 && options[0].m_bCanTrain,
            "The trainer preview and spellbook use different card identities or costs.");
        wizard.MagicSchoolBehavior.MagicSchool = MagicSchool.Storm;
        wizard.MagicSchoolBehavior.TrainingPoints = 3;
        _ = optionsMethod.Invoke(trainer, [wizard]);
        Require(options[0].m_trainingCost == 0, "One player's trainer request changed another player's options.");
        wizard.SpellbookBehavior.LearnedSpellTemplateIds.Add((uint)fire.TemplateID);
        Require(!(bool)canTrain.Invoke(null, [wizard, fire])!, "A known training slot could be purchased twice.");
        storm.RequiredSpellID = fire.TemplateID;
        storm.Level = 2;
        Require(!(bool)canTrain.Invoke(null, [wizard, storm])!, "A forged training request bypassed the level requirement.");
        wizard.MagicSchoolBehavior.Level = 2;
        Require((bool)canTrain.Invoke(null, [wizard, storm])!, "A valid prerequisite did not unlock training.");
        wizard.SpellbookBehavior.LearnedSpellTemplateIds.Clear();
        Require(!(bool)canTrain.Invoke(null, [wizard, storm])!, "A training request bypassed a prerequisite.");
        using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".runtime/generated/catalog.json")));
        uint[] starters = [84361, 103007158, 1067010286, 1731857280, 2062265892];
        JsonElement[] rolls = catalog.RootElement.GetProperty("SpellSwaps").EnumerateArray()
            .Where(swap => starters.Contains(swap.GetProperty("From").GetUInt32())).ToArray();
        Require(rolls.Length == starters.Length && rolls.Select(roll => roll.GetProperty("Target").GetString()).Distinct().Count() == starters.Length,
            "Starter trainer rolls repeated the same displayed spell.");
        Console.WriteLine($"Passed {_checks} trainer checks: original-school pricing, stable indices, prerequisites, independent options and visible spell variants.");
        return _checks;
    }

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}
