"""Connect personal vendor stock to shop displays and purchase validation."""
from collections.abc import Callable


def patch_vendor_hooks(replace: Callable[[str, str, str], None]) -> None:
    patch_service_routing(replace)
    replace("Game/Spells/SpellFactory.cs", "using System.Collections.Generic;",
            "using System.Collections.Generic;\nusing System.Linq;")
    replace("Game/Spells/SpellFactory.cs", "    public void DisposeStream() {", """    internal static IEnumerable<SpellTemplate> GetTreasureCardTemplates() {
        var registeredPaths = CoreObjectFactory.TemplateManifest.m_serializedTemplates
            .Select(location => (string)location.m_filename).ToHashSet(StringComparer.Ordinal);
        return s_spellTemplates.Where(entry => entry.Value.m_Treasure &&
                s_spellTemplatePaths[entry.Key].StartsWith("Spells/TreasureCards/", StringComparison.Ordinal) &&
                registeredPaths.Contains(s_spellTemplatePaths[entry.Key]))
            .Select(entry => entry.Value);
    }

    public void DisposeStream() {""")
    replace("Login/Services/CharacterService.cs", 'ScriptName = $"Randomizer/Menu{mode}.lua"',
            'ScriptName = $"Randomizer/Menu{mode + (Imlight.CoreLib.Game.VendorRandomization.IsEnabled(account.AccountId) ? 16 : 0)}.lua"')
    replace("Login/Services/CharacterService.cs", "Imlight.CoreLib.Game.RandomizerRuntime.TryReadChoice(message.Message, out var mode)",
            "Imlight.CoreLib.Game.RandomizerRuntime.TryReadOptions(message.Message, out var mode, out var vendors)")
    replace("Login/Services/CharacterService.cs", "            Imlight.CoreLib.Game.RandomizerRuntime.SetMode(account.AccountId, mode);",
            """            Imlight.CoreLib.Game.RandomizerRuntime.SetMode(account.AccountId, mode);
            Imlight.CoreLib.Game.VendorRandomization.SetEnabled(account.AccountId, vendors);""")
    path = "Game/Zone/Components/InteractVendorComponent.cs"
    replace(path, "private List<GID> _inventory;", "private List<GID> _inventory = [];")
    replace(path, "SendShopOfferings(playerActor);", "SendShopOfferings(playerActor, playerCharacter);")
    replace(path, """    public bool HasItem(GID itemGID)
        => _inventory.Any(x => x.MParts.TemplateId == itemGID.MParts.TemplateId);""", """    private List<GID> InventoryFor(Wizard wizard)
        => Imlight.CoreLib.Game.VendorRandomization.Items(wizard, Entity.ActiveGameObject.m_templateID, _inventory);

    public bool HasItem(GID itemGID, Wizard wizard)
        => InventoryFor(wizard).Any(x => x.MParts.TemplateId == itemGID.MParts.TemplateId);""")
    replace(path, "private void SendShopOfferings(IActorRef playerActor)",
            "private void SendShopOfferings(IActorRef playerActor, Wizard wizard)")
    replace(path, "m_shopList = _inventory,", "m_shopList = InventoryFor(wizard),")
    replace("Game/Services/ShopService.cs", "vendorComponent.HasItem((GID) message.ShopID)",
            "vendorComponent.HasItem((GID) message.ShopID, GetActiveWizard())")
    path = "Game/Zone/Components/InteractTreasureVendorComponent.cs"
    replace(path, "private List<TreasureCardEntry> _inventory;", "private List<TreasureCardEntry> _inventory = [];")
    replace(path, "SendTreasureShopOfferings(playerActor);", "SendTreasureShopOfferings(playerActor, playerCharacter);")
    replace(path, """    public bool HasSpell(string spellName)
        => _inventory.Any(x => x.SpellName == spellName);""", """    private List<TreasureCardEntry> InventoryFor(Wizard wizard)
        => Imlight.CoreLib.Game.VendorRandomization.Cards(wizard, Entity.ActiveGameObject.m_templateID, _inventory);

    public bool HasSpell(string spellName, Wizard wizard)
        => InventoryFor(wizard).Any(x => x.SpellName == spellName);""")
    replace(path, """    public bool HasSpell(uint spellHash)
        => _inventory.Any(x => StringHash.Compute(x.SpellName) == spellHash);""", """    public bool HasSpell(uint spellHash, Wizard wizard)
        => InventoryFor(wizard).Any(x => StringHash.Compute(x.SpellName) == spellHash);""")
    replace(path, """    public string GetSpellName(uint spellHash)
        => _inventory.Find(x => StringHash.Compute(x.SpellName) == spellHash)?.SpellName;""", """    public string GetSpellName(uint spellHash, Wizard wizard)
        => InventoryFor(wizard).Find(x => StringHash.Compute(x.SpellName) == spellHash)?.SpellName;""")
    replace(path, """    public int GetSpellPrice(string spellName) {
        var entry = _inventory.Find(x => x.SpellName == spellName);""", """    public int GetSpellPrice(string spellName, Wizard wizard) {
        var entry = InventoryFor(wizard).Find(x => x.SpellName == spellName);""")
    replace(path, """    public int GetSpellPrice(uint spellHash) {
        var spellName = GetSpellName(spellHash);
        return spellName != null ? GetSpellPrice(spellName) : 0;""", """    public int GetSpellPrice(uint spellHash, Wizard wizard) {
        var spellName = GetSpellName(spellHash, wizard);
        return spellName != null ? GetSpellPrice(spellName, wizard) : 0;""")
    replace(path, "private void SendTreasureShopOfferings(IActorRef playerActor)",
            "private void SendTreasureShopOfferings(IActorRef playerActor, Wizard wizard)")
    replace(path, "m_treasureSpellNames = _inventory.Select(x => (ByteString)x.SpellName).ToList(),",
            "m_treasureSpellNames = InventoryFor(wizard).Select(x => (ByteString)x.SpellName).ToList(),")
    replace("Game/Services/TreasureShopService.cs", "vendorComponent.HasSpell(message.TreasureCardID)",
            "vendorComponent.HasSpell(message.TreasureCardID, GetActiveWizard())")
    replace("Game/Services/TreasureShopService.cs", "vendorComponent.GetSpellPrice(message.TreasureCardID)",
            "vendorComponent.GetSpellPrice(message.TreasureCardID, playerWizard)")
    replace("Game/Services/TreasureShopService.cs", "vendorComponent.GetSpellName(message.TreasureCardID)",
            "vendorComponent.GetSpellName(message.TreasureCardID, playerWizard)")


