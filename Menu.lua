Include("API/ControlClasses.lua");

local initialChoice = INITIAL_MODE;
local randomVendors = initialChoice >= 16;
local initialMode = initialChoice % 16;
local mode = ((initialMode - 1) % 3) + 1;
local shuffleCreatures = initialMode > 3;
local names = {"Shuffle", "Chaos", "Nightmare"};
local panel = nil;
local characterSelect = nil;
local events = {"WB_BUTTONUP", "GM_MOUSEBTNUP", "GM_MOUSEBTNDOWN", "GM_KEYUP"};

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
    while true do
        local event = GetEvent("", false);
        if event then
            Log("Spiral Shuffle input: " .. tostring(event.EventName) .. ":" .. tostring(event.Name) .. ":" .. tostring(event.Button) .. ":" .. tostring(event.Key));
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
            elseif event.Name == "shuffleNew" then
                if closeMenu(true) then
                    return;
                end
            elseif event.Name == "shuffleContinue" then
                closeMenu(false);
                return;
            end
        end
        Sleep(0.02);
    end
end
