using XLua;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityADV.Core;
using Cysharp.Threading.Tasks;

namespace UnityADV.Script
{
    /// <summary>
    /// Lua脚本引擎
    /// 替代原框架的PsLua，使用Unity原生文件系统
    /// </summary>
    public class LuaScriptEngine : IDisposable
    {
        private LuaEnv _luaEnv;
        private LuaCallbacks _luaCallbacks;
        private ADVManager _advManager;
        private HashSet<string> _loadedScripts;
        private string _luaScriptPath;
        private Action _resumeFunc;
        private Action _isDeadFunc;

        public LuaEnv LuaEnv => _luaEnv;

        public LuaScriptEngine(ADVManager manager)
        {
            _advManager = manager;
        }

        /// <summary>
        /// 初始化Lua引擎
        /// </summary>
        public async UniTask Prepare()
        {
            _luaEnv = new LuaEnv();
            //_luaEnv.AddLoader(CustomLoader);
            _loadedScripts = new HashSet<string>();
            _luaScriptPath = Path.Combine(Application.streamingAssetsPath, "Lua");

            // 1. 先加载基础系统脚本（不依赖 ADV 对象）
            LoadScript("System");
            LoadScript("Main");

            // 2. 创建并注入 ADV 对象（必须在 Include.lua 之前！）
            _luaCallbacks = new LuaCallbacks(_advManager);
            _luaEnv.Global.Set("ADV", _luaCallbacks);

            // 3. 加载 Include.lua（会调用 ADV:RegisterLuaScript，所以必须在 ADV 注入之后）
            LoadScript("Include");

            // 4. 获取必要的函数
            _resumeFunc = _luaEnv.Global.Get<Action>("ResumeCoroutine");
            _isDeadFunc = _luaEnv.Global.Get<Action>("IsCoroutineDead");

            Debug.Log("LuaScriptEngine initialized");
        }

