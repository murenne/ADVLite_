using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace UnityADV.Localization
{
    /// <summary>
    /// 本地化数据结构
    /// </summary>
    [System.Serializable]
    public class LocalizationData
    {
        public Dictionary<string, Dictionary<string, string>> CharacterNames;
        public Dictionary<string, Dictionary<string, string>> Scenarios;
        public Dictionary<string, Dictionary<string, string>> UI;
    }

    /// <summary>
    /// 本地化管理器
    /// 支持JSON格式的多语言翻译
    /// </summary>
    public class LocalizationManager1
    {
        private JObject _localizationData;
        private string _currentLanguage = "ja";
        private Dictionary<string, string> _characterNameCache = new Dictionary<string, string>();
        private Dictionary<string, string> _scenarioTextCache = new Dictionary<string, string>();

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    ClearCache();
                    Debug.Log($"Language changed to: {_currentLanguage}");
                }
            }
        }

        /// <summary>
        /// 从Resources加载本地化数据
        /// </summary>
        public void LoadFromResources(string resourcePath)
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>(resourcePath);
                if (textAsset != null)
                {
                    _localizationData = JObject.Parse(textAsset.text);
                    Debug.Log($"Loaded localization data from Resources: {resourcePath}");
                }
                else
                {
                    Debug.LogWarning($"Localization file not found in Resources: {resourcePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load localization data: {e.Message}");
            }
        }

        /// <summary>
        /// 从文件加载本地化数据
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    _localizationData = JObject.Parse(jsonContent);
                    Debug.Log($"Loaded localization data from file: {filePath}");
                }
                else
                {
                    Debug.LogWarning($"Localization file not found: {filePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load localization file: {e.Message}");
            }
        }

        /// <summary>
        /// 获取角色名称
        /// </summary>
        public string GetCharacterName(int nameId)
        {
            string cacheKey = $"char_{nameId}_{_currentLanguage}";

            if (_characterNameCache.TryGetValue(cacheKey, out string cached))
            {
                return cached;
            }

            try
            {
                var token = _localizationData?["CharacterNames"]?[nameId.ToString()];
                if (token != null)
                {
                    string result = token[_currentLanguage]?.ToString();
                    if (!string.IsNullOrEmpty(result))
                    {
                        _characterNameCache[cacheKey] = result;
                        return result;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get character name for ID {nameId}: {e.Message}");
            }

            return $"Character_{nameId}";
        }

        /// <summary>
        /// 获取场景文本（通过koeId）
        /// </summary>
        public string GetScenarioText(int chapterId, int koeId)
        {
            // 格式化为9位数字的ScenarioId
            string scenarioId = koeId.ToString("D9");
            return GetScenarioTextByKey(chapterId, scenarioId);
        }

        /// <summary>
        /// 获取场景文本（通过ScenarioId字符串）
        /// </summary>
        public string GetScenarioTextByKey(int chapterId, string scenarioId)
        {
            // 如果是日语，直接返回null（使用Lua中的原文）
            if (_currentLanguage == "ja")
            {
                return null;
            }

            string cacheKey = $"scenario_{chapterId}_{scenarioId}_{_currentLanguage}";

            if (_scenarioTextCache.TryGetValue(cacheKey, out string cached))
            {
                return cached;
            }

            try
            {
                // 尝试从Scenarios节点获取
                var token = _localizationData?["Scenarios"]?[chapterId.ToString()]?[scenarioId];
                if (token != null)
                {
                    string result = token[_currentLanguage]?.ToString();
                    if (!string.IsNullOrEmpty(result))
                    {
                        // 处理换行符
                        result = result.Replace("\\n", "\n");
                        _scenarioTextCache[cacheKey] = result;
                        return result;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get scenario text for Chapter {chapterId}, Scenario {scenarioId}: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// 获取UI文本
        /// </summary>
        public string GetUIText(string key)
        {
            try
            {
                var token = _localizationData?["UI"]?[key];
                if (token != null)
                {
                    string result = token[_currentLanguage]?.ToString();
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get UI text for key {key}: {e.Message}");
            }

            return key;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _characterNameCache.Clear();
            _scenarioTextCache.Clear();
        }

        /// <summary>
        /// 检测系统语言
        /// </summary>
        public static string DetectSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return "zh-CN";
                case SystemLanguage.ChineseTraditional:
                    return "zh-TW";
                case SystemLanguage.Korean:
                    return "ko";
                default:
                    return "en";
            }
        }
    }
}
