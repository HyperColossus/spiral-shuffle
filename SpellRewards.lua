Include("API/ControlClasses.lua");

local WIDTH = 630;
local HEIGHT = 400;
local panel = nil;
local root = nil;
local card = nil;
local pending = nil;
local displayedToken = nil;
local dismissedToken = nil;
local inCombat = false;
local serverCombat = false;
local readyAt = 0;
local openedAt = nil;
local tick = 0;
local names = {"Shuffle", "Chaos", "Nightmare"};

local function split(value)
    local fields = {};
    for part in string.gmatch(value .. "|", "(.-)|") do
        table.insert(fields, part);
    end
    return fields;
end

local function decode(value)
    return string.gsub(value, "%%(%x%x)", function(hex) return string.char(tonumber(hex, 16)); end);
end

local function safeText(value)
    return string.gsub(string.gsub(value, "<", "&lt;"), ">", "&gt;");
end

local function hide()
    panel.ShowWindow(false);
    openedAt = nil;
end

local function dismiss()
    if not pending or displayedToken ~= pending.token then return; end
    dismissedToken = pending.token;
    SendProcessMessage("MSG_MG2_REWARDS", {score = dismissedToken, gameName = "SpellRewardUI"});
    pending = nil;
    displayedToken = nil;
    hide();
end

local function receive(payload)
    if payload == "SpellRewardCombat" then
        serverCombat = true;
        hide();
        return;
    elseif payload == "SpellRewardEmpty" then
        serverCombat = false;
        return;
    end
    local fields = split(payload);
    if fields[1] ~= "SpellReward" or #fields ~= 10 then return; end
    serverCombat = false;
    local token = tonumber(fields[2]);
    if not token or token == dismissedToken then return; end
    pending = {token = token, id = tonumber(fields[3]), name = decode(fields[4]), school = decode(fields[5]),
        cost = decode(fields[6]), accuracy = tonumber(fields[7]), difficulty = tonumber(fields[8]),
        number = tonumber(fields[9]), total = tonumber(fields[10])};
end

local function showReward()
    if not pending or inCombat or serverCombat or tick < readyAt then return; end
    if displayedToken ~= pending.token then
        if card then card.DetachSelf(); end
        card = CreateGUIClass("Randomizer/SpellDrop" .. pending.id .. ".gui", false);
        if card then
            card.DetachSelf();
            panel.FindNamedWindow("rewardCardHolder").AttachWindow(card);
            card.ShowWindow(true);
        end
        local name = GetString(pending.name);
        panel.FindNamedWindow("rewardName").SetText(safeText(name));
        panel.FindNamedWindow("rewardSchool").SetText(safeText(pending.school) .. " spell");
        panel.FindNamedWindow("rewardCost").SetText("Pips: " .. safeText(pending.cost));
        panel.FindNamedWindow("rewardAccuracy").SetText("Accuracy: " .. pending.accuracy .. "%");
        panel.FindNamedWindow("rewardRoll").SetText(names[pending.difficulty] .. " spell roll");
        panel.FindNamedWindow("rewardCount").SetText(pending.total > 1 and (pending.number .. " / " .. pending.total) or "");
        panel.FindNamedWindow("rewardDismiss").SetLabel(pending.number < pending.total and "Next spell" or "Got it!");
        displayedToken = pending.token;
        PlaySound("GUI/SpellSelect.wav", 0.75, 0, 0);
    end
    if not openedAt then openedAt = tick; end
    local progress = math.min(1, (tick - openedAt + 1) / 9);
    local ease = 1 - (1 - progress) * (1 - progress) * (1 - progress);
    panel.m_fAlpha = ease;
    panel.m_fTargetAlpha = ease;
    panel.SetLocation((GetScreenWidth() - WIDTH) / 2, (GetScreenHeight() - HEIGHT) / 2 + (1 - ease) * 22);
    panel.ShowWindow(true);
end

function main()
    for _, name in ipairs({"MSG_MG2_REWARDS", "OnEnterCombat", "OnLeaveCombat", "WB_BUTTONUP"}) do RegisterEvent(name); end
    root = OpenClass(GetRootWindow());
    local previous = root.FindNamedWindow("RandomizerSpellReward");
    if previous then previous.DetachSelf(); end
    panel = CreateGUIClass("RandomizerSpellReward.gui", false);
    if not panel then return; end
    panel.DetachSelf();
    root.AttachWindow(panel);
    while true do
        for index = 1, 128 do
            local event = GetEvent("", false);
            if not event then break; end
            if event.EventName == "OnEnterCombat" then
                inCombat = true;
                hide();
            elseif event.EventName == "OnLeaveCombat" then
                inCombat = false;
                readyAt = tick + 27;
            elseif event.EventName == "WB_BUTTONUP" and event.Name == "rewardDismiss" then
                dismiss();
            elseif event.MsgName == "MSG_MG2_REWARDS" and event.gameName then
                receive(event.gameName);
            end
        end
        if tick % 15 == 0 then SendProcessMessage("MSG_MG2_REWARDS", {score = 0, gameName = "SpellRewardUI"}); end
        showReward();
        tick = tick + 1;
        Sleep(0.03);
    end
end
