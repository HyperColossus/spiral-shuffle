Include("API/ControlClasses.lua");

local PANEL_WIDTH = 410;
local PANEL_HEIGHT = 370;
local enemies = {};
local mouseX = -10000;
local mouseY = -10000;
local panel = nil;
local root = nil;
local tick = 0;
local planningCamera = nil;
local displayedEnemy = nil;
local hoveredId = nil;
local schoolRow = 1;
local displayedSchool = nil;

local function split(value, separator)
    local result = {};
    for part in string.gmatch(value .. separator, "(.-)" .. separator) do
        table.insert(result, part);
    end
    return result;
end

local function receiveSnapshot(payload)
    local fields = split(payload, "|");
    if fields[1] == "Planning" and #fields == 6 then
        planningCamera = {tonumber(fields[2]), tonumber(fields[3]), tonumber(fields[4]),
            tonumber(fields[5]), tonumber(fields[6])};
        return;
    end
    if fields[1] ~= "Snapshot" or #fields < 12 then
        return;
    end
    local enemy = {id = fields[2], x = tonumber(fields[3]), y = tonumber(fields[4]),
        z = tonumber(fields[5]), name = fields[6], school = fields[7], level = fields[8],
        health = tonumber(fields[9]), maxHealth = tonumber(fields[10]), rows = {},
        extras = split(fields[12], ","), updated = tick};
    if not enemy.x or not enemy.y or not enemy.z or not enemy.health then
        return;
    end
    for _, row in ipairs(split(fields[11], ";")) do
        table.insert(enemy.rows, split(row, ","));
    end
    enemies[enemy.id] = enemy;
end

local function handleEvent(event)
    if event.EventName == "GM_MOUSEMOVE" then
        mouseX = tonumber(event.X) or mouseX;
        mouseY = tonumber(event.Y) or mouseY;
    elseif event.EventName == "GM_MOUSEWHEEL" and hoveredId then
        schoolRow = schoolRow % 8 + 1;
    elseif event.MsgName == "MSG_MG2_REWARDS" and event.gameName then
        receiveSnapshot(event.gameName);
    end
end

local function pollEvents()
    for index = 1, 256 do
        local event = GetEvent("", false);
        if not event then
            return;
        end
        handleEvent(event);
    end
end

local function cameraProjection(width, height)
    if not planningCamera then
        return nil;
    end
    -- The native FX_Cam_Planning.nif camera stays fixed while choosing cards.
    local center = planningCamera;
    local sinYaw, cosYaw = math.sin(center[4]), math.cos(center[4]);
    local facing = center[5];
    local cameraX = center[1] + facing * (0.904 * cosYaw - 1059.052 * sinYaw);
    local cameraY = center[2] + facing * (-0.904 * sinYaw - 1059.052 * cosYaw);
    local cameraZ = center[3] + 625.280;
    local focal = height / (2 * 0.4821428657);
    return function(x, y, z)
        local dx, dy, dz = x - cameraX, y - cameraY, z - cameraZ;
        local horizontal = facing * (sinYaw * dx + cosYaw * dy);
        local depth = 0.853 * horizontal - 0.522 * dz;
        if depth < 25 then
            return nil;
        end
        local right = facing * (cosYaw * dx - sinYaw * dy);
        local up = 0.522 * horizontal + 0.853 * dz;
        return width / 2 + right * focal / depth, height / 2 - up * focal / depth, focal / depth;
    end;
end

