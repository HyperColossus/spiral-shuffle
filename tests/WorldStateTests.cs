using System.Collections;
using System.Reflection;

internal static class WorldStateTests {
    internal static int Run(Assembly assembly) {
        Type render = assembly.GetType("Imlight.CoreLib.Game.Zone.Components.RenderComponent")!;
        Type modType = assembly.GetType("Imlight.CoreLib.WizardData.Models.Player.Dynamod")!;
        MethodInfo matching = render.GetMethod("MatchingDynamods", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo resolve = render.GetMethod("ResolvePlayerState", BindingFlags.Static | BindingFlags.NonPublic)!;
        const string path = "WizardCity/WC_Hub";
        const string name = "WizardZone_TheCommons";
        const string tag = "WC_GateCommons_ToUnicornWay";
        int checks = 0;

        foreach (string zone in new[] { path, path.ToLowerInvariant(), name, "" }) {
            Array mods = Mods(modType, (zone, tag, "IdleOpen"));
            object filtered = matching.Invoke(null, [mods, path, name, tag])!;
            Equal(1, ((IEnumerable)filtered).Cast<object>().Count());
            Equal("IdleOpen", resolve.Invoke(null, [filtered, "IdleClosed"]));
            Equal("IdleOpen", resolve.Invoke(null, [filtered, "IdleClosed"]));
            checks += 3;
        }

        foreach ((string zone, string objectTag) in new[] {
            ("WizardCity/WC_Streets/WC_Unicorn", tag),
            (path, "AnotherGate")
        }) {
            object filtered = matching.Invoke(null, [Mods(modType, (zone, objectTag, "IdleOpen")), path, name, tag])!;
            Equal(0, ((IEnumerable)filtered).Cast<object>().Count());
            Equal("IdleClosed", resolve.Invoke(null, [filtered, "IdleClosed"]));
            checks += 2;
        }

        foreach ((string state, string expected) in new[] {
            ("On", "IdleClosed"), ("Off", "Off"), ("", ""), ("IdleOpen", "IdleOpen")
        }) {
            object filtered = matching.Invoke(null, [Mods(modType, (path, tag, state)), path, name, tag])!;
            Equal(expected, resolve.Invoke(null, [filtered, "IdleClosed"]));
            checks++;
        }

        object empty = matching.Invoke(null, [null, path, name, tag])!;
        Equal("IdleClosed", resolve.Invoke(null, [empty, "IdleClosed"]));
        checks++;
        Console.WriteLine($"Passed {checks} saved object state checks, including gate reload and recreation.");
        return checks;
    }

    private static Array Mods(Type type, params (string Zone, string Tag, string State)[] values) {
        Array result = Array.CreateInstance(type, values.Length);
        for (int index = 0; index < values.Length; index++) {
            object mod = Activator.CreateInstance(type)!;
            Set(type, mod, "ZoneName", values[index].Zone);
            Set(type, mod, "ClientTag", values[index].Tag);
            Set(type, mod, "ModState", values[index].State);
            result.SetValue(mod, index);
        }

        return result;
    }

    private static void Set(Type type, object target, string property, string value)
        => type.GetProperty(property, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private static void Equal(object expected, object? actual) {
        if (!expected.Equals(actual)) {
            throw new InvalidDataException($"Expected {expected}, received {actual}.");
        }
    }
}
