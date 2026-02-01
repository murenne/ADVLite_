---------------------------------------------------------------------------------------------------
-- System.lua - 系统工具函数库
-- 职责：提供时间管理、错误追踪、数据操作等基础工具函数
-- 这个文件是 ADV 系统的"工具箱"，被所有 Lua 脚本广泛使用
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
-- 系统
-- 通用参数：用于临时存储整数值，可在不同函数间传递数据
intParam1 = 0 -- 通用整数参数1
intParam2 = 0 -- 通用整数参数2
intParam3 = 0 -- 通用整数参数3

reservedDeltaTime = 0 -- 保留的帧时间（由 C# 传入，单位：秒）

---------------------------------------------------------------------------------------------------
-- 错误追踪
-- 用于记录 Lua 脚本错误的位置，方便调试

errorFile = "" -- 错误发生的文件名
errorLine = 0 -- 错误发生的行号

-- 功能：记录错误位置
-- 参数：debugInfo - Lua 的 debug.getinfo() 返回的调试信息
-- 用途：在错误处理时调用，记录错误发生的位置
SetErrorPosition = function(debugInfo)

    -- 从完整路径中提取文件名（去掉路径部分）
    errorFile = debugInfo.source:match("[^/]*$")
    errorLine = debugInfo.currentline
end

---------------------------------------------------------------------------------------------------
-- 时间系统
-- Lua 层的时间管理系统，用于计算动画、延迟等时间相关功能

Time_PrevTime = 0 -- 上一帧的时间戳
Time_DeltaTime = 0 -- 当前帧与上一帧的时间差（帧时间）
Time_NowTime = 0 -- 当前时间戳（累计时间）

---------------------------------------------------------------------------------------------------
-- 初始化（内部调用）
-- 功能：初始化时间系统
-- 调用者：Main.lua 的 StartCoroutine() 函数
-- 调用时机：协程启动时

InitializeSystem = function()

    Time_PrevTime = 0
    Time_DeltaTime = 0
    Time_NowTime = 1

end

---------------------------------------------------------------------------------------------------
-- 帧更新（内部调用）
-- 功能：每帧更新时间系统
-- 调用者：Main.lua 的 PauseCoroutine() 函数
-- 调用时机：协程恢复时（每帧）

UpdateFrame = function()

    Time_PrevTime = Time_NowTime
    Time_DeltaTime = reservedDeltaTime -- 使用 C# 传入的帧时间
    Time_NowTime = Time_NowTime + Time_DeltaTime -- 累加时间

end

---------------------------------------------------------------------------------------------------
-- 空值判定
-- 功能：判断值是否为空（nil 或空字符串）
-- 参数：value - 要判断的值
-- 返回：true = 空值，false = 非空值
-- 用途：在使用变量前检查是否有效

function IsEmpty(value)

    return value == nil or value == ""

end

---------------------------------------------------------------------------------------------------
-- 安全访问嵌套表
-- 功能：安全地访问嵌套表，避免因 nil 而报错
-- 参数：tbl - 要访问的表，... - 键的路径（可变参数）
-- 返回：找到的值，如果路径中有 nil 则返回 nil
-- 用途：替代 tbl[key1][key2][key3] 这种可能报错的写法
--
-- 示例：
--   local name = SafeGet(Gv_CharaData, 1, "name")
--   等价于：Gv_CharaData[1]["name"]，但不会因为 Gv_CharaData[1] 为 nil 而报错

function SafeGet(tbl, ...)

    local current = tbl
    for _, key in ipairs({...}) do
        if current == nil then
            return nil
        end
        current = current[key]
    end
    return current

end

---------------------------------------------------------------------------------------------------
-- 安全判断嵌套表是否非空
-- 功能：安全地判断嵌套表的值是否非空
-- 参数：tbl - 要访问的表，... - 键的路径（可变参数）
-- 返回：true = 非空，false/nil = 空或路径中有 nil
-- 用途：结合了 SafeGet 和 IsEmpty 的功能
--
-- 示例：
--   if SafeIsNotEmpty(charaInfoTable, charaId, "File") then
--       -- 角色文件路径存在且非空
--   end

function SafeIsNotEmpty(tbl, ...)

    local current = tbl
    for _, key in ipairs({...}) do
        if current == nil then
            return nil
        end
        current = current[key]
    end
    return not IsEmpty(current)

end

---------------------------------------------------------------------------------------------------
-- 合并表
-- 功能：合并两个表，t2 的值会覆盖 t1 的同名键
-- 参数：t1 - 基础表（默认值），t2 - 覆盖表（用户值）
-- 返回：合并后的新表（不修改原表）
-- 用途：实现选项的默认值 + 用户自定义值
--
-- 示例：
--   local defaultOptions = { Level = 2, PosX = 0, PosY = 0 }
--   local userOptions = { PosX = 100 }
--   local finalOptions = MergeTables(defaultOptions, userOptions)
--   -- 结果：{ Level = 2, PosX = 100, PosY = 0 }

function MergeTables(t1, t2)

    local merged = {}
    -- 先复制 t1 的所有键值对
    for k, v in pairs(t1) do
        merged[k] = v
    end
    -- 再复制 t2 的所有键值对（会覆盖同名键）
    for k, v in pairs(t2) do
        merged[k] = v
    end

    return merged
end

---------------------------------------------------------------------------------------------------
-- 类型化调试日志
-- 功能：打印变量的类型和值，方便调试
-- 参数：value - 要打印的值
-- 用途：比普通 print 更详细，会显示变量类型
--
-- 示例：
--   LogWithType(123)        -- 输出：(number) 123
--   LogWithType("hello")    -- 输出：(string) "hello"
--   LogWithType(nil)        -- 输出：(nil)
--   LogWithType({a=1})      -- 输出：(table)

function LogWithType(value)

    if type(value) == "nil" then
        ADV:DebugLog("(nil)")
    elseif type(value) == "boolean" then
        ADV:DebugLog("(boolean) " .. tostring(value))
    elseif type(value) == "number" then
        ADV:DebugLog("(number) " .. tostring(value))
    elseif type(value) == "string" then
        ADV:DebugLog("(string) \"" .. value .. "\"")
    elseif type(value) == "table" then
        ADV:DebugLog("(table)")
    else
        ADV:DebugLog("(unknown type)")
    end
end

---------------------------------------------------------------------------------------------------
-- 常用工具函数总结：
--
-- 1. 空值判断：
--    IsEmpty(value)                    -- 判断是否为 nil 或 ""
--
-- 2. 安全访问：
--    SafeGet(tbl, key1, key2, ...)      -- 安全访问嵌套表
--    SafeIsNotEmpty(tbl, key1, ...)    -- 安全判断嵌套表是否非空
--
-- 3. 表操作：
--    MergeTables(t1, t2)               -- 合并两个表
--
-- 4. 调试：
--    LogWithType(value)                        -- 类型化日志输出
--    SetErrorPosition(debugInfo)            -- 记录错误位置
--
-- 5. 时间系统（自动管理，一般不需要手动调用）：
--    Time_NowTime                      -- 当前时间戳
--    Time_DeltaTime                    -- 帧时间
---------------------------------------------------------------------------------------------------
