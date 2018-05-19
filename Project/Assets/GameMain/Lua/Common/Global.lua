-- 辅助记录全局变量的名称是否被使用过

local _GlobalNames = { }

local function __innerDeclare(name, defaultValue)
    if not rawget(_G, name) then
        rawset(_G, name, defaultValue or false)
    else
        Log.Error("[Warning] The global variable " .. name .. " is already declared!")
    end
    _GlobalNames[name] = true
    return _G[name]
end

local function __innerDeclareIndex(tbl, key)
    if not _GlobalNames[key] then
        Log.Error("Attempt to access an undeclared global variable : " .. key, 2)
    end

    return nil
end

local function __innerDeclareNewindex(tbl, key, value)
    if not _GlobalNames[key] then
        Log.Error("Attempt to write an undeclared global variable : " .. key, 2)
    else
        rawset(tbl, key, value)
    end
end

local function __GLDeclare(name, defaultValue)
    local ok, ret = pcall(__innerDeclare, name, defaultValue)
    if not ok then
        Log.Error(debug.traceback(res, 2))
        return nil
    else
        return ret
    end
end

local function __isGLDeclared(name)
    if _GlobalNames[name] or rawget(_G, name) then
        return true
    else
        return false
    end
end

-- Set "GLDeclare" into global.
if (not __isGLDeclared("GLDeclare")) or (not GLDeclare) then
    __GLDeclare("GLDeclare", __GLDeclare)
end

-- Set "IsGLDeclared" into global.
if (not __isGLDeclared("IsGLDeclared")) or(not IsGLDeclared) then
    __GLDeclare("IsGLDeclared", __isGLDeclared)
end

setmetatable(_G,
{
    __index = function(tbl, key)
        local ok, res = pcall(__innerDeclareIndex, tbl, key)
        if not ok then
            Log.Error(debug.traceback(res, 2))
        end

        return nil
    end,

    __newindex = function(tbl, key, value)
        local ok, res = pcall(__innerDeclareNewindex, tbl, key, value)
        if not ok then
            Log.Error(debug.traceback(res, 2))
        end
    end
} )

return __GLDeclare


