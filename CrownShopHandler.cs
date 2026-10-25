/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using Imcodec.IO;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Imlight.CoreLib.Game.CrownShop;

public static class CrownShopHandler {
    private static bool s_enabled = true;
    private static uint s_wishlistMaxSize = 30;
    private static uint s_wishlistSBExpansionSize = 10;
    private static bool s_autoReloadOnChanges = false;
    public static uint s_maxBuyCount = 100;
    private static string s_crownShopConfigPath = "./spiraldb-cache/CrownShop";

    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static FileSystemWatcher? s_watcher;
    private static DateTime s_lastReloadTime = DateTime.MinValue;
    private static readonly object s_reloadLock = new();

    private static List<CrownShopTabConfig> s_crownShopTabs = new();
    private static List<CrownShopCategoryConfig> s_crownShopCategories = new();
    private static Dictionary<ulong, CrownShopItem> s_items = new();
    private static ByteString s_serializedCrownShopData = ByteString.Empty;
    private static bool s_isLoaded = false;

    public static void Load() {
        try {
            var enabledStr = ConfigurationManager.GetSetting("CrownShop.Enabled");
            s_enabled = string.IsNullOrEmpty(enabledStr) ? true : ConfigurationManager.Settings["CrownShop.Enabled"].AsBool(true);
            s_wishlistMaxSize = ConfigurationManager.Settings["CrownShop.WishlistMaxSize"].AsUInt(30);
            s_wishlistSBExpansionSize = ConfigurationManager.Settings["CrownShop.WishlistSBExpansionSize"].AsUInt(10);
            s_autoReloadOnChanges = ConfigurationManager.Settings["CrownShop.AutoReloadOnChanges"].AsBool(false);
            s_maxBuyCount = ConfigurationManager.Settings["CrownShop.MaxBuyCount"].AsUInt(100);
            var dbPath = ConfigurationManager.Settings["Database.SpiralDBLocalPath"].AsString();
            if (!string.IsNullOrEmpty(dbPath)) {
                s_crownShopConfigPath = dbPath + "/CrownShop";
            }
        } catch {
            // ConfigurationManager not initialized; using defaults
        }

        if (!s_enabled) {
            Logger.Information("CrownShop not initializing (disabled in config)");
            return;
        }

        var configPath = string.IsNullOrEmpty(s_crownShopConfigPath) ? "./spiraldb-cache/CrownShop" : s_crownShopConfigPath;
        var basePath = Path.GetFullPath(configPath);
        if (!Directory.Exists(basePath)) {
            var fallback = Path.GetFullPath("/home/hypercolossus/Revive101/Imlight/spiraldb-cache/CrownShop");
            if (Directory.Exists(fallback)) {
                basePath = fallback;
            } else {
                Logger.Warning("CrownShop configuration directory not found: {0}", Logger.Args(basePath));
                return;
            }
        }

        lock (s_reloadLock) {
            Logger.Information("Loading CrownShop configuration from: {0}", Logger.Args(basePath));

            var loadedTabs = new List<CrownShopTabConfig>();
            var filePaths = Directory.GetFiles(basePath, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var path in filePaths) {
                if (!File.Exists(path)) {
                    continue;
                }

                try {
                    var json = File.ReadAllText(path);
                    var tab = JsonSerializer.Deserialize<CrownShopTabConfig>(json, s_jsonSerializerOptions);

                    if (tab == null) {
                        Logger.Warning("CrownShop tab configuration file is empty or invalid: {0}", Logger.Args(path));
                        continue;
                    }

                    loadedTabs.Add(tab);
                }
                catch (Exception e) {
                    Logger.Warning("Failed to load CrownShop tab configuration file: {0}. Error: {1}", Logger.Args(path, e.Message));
                }
            }

            s_crownShopTabs = loadedTabs.OrderBy(t => t.TabId).ToList();
            s_crownShopCategories = s_crownShopTabs.SelectMany(t => t.Categories).ToList();

            var layoutTabs = new List<CrownShopCategoryMenu>();
            var layoutCategories = new List<CrownShopCategory>();
            var crownShopItems = new Dictionary<ulong, CrownShopItem>();

            foreach (var tab in s_crownShopTabs) {
                var layoutTab = new CrownShopCategoryMenu() {
                    m_ID = tab.TabId,
                    m_name = tab.Name,
                    m_iconResource = tab.Icon,
                    m_categoryIDs = tab.CategoryIds,
                    m_tags = tab.Tags,
                    m_description = tab.Description
                };
                layoutTabs.Add(layoutTab);

                foreach (var category in tab.Categories) {
                    var layoutCategory = new CrownShopCategory() {
                        m_ID = category.CategoryId,
                        m_parentTabID = category.ParentTabId,
                        m_name = category.Name,
                        m_description = category.Description,
                        m_iconResource = category.Icon,
                        m_tags = category.Tags,
                        m_allowMultipleBuy = category.Flags.AllowMultipleBuy,
                        m_forceDisallowMultipleBuy = category.Flags.ForceDisallowMultipleBuy,
                        m_dontFilterOwnedRecoItems = category.Flags.DontFilterOwnedRecoItems,
                        m_isHousesCategory = category.Flags.IsHousesCategory,
                        m_isEverythingCategory = category.Flags.IsEverythingCategory,
                        m_isGroupElixirsCategory = category.Flags.IsGroupElixirsCategory
                    };
                    layoutCategories.Add(layoutCategory);
                }

                foreach (var item in tab.Items) {
                    var displayPriority = item.DisplayPriority != null && item.DisplayPriority.Count > 0
                        ? string.Join(",", item.DisplayPriority.Select((p, idx) => $"{p}:{idx}"))
                        : "0:0";

                    var crownShopItem = new CrownShopItem() {
                        m_itemTemplateId = item.TemplateId,
                        m_itemFlags = item.ItemFlags,
                        m_goldCost = item.GoldCost,
                        m_strikethruGold = item.StrikethruGold,
                        m_crownsCost = item.CrownsCost,
                        m_strikethruCrowns = item.StrikethruCrowns,
                        m_ticketCost = item.TicketCost,
                        m_displayPriority = displayPriority,
                        m_combatOnly = item.CrownShopItemFlags.CombatOnly,
                        m_noGift = item.CrownShopItemFlags.NoGift,
                        m_recommendIfOwned = item.CrownShopItemFlags.RecommendIfOwned,
                        m_saleID = 0,
                        m_description = "",
                        m_segReqsPoolsStatements = [],
                        m_segReqsStatement = ""
                    };

                    crownShopItems[item.TemplateId] = crownShopItem;
                }
            }

            s_items = crownShopItems;

            var crownShopLayout = new CrownShopLayout() {
                m_tabs = layoutTabs,
                m_categories = layoutCategories
            };

            var crownShopData = new CrownShopData() {
                m_items = crownShopItems.Values.ToList(),
                m_crownShopLayout = crownShopLayout,
                m_wishlistMaxSize = (int) s_wishlistMaxSize,
                m_wishlistSBExpansionSize = (int) s_wishlistSBExpansionSize,
                m_crownShopSegReqsSummary = new(),
                m_recomendedItems = new LevelData()
            };

            var serializer = new ObjectSerializer(
                Versionable: false,
                Behaviors: SerializerFlags.SerializeFlags | SerializerFlags.Compress
            );

            var propertyFlags = PropertyFlags.Prop_Save | PropertyFlags.Prop_Public;
            if (!serializer.Serialize(crownShopData, propertyFlags, out var serializedData)) {
                Logger.Error("Failed to serialize CrownShopData");
                return;
            }

            s_serializedCrownShopData = serializedData;
            s_isLoaded = true;

            Logger.Information("CrownShop successfully initialized: {0} tabs, {1} categories, {2} items.",
                Logger.Args(s_crownShopTabs.Count, s_crownShopCategories.Count, s_items.Count));

            if (s_autoReloadOnChanges && s_watcher == null) {
                SetupAutoReloadWatcher(basePath);
            }
        }
    }