local function updateText(enemy)
    panel.FindNamedWindow("enemyTitle").SetText("<string;" .. enemy.name .. ">");
    panel.FindNamedWindow("enemyMeta").SetText(enemy.school .. " | Level " .. enemy.level);
    panel.FindNamedWindow("enemyHealth").SetText(enemy.health .. " / " .. enemy.maxHealth);
    local row = enemy.rows[schoolRow] or {};
    local function value(index)
        return row[index] or "0";
    end
    panel.FindNamedWindow("enemySchool").SetText((row[1] or enemy.school) .. " school stats");
    local values = {value(2) .. "%  +" .. value(3) .. " flat", value(4) .. "%  +" .. value(5) .. " flat",
        value(6) .. "%", value(7) .. "%", value(8) .. " (" .. value(10) .. "%)",
        value(9) .. " (" .. value(11) .. "%)"};
    for index, text in ipairs(values) do
        panel.FindNamedWindow("enemyValue" .. index).SetText(text);
    end
    local extras = enemy.extras;
    panel.FindNamedWindow("enemyPips").SetText("Power pips " .. (extras[1] or "0") .. "% | Stun resist " .. (extras[2] or "0") .. "%");
    panel.FindNamedWindow("enemyExtras").SetText("Healing " .. (extras[3] or "0") .. "/" .. (extras[4] or "0") ..
        "% | Shadow " .. (extras[5] or "0"));
end

local function updatePanel()
    local planning = OpenClass(FindNamedWindow("PlanningPhase"));
    if not planning or not planning.IsVisible() then
        hoveredId = nil;
        panel.ShowWindow(false);
        return;
    end
    local width, height = root.GetRight(), root.GetBottom();
    local project = cameraProjection(width, height);
    if not project then
        panel.ShowWindow(false);
        return;
    end
    local selected, bestDistance, anchorX, anchorY, radius;
    for id, enemy in pairs(enemies) do
        if tick - enemy.updated > 40 or enemy.health <= 0 then
            enemies[id] = nil;
        else
            local footX, footY, scale = project(enemy.x, enemy.y, enemy.z);
            local headX, headY = project(enemy.x, enemy.y, enemy.z + 180);
            if footX and headX then
                local bodyWidth = math.max(28, math.min(150, scale * 80));
                local centerX = (footX + headX) / 2;
                local minY, maxY = math.min(headY, footY) - 14, math.max(headY, footY) + 14;
                if math.abs(mouseX - centerX) <= bodyWidth and mouseY >= minY and mouseY <= maxY then
                    local distance = math.abs(mouseX - centerX);
                    if not bestDistance or distance < bestDistance then
                        selected, bestDistance = enemy, distance;
                        anchorX, anchorY, radius = centerX, footY, bodyWidth;
                    end
                end
            end
        end
    end
    if not selected then
        hoveredId = nil;
        panel.ShowWindow(false);
        return;
    end
    if hoveredId ~= selected.id then
        hoveredId = selected.id;
        schoolRow = 1;
        for index, row in ipairs(selected.rows) do
            if row[1] == selected.school then
                schoolRow = index;
            end
        end
    end
    if displayedEnemy ~= selected or displayedSchool ~= schoolRow then
        updateText(selected);
        displayedEnemy = selected;
        displayedSchool = schoolRow;
    end
    local left = anchorX + radius + 18;
    if left + PANEL_WIDTH > width - 12 then
        left = anchorX - radius - PANEL_WIDTH - 18;
    end
    left = math.max(12, math.min(width - PANEL_WIDTH - 12, left));
    local top = math.max(12, math.min(height - PANEL_HEIGHT - 12, anchorY - PANEL_HEIGHT));
    panel.SetLocation(left, top);
    panel.ShowWindow(true);
end

function main()
    RegisterEvent("GM_MOUSEMOVE");
    RegisterEvent("GM_MOUSEWHEEL");
    RegisterEvent("MSG_MG2_REWARDS");
    root = OpenClass(GetRootWindow());
    local previous = root.FindNamedWindow("RandomizerEnemyInspector");
    if previous then
        previous.DetachSelf();
    end
    panel = CreateGUIClass("RandomizerEnemyInspector.gui", false);
    Sleep(0.1);
    if not panel then
        return;
    end
    panel.DetachSelf();
    root.AttachWindow(panel);
    while true do
        pollEvents();
        if tick % 10 == 0 then
            SendProcessMessage("MSG_MG2_REWARDS", {score = 0, gameName = "Inspect"});
        end
        updatePanel();
        tick = tick + 1;
        Sleep(0.05);
    end
end
