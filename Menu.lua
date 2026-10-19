Include("API/ControlClasses.lua");

local initialChoice = INITIAL_MODE;
local randomVendors = initialChoice >= 16;
local initialMode = initialChoice % 16;
local mode = ((initialMode - 1) % 3) + 1;
local shuffleCreatures = initialMode > 3;
local dropSetting = -100;
local dropSettingsReady = false;
local dropRates = {10, 20, 30, 50, 75, 100, 150, 200, 300, 500, 750, 1000, 1500, 2000, 3000, 4000, 5000, 7500, 10000};
local names = {"Shuffle", "Chaos", "Nightmare"};
local panel = nil;
local characterSelect = nil;
local events = {"WB_BUTTONUP", "GM_MOUSEBTNUP", "GM_MOUSEBTNDOWN", "GM_KEYUP", "MSG_MG2_REWARDS"};

local function fixedDrops()
    return math.abs(dropSetting) >= 100000;
end

local function updateDropSettings()
    local label = dropSettingsReady and (dropSetting > 0 and "ON" or "OFF") or "Loading";
    panel.FindNamedWindow("shuffleSpellDrops").SetLabel("Spell Drops: " .. label);
    panel.FindNamedWindow("shuffleDropInput").SetText(tostring((math.abs(dropSetting) % 100000) / 100));
    panel.FindNamedWindow("shuffleDropMode").SetLabel(fixedDrops() and "Odds: Fixed per enemy" or "Odds: Random per enemy");
    local hint = fixedDrops() and "Same chance for every enemy." or "Random odds up to your cap.";
    panel.FindNamedWindow("shuffleDropHint").SetText("<center>" .. hint .. "<br>0.1%-100%. Type a value, then Set.</center>");
end

local function saveDropSettings()
    updateDropSettings();
    SendProcessMessage("MSG_MG2_REWARDS", {score = dropSetting, gameName = "SpiralSpellDrops"});
end

local function adjustDropRate(direction)
    local rate = math.abs(dropSetting) % 100000;
    if direction > 0 then
        for index = 1, #dropRates do
            if dropRates[index] > rate then
                rate = dropRates[index];
                break;
            end
        end
    else
        for index = #dropRates, 1, -1 do
            if dropRates[index] < rate then
                rate = dropRates[index];
                break;
            end
        end
    end
    rate = rate + (fixedDrops() and 100000 or 0);
    dropSetting = dropSetting > 0 and rate or -rate;
    saveDropSettings();
end

local function applyDropRate()
    local rate = tonumber(panel.FindNamedWindow("shuffleDropInput").GetText());
    if not rate or rate < 0.1 or rate > 100 then
        panel.FindNamedWindow("shuffleDropHint").SetText("<center>Enter a percentage from 0.1 to 100.</center>");
        return false;
    end
    local value = math.floor(rate * 100 + 0.5) + (fixedDrops() and 100000 or 0);
    dropSetting = dropSetting > 0 and value or -value;
    saveDropSettings();
    return true;
end

local function waitForCharacterSelect()
    local root = OpenClass(GetRootWindow());
    for attempt = 1, 480 do
        local window = root.FindNamedWindow("mainWindow");
        local button = window and window.FindNamedWindow("btnNew");
        if window and button and window.IsVisible() and button.IsVisible() then
            return window;
        end
        Sleep(0.25);
    end
    return nil;
end

local function selectMode(value)
    mode = value;
    for index = 1, 3 do
        local button = panel.FindNamedWindow("shuffleMode" .. index);
        local suffix = index == mode and " *" or "";
        button.SetLabel(names[index] .. suffix);
    end
    panel.FindNamedWindow("shuffleStatus").SetText("<center>Selected: " .. names[mode] .. " | Seed: SPIRAL-101</center>");
    local creatureLabel = shuffleCreatures and "Creature Shuffle + Stats" or "Creature Stats Only";
    local creatureDescription = shuffleCreatures
        and "Names, models, schools and decks shuffle. Each creature gets fresh stat rolls."
        or "A Lost Soul stays a Lost Soul. Each creature gets fresh stat rolls.";
    panel.FindNamedWindow("shuffleCreatures").SetLabel(creatureLabel);
    panel.FindNamedWindow("shuffleCreatureDescription").SetText("<center>" .. creatureDescription .. "</center>");
    panel.FindNamedWindow("shuffleVendors").SetLabel("Randomize Vendors: " .. (randomVendors and "ON" or "OFF"));
    SendProcessMessage("MSG_MG2_REWARDS", {score = mode + (shuffleCreatures and 3 or 0) + (randomVendors and 16 or 0), gameName = "SpiralShuffle"});
