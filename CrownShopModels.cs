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
using System.Collections.Generic;

namespace Imlight.CoreLib.Game.CrownShop;

public class CrownShopTabConfig {
    public int TabId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<int> CategoryIds { get; set; } = new();
    public string Tags { get; set; } = "Seperate";
    public string Description { get; set; } = "0";
    public List<CrownShopCategoryConfig> Categories { get; set; } = new();
    public List<CrownShopItemConfig> Items { get; set; } = new();
}

public class CrownShopCategoryConfig {
    public int CategoryId { get; set; }
    public int ParentTabId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = "0";
    public string Icon { get; set; } = string.Empty;
    public string Tags { get; set; } = "None";
    public CrownShopCategoryFlags Flags { get; set; } = new();
}

public class CrownShopCategoryFlags {
    public bool AllowMultipleBuy { get; set; } = true;
    public bool ForceDisallowMultipleBuy { get; set; } = false;
    public bool DontFilterOwnedRecoItems { get; set; } = false;
    public bool IsHousesCategory { get; set; } = false;
    public bool IsEverythingCategory { get; set; } = false;
    public bool IsGroupElixirsCategory { get; set; } = false;
}

public class CrownShopItemConfig {
    public ulong TemplateId { get; set; }
    public int ItemFlags { get; set; } = 0;
    public int GoldCost { get; set; } = 0;
    public int StrikethruGold { get; set; } = 0;
    public int CrownsCost { get; set; } = 0;
    public int StrikethruCrowns { get; set; } = 0;
    public int TicketCost { get; set; } = 0;
    public List<uint> DisplayPriority { get; set; } = new() { 0 };
    public CrownShopItemFlags CrownShopItemFlags { get; set; } = new();
}

public class CrownShopItemFlags {
    public bool CombatOnly { get; set; } = false;
    public bool RecommendIfOwned { get; set; } = false;
    public bool NoGift { get; set; } = false;
}

[Flags]
public enum CrownShopItemFlagsBits {
    None = 0,
    Hot = 1 << 0,
    Featured = 1 << 1,
    Sale = 1 << 3,
    New = 1 << 4,
}
