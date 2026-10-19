using System.Collections.Generic;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.Zone.Components;

namespace Imlight.CoreLib.Game;

internal sealed class ServiceOptionRoutes {
    private readonly Dictionary<(string Service, uint Index), (IServiceComponent Component, uint LocalIndex)> _routes = [];
    private readonly Dictionary<string, uint> _counts = [];

    internal void Add(IServiceComponent component, IReadOnlyList<ServiceOptionBase> options) {
        for (int localIndex = 0; localIndex < options.Count; localIndex++) {
            string service = options[localIndex].m_serviceName;
            uint serviceIndex = _counts.GetValueOrDefault(service);
            _routes.Add((service, serviceIndex), (component, (uint)localIndex));
            _counts[service] = serviceIndex + 1;
        }
    }

    internal bool TryResolve(string service, uint index, out IServiceComponent component, out uint localIndex) {
        if (_routes.TryGetValue((service, index), out var route)) {
            component = route.Component;
            localIndex = route.LocalIndex;
            return true;
        }

        component = null;
        localIndex = 0;
        return false;
    }
}