end

local function closeMenu(createWizard)
    if dropSettingsReady and not applyDropRate() then return false; end
    local button = characterSelect.FindNamedWindow("btnNew");
    if createWizard and (not button or button.Greyed()) then
        panel.FindNamedWindow("shuffleStatus").SetText("<center>No empty wizard slot is available.</center>");
        return false;
    end
    panel.ShowWindow(false);
    panel.DetachSelf();
    for _, name in ipairs(events) do
        UnregisterEvent(name);
    end
    if createWizard then
        button.SetButtonDown(true);
        Sleep(0.1);
        button.SetButtonDown(false);
    end
    Kill(GetProcessID());
    return true;
end

function main()
    characterSelect = waitForCharacterSelect();
    if not characterSelect then
        Log("Spiral Shuffle: character selection was not ready. Menu skipped.");
        Kill(GetProcessID());
        return;
    end
    for _, name in ipairs(events) do
        RegisterEvent(name);
    end
    panel = CreateGUIClass("RandomizerMenu.gui", false);
    if not panel then
        Log("Spiral Shuffle: native menu failed to load.");
        Kill(GetProcessID());
        return;
    end
    panel.DetachSelf();
    characterSelect.AttachWindow(panel);
    panel.ShowWindow(true);
    Log("Spiral Shuffle: menu attached to character selection.");
    selectMode(mode);
    updateDropSettings();
    SendProcessMessage("MSG_MG2_REWARDS", {score = 0, gameName = "SpiralSpellDrops"});
    local settingsWait = 0;
    while true do
        local event = GetEvent("", false);
        if event then
            Log("Spiral Shuffle input: " .. tostring(event.EventName) .. ":" .. tostring(event.Name) .. ":" .. tostring(event.Button) .. ":" .. tostring(event.Key));
        end
        if event and event.MsgName == "MSG_MG2_REWARDS" and event.gameName == "SpiralSpellDrops" then
            local setting = tonumber(event.score);
            local absolute = setting and math.abs(setting) or 0;
            if setting and ((absolute >= 10 and absolute <= 10000) or (absolute >= 100010 and absolute <= 110000)) then
                dropSetting = setting;
                dropSettingsReady = true;
                updateDropSettings();
            end
        end
        if event and event.EventName == "WB_BUTTONUP" then
            for index = 1, 3 do
                if event.Name == "shuffleMode" .. index then
                    selectMode(index);
                end
            end
            if event.Name == "shuffleCreatures" then
                shuffleCreatures = not shuffleCreatures;
                selectMode(mode);
            elseif event.Name == "shuffleVendors" then
                randomVendors = not randomVendors;
                selectMode(mode);
            elseif event.Name == "shuffleSpellDrops" and dropSettingsReady then
                dropSetting = -dropSetting;
                saveDropSettings();
            elseif event.Name == "shuffleDropLess" and dropSettingsReady then
                adjustDropRate(-1);
            elseif event.Name == "shuffleDropMore" and dropSettingsReady then
                adjustDropRate(1);
            elseif event.Name == "shuffleDropMode" and dropSettingsReady then
                if applyDropRate() then
                    local value = math.abs(dropSetting) % 100000;
                    value = value + (fixedDrops() and 0 or 100000);
                    dropSetting = dropSetting > 0 and value or -value;
                    saveDropSettings();
                end
            elseif event.Name == "shuffleDropApply" and dropSettingsReady then
                applyDropRate();
            elseif event.Name == "shuffleNew" then
                if closeMenu(true) then
                    return;
                end
            elseif event.Name == "shuffleContinue" then
                closeMenu(false);
                return;
            end
        end
        settingsWait = settingsWait + 1;
        if not dropSettingsReady and settingsWait % 50 == 0 then
            SendProcessMessage("MSG_MG2_REWARDS", {score = 0, gameName = "SpiralSpellDrops"});
        end
        Sleep(0.02);
    end
end