def patch_service_routing(replace: Callable[[str, str, str], None]) -> None:
    path = "Game/Zone/Components/InteractServiceMementoComponent.cs"
    replace(path, """    private Dictionary<int, IServiceComponent> _optionIndexToComponent = [];
    private readonly Dictionary<IServiceComponent, int> _componentFirstOption = [];""", """    private readonly Dictionary<ulong, Imlight.CoreLib.Game.ServiceOptionRoutes> _playerOptionRoutes = [];""")
    replace(path, """    public override void OnPlayerLeave(IActorRef playerActor, ulong id) {
""", """    public override void OnPlayerLeave(IActorRef playerActor, ulong id) {
        _playerOptionRoutes.Remove(id);
""")
    replace(path, """        // Route to the component that owns this option index.
        // Using FirstOrDefault by ServiceName is unsafe when multiple components share
        // the same service name (e.g. InteractQuestSelectComponent shadowing WoodenChestComponent).
        if (!_optionIndexToComponent.TryGetValue((int) serviceIndex, out var serviceComponent)) {""", """        // The client numbers options within each service, rather than across the whole dialog.
        if (!_playerOptionRoutes.TryGetValue(playerObject.m_globalID.Full, out var routes) ||
            !routes.TryResolve(serviceName, serviceIndex, out var serviceComponent, out var localIndex)) {""")
    replace(path, """        // Call the service component's interaction method with the index of its own option.
        var localIndex = _componentFirstOption.TryGetValue(serviceComponent, out var first)
            ? serviceIndex - (uint) first
            : serviceIndex;
""", "")
    replace(path, """        // Get all service options and track which component owns each flat index.
        _optionIndexToComponent.Clear();
        _componentFirstOption.Clear();""", """        var routes = new Imlight.CoreLib.Game.ServiceOptionRoutes();""")
    replace(path, """            if (componentOptions.Count > 0) {
                _componentFirstOption[component] = allOptions.Count;
            }

            foreach (var option in componentOptions) {
                _optionIndexToComponent[allOptions.Count] = component;
                allOptions.Add(option);
            }""", """            routes.Add(component, componentOptions);
            allOptions.AddRange(componentOptions);""")
    replace(path, """        // Get UI overrides based on priority.
""", """        if (playerCharacter is not null) {
            _playerOptionRoutes[playerCharacter.GameObjectID] = routes;
        }

        // Get UI overrides based on priority.
""")