    private static void SetupAutoReloadWatcher(string basePath) {
        try {
            s_watcher = new FileSystemWatcher(basePath, "*.json") {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            s_watcher.Changed += OnConfigFileChanged;
            s_watcher.Created += OnConfigFileChanged;
            s_watcher.Renamed += (s, e) => OnConfigFileChanged(s, e);

            Logger.Information("CrownShop: Live hot-reload watcher enabled for {0}.", Logger.Args(basePath));
        } catch (Exception e) {
            Logger.Warning("Could not enable CrownShop auto-reload watcher: {0}", Logger.Args(e.Message));
        }
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e) {
        if ((DateTime.UtcNow - s_lastReloadTime).TotalMilliseconds < 500) {
            return;
        }

        s_lastReloadTime = DateTime.UtcNow;
        Logger.Information("CrownShop file changed. Reloading...");
        Load();
    }

    public static ByteString GetCrownShopData() {
        if (!s_isLoaded || s_serializedCrownShopData.IsEmpty) {
            Load();
        }
        return s_serializedCrownShopData;
    }

    public static List<CrownShopItem> GetCrownShopItems() => s_items.Values.ToList();

    public static bool TryGetCrownShopItem(ulong id, out CrownShopItem item)
        => s_items.TryGetValue(id, out item);
}