        /// <summary>
        /// 加载Lua脚本
        /// </summary>
        public void LoadScript(string scriptName)
        {
            if (!_loadedScripts.Add(scriptName))
            {
                Debug.Log($"Script already loaded: {scriptName}");
                return;
            }

            try
            {
                string fullPath = Path.Combine(_luaScriptPath, scriptName);
                if (!fullPath.EndsWith(".lua") && !fullPath.EndsWith(".xlua"))
                {
                    // 尝试两种扩展名
                    if (File.Exists(fullPath + ".lua"))
                    {
                        fullPath += ".lua";
                    }
                    else if (File.Exists(fullPath + ".xlua"))
                    {
                        fullPath += ".xlua";
                    }
                }

                if (File.Exists(fullPath))
                {
                    string content = File.ReadAllText(fullPath);
                    content = CustomizeScript(content);

                    _luaEnv.DoString(content, scriptName);//记住这个脚本里所有的方法
                    Debug.Log($"Loaded Lua script: {scriptName}");
                }
                else
                {
                    Debug.LogWarning($"Lua script not found: {fullPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load Lua script: {scriptName}, Error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 加载脚本并支持跳过到指定行
        /// </summary>
        public void LoadScriptWithSkip(string scriptName, int skipToLine)
        {
            if (!_loadedScripts.Add(scriptName))
                return;

            try
            {
                string fullPath = Path.Combine(_luaScriptPath, scriptName);
                if (!fullPath.EndsWith(".lua") && !fullPath.EndsWith(".xlua"))
                {
                    if (File.Exists(fullPath + ".lua"))
                        fullPath += ".lua";
                    else if (File.Exists(fullPath + ".xlua"))
                        fullPath += ".xlua";
                }

                if (File.Exists(fullPath))
                {
                    string content = File.ReadAllText(fullPath);
                    content = CustomizeScript(content);

                    // 如果需要跳过，插入停止跳过命令
                    if (skipToLine > 0)
                    {
                        var lines = content.Split('\n');
                        if (skipToLine <= lines.Length)
                        {
                            var beforeLines = new string[skipToLine - 1];
                            Array.Copy(lines, beforeLines, skipToLine - 1);
                            var afterLines = new string[lines.Length - skipToLine + 1];
                            Array.Copy(lines, skipToLine - 1, afterLines, 0, lines.Length - skipToLine + 1);

                            content = string.Join("\n", beforeLines) + "\nADV:StopStartSkip()\n" + string.Join("\n", afterLines);
                        }
                    }

                    _luaEnv.DoString(content, scriptName);
                    _loadedScripts.Add(scriptName);
                    Debug.Log($"Loaded Lua script with skip: {scriptName} (line {skipToLine})");
                }
                else
                {
                    Debug.LogWarning($"Lua script not found: {fullPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load Lua script: {scriptName}, Error: {e.Message}");
                throw;
            }
        }

        private string CustomizeScript(string scriptContents)
        {
            var lines = scriptContents.Split('\n');
            var resultLines = new List<string>();
            foreach (var line in lines)
            {
                // 在 TEXT 开头的行前添加错误位置记录
                resultLines.Add(line.Trim().StartsWith("TEXT")
                    ? $"SetErrorPosition(debug.getinfo(1)) {line}"
                    : line);
            }
            return string.Join("\n", resultLines);
        }

        /// <summary>
        /// 获取Lua函数
        /// </summary>
        public T GetFunction<T>(string funcName) where T : Delegate
        {
            try
            {
                return _luaEnv.Global.Get<T>(funcName);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get Lua function: {funcName}, Error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tick（定期调用以处理Lua GC）
        /// </summary>
        public void Tick()
        {
            _luaEnv?.Tick();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _resumeFunc = null;
            _isDeadFunc = null;
            _luaCallbacks = null;
            _loadedScripts?.Clear();

            _luaEnv?.Dispose();
            _luaEnv = null;

            _advManager = null;
            Debug.Log("LuaScriptEngine disposed");
        }

        public void Resume()
        {
            _resumeFunc?.Invoke();
        }

        public bool IsScriptDead()
        {
            _isDeadFunc?.Invoke();

            return _luaCallbacks.GetBoolValue();
        }














        /// <summary>
        /// 自定义Lua文件加载器
        /// </summary>
        private byte[] CustomLoader(ref string filepath)
        {
            // 移除.lua扩展名（如果有）
            if (filepath.EndsWith(".lua"))
            {
                filepath = filepath.Substring(0, filepath.Length - 4);
            }

            // 尝试多个路径
            string[] searchPaths = new string[]
            {
                Path.Combine(_luaScriptPath, filepath + ".lua"),
                Path.Combine(_luaScriptPath, filepath + ".xlua"),
                Path.Combine(Application.streamingAssetsPath, filepath + ".lua"),
                Path.Combine(Application.streamingAssetsPath, filepath + ".xlua"),
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        return File.ReadAllBytes(path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load Lua file: {path}, Error: {e.Message}");
                    }
                }
            }

            Debug.LogWarning($"Lua file not found: {filepath}");
            return null;
        }

        /// <summary>
        /// 完整 GC
        /// </summary>
        public void FullGC()
        {
            _luaEnv?.FullGc();
        }

        /// <summary>
        /// 设置Lua全局变量
        /// </summary>
        public void SetGlobal(string name, object value)
        {
            _luaEnv.Global.Set(name, value);
        }

        /// <summary>
        /// 获取Lua全局变量
        /// </summary>
        public T GetGlobal<T>(string name)
        {
            return _luaEnv.Global.Get<T>(name);
        }

        /// <summary>
        /// 执行Lua代码
        /// </summary>
        public object[] DoString(string luaCode, string chunkName = "chunk")
        {
            try
            {
                return _luaEnv.DoString(luaCode, chunkName);
            }
            catch (Exception e)
            {
                Debug.LogError($"Lua execution error: {e.Message}");
                throw;
            }
        }
    }
}
