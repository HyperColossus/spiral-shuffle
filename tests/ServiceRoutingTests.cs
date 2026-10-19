using System.Reflection;
using Akka.Actor;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.WizBang;
using Imlight.CoreLib.Game.Zone.Components;
using Imlight.CoreLib.WizardData.Models.Player;
using Type = System.Type;

internal static class ServiceRoutingTests {
    private const BindingFlags Internal = BindingFlags.Instance | BindingFlags.NonPublic;

    internal static int Run(Assembly assembly) {
        Type type = assembly.GetType("Imlight.CoreLib.Game.ServiceOptionRoutes")!;
        MethodInfo add = type.GetMethod("Add", Internal)!;
        MethodInfo resolve = type.GetMethod("TryResolve", Internal)!;
        FakeService quest = new("QuestUnderwayService");
        FakeService cards = new("TreasureShopService");
        FakeService gear = new("WizShoppingService");
        int checks = 0;
        foreach (FakeService[] order in new[] {
            new[] { quest, cards, gear }, new[] { quest, gear, cards },
            new[] { cards, quest, gear }, new[] { cards, gear, quest },
            new[] { gear, quest, cards }, new[] { gear, cards, quest }
        }) {
            object routes = Activator.CreateInstance(type, true)!;
            foreach (FakeService component in order) {
                add.Invoke(routes, [component, Options(component.ServiceName, 1)]);
            }

            foreach (FakeService component in order) {
                AssertRoute(resolve, routes, component.ServiceName, 0, component, 0);
                checks++;
            }
            AssertMissing(resolve, routes, cards.ServiceName, 1);
            AssertMissing(resolve, routes, "UnknownService", 0);
            checks += 2;
        }

        FakeService usage = new("Interact");
        FakeService chest = new("Interact");
        object firstPlayer = Activator.CreateInstance(type, true)!;
        add.Invoke(firstPlayer, [usage, Options("Interact", 2)]);
        add.Invoke(firstPlayer, [chest, Options("Interact", 1)]);
        AssertRoute(resolve, firstPlayer, "Interact", 0, usage, 0);
        AssertRoute(resolve, firstPlayer, "Interact", 1, usage, 1);
        AssertRoute(resolve, firstPlayer, "Interact", 2, chest, 0);
        AssertMissing(resolve, firstPlayer, "Interact", uint.MaxValue);
        object secondPlayer = Activator.CreateInstance(type, true)!;
        add.Invoke(secondPlayer, [usage, Options("Interact", 0)]);
        add.Invoke(secondPlayer, [chest, Options("Interact", 1)]);
        AssertRoute(resolve, secondPlayer, "Interact", 0, chest, 0);
        AssertRoute(resolve, firstPlayer, "Interact", 0, usage, 0);
        add.Invoke(secondPlayer, [gear, Options("AlternateShopService", 1)]);
        AssertRoute(resolve, secondPlayer, "AlternateShopService", 0, gear, 0);
        checks += 7;
        Console.WriteLine($"Passed {checks} NPC menu routing checks: quest and shop collisions, shared services, option offsets and separate snapshots.");
        return checks;
    }

    private static ServiceOptionBase[] Options(string name, int count)
        => Enumerable.Range(0, count).Select(_ => new ServiceOptionBase { m_serviceName = name }).ToArray();

    private static void AssertRoute(MethodInfo resolve, object routes, string service, uint index,
        IServiceComponent expected, uint expectedIndex) {
        object?[] args = [service, index, null, 0U];
        if (!(bool)resolve.Invoke(routes, args)! || !ReferenceEquals(args[2], expected) ||
            (uint)args[3]! != expectedIndex) {
            throw new InvalidDataException($"Incorrect route for {service}[{index}].");
        }
    }

    private static void AssertMissing(MethodInfo resolve, object routes, string service, uint index) {
        object?[] args = [service, index, null, 0U];
        if ((bool)resolve.Invoke(routes, args)!) {
            throw new InvalidDataException($"Unexpected route for {service}[{index}].");
        }
    }

    private sealed class FakeService(string name) : IServiceComponent {
        public string ServiceName => name;
        public string NpcIcon => "";
        public string NpcNameKey => "";
        public string NpcTextKey => "";
        public WizBangs WizBang => WizBangs.None;
        public string StateName => "";
        public string InteractWizBang => "";
        public string DisplayKey => "";

        public IEnumerable<ServiceOptionBase> GetServiceOptions(Wizard wizard) => [];

        public void OnServiceInteraction(IActorRef actor, Wizard wizard, CoreObject obj, uint index) { }
    }
}
