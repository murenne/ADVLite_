---------------------------------------------------------------------------------------------------
-- Main.lua - Lua 协程生命周期管理器
-- 职责：管理 ADV 脚本的协程启动、暂停、恢复和停止
-- 这个文件是 ADV 系统的"心脏"，控制整个脚本的执行节奏
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
-- 定义
-- 全局变量声明
ADV = nil -- C# 回调对象（由 C# 的 _luaEnv.Global.Set("ADV", _luaCallbacks) 注入）
mainCoroutine = nil -- 主协程句柄，保存用户脚本的协程对象

-- 创建并启动主协程
StartCoroutine = function(mainProc)

    InitializeSystem()
    mainCoroutine = coroutine.create(mainProc)

end

---------------------------------------------------------------------------------------------------
-- 暂停当前协程，将控制权交还给 C#
-- 调用时机：Lua 脚本需要等待异步操作时（如 ADV:WaitTask() 后）
-- 重要：每次调用 ADV:WaitTask() 后都必须调用 PauseCoroutine()
PauseCoroutine = function()

    -- 暂停协程，yield(0) 会将控制权交还给 C#
    coroutine.yield(0)

    -- 当 C# 调用 ResumeCoroutine() 恢复协程后，会执行这里
    -- 更新 Lua 层的时间系统（System.lua 中定义）
    UpdateFrame()
end

---------------------------------------------------------------------------------------------------
-- 恢复协程执行
-- 调用者：C# 的 ADVManager.ResumeScript()
-- 调用时机：异步任务完成后，或每帧更新时

ResumeCoroutine = function()

    --  Lua 协程有时不会抛出错误，所以用 assert 包裹
    -- assert 确保协程执行成功，如果失败会立即报错
    assert(coroutine.resume(mainCoroutine))
end

---------------------------------------------------------------------------------------------------
-- 检查协程是否已经执行完毕
-- 调用者：C# 的 ADVManager
-- 返回：通过 ADV:SetBoolValue() 将结果传回 C#

IsCoroutineDead = function()

    ADV:SetBoolValue(coroutine.status(mainCoroutine) == "dead")

end

---------------------------------------------------------------------------------------------------
-- 调试
-- 功能：打印调试信息到 Unity Console
-- 参数：lh_msg - 要打印的消息

DebugLogInfo = function(lh_msg)

    ADV:DebugLog(lh_msg)
end

---------------------------------------------------------------------------------------------------
-- 使用示例：
--
-- function Main_Story()
--     ADV:PrepareAssetSprite("bg.png")
--     ADV:WaitTask()  -- 通知 C# 等待异步任务
--     PauseCoroutine()      -- 暂停协程，交还控制权
--     
--     -- C# 完成任务后会调用 ResumeCoroutine()，协程从这里继续执行
--     ADV:ObjectCreateSprite(100, "bg.png", {...})
-- end
--
-- function StartCoroutine_Adv()
--     StartCoroutine(Main_Story)  -- 启动主协程
-- end
---------------------------------------------------------------------------------------------------
