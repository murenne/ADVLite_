using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityADV.Localization
{
    /// <summary>
    /// 架构说明：
    /// - Lua脚本 = 简体中文对话原文
    /// - JSON基础(zh-CN) = 角色名 + 元数据（不含对话）
    /// - JSON覆盖(ja/en) = 对话翻译 + 角色名 + 元数据
    /// </summary>
    public class LocalizationManager
    {
        private string _currentLanguage = "zh-CN";

        // 数据存储
        private JObject _characterNameData;
        private JObject _metadataData;      // 新增：标题和摘要
        private JObject _scenarioData;

        // 缓存
        private Dictionary<int, string> _characterNameCache = new Dictionary<int, string>();
        private Dictionary<int, (string title, string summary)> _metadataCache = new Dictionary<int, (string, string)>();
        private Dictionary<int, string> _scenarioCache = new Dictionary<int, string>();

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadData();
                }
            }
        }

        /// <summary>
        /// 加载本地化数据
        /// </summary>
        public void LoadData()
        {
            ClearCache();

            Debug.Log($"[LocalizationManager] Loading data for language: {_currentLanguage}");
            Debug.Log($"[LocalizationManager] Architecture: Lua(zh-CN dialogue) + JSON(translations + metadata)");

            // 1. 加载基础数据（zh-CN）- 只包含角色名和元数据
            var baseJson = Resources.Load<TextAsset>("Localization/ADVText");
            if (baseJson == null)
            {
                Debug.LogError("[LocalizationManager] Base JSON (ADVText.json) not found! Please build JSON first via Tools → Localization → Build JSON from TSV");
                return;
            }

            var jObject = JObject.Parse(baseJson.text);
            _characterNameData = jObject["survivor_MasterAdvCharacterNameForClient"] as JObject;
            _metadataData = jObject["survivor_MasterAdvForClient"] as JObject;
            _scenarioData = null; // 基础JSON不包含对话数据

            Debug.Log($"[LocalizationManager] Base data loaded (zh-CN): CharacterNames + Metadata");

            // 2. 如果当前语言不是简体中文，加载覆盖数据（包含对话翻译）
            if (_currentLanguage != "zh-CN")
            {
                var overrideJson = Resources.Load<TextAsset>($"Localization/ADVText-{_currentLanguage}");
                if (overrideJson != null)
                {
                    OverrideData(JObject.Parse(overrideJson.text));
                    Debug.Log($"[LocalizationManager] Override data loaded for {_currentLanguage} (Dialogue + CharacterNames + Metadata)");
                }
                else
                {
                    Debug.LogWarning($"[LocalizationManager] No override JSON found for {_currentLanguage}, using base language (zh-CN)");
                }
            }
            else
            {
                Debug.Log($"[LocalizationManager] Current language is zh-CN, dialogue text will come from Lua scripts");
            }

            LogDataStats();
        }

        /// <summary>
        /// 覆盖基础数据
        /// </summary>
        private void OverrideData(JObject overrideData)
        {
            // 覆盖角色名
            if (overrideData.TryGetValue("survivor_MasterAdvCharacterNameForClient", out var charToken))
            {
                _characterNameData = charToken as JObject;
                Debug.Log($"[LocalizationManager] Character names overridden");
            }

            // 覆盖元数据
            if (overrideData.TryGetValue("survivor_MasterAdvForClient", out var metadataToken))
            {
                _metadataData = metadataToken as JObject;
                Debug.Log($"[LocalizationManager] Metadata overridden");
            }

            // 覆盖场景文本（对话翻译）
            if (overrideData.TryGetValue("survivor_MasterAdvScenarioForClient", out var scenarioToken))
            {
                _scenarioData = scenarioToken as JObject;
                Debug.Log($"[LocalizationManager] Scenario texts overridden");
            }
        }

        /// <summary>
        /// 获取角色名称
        /// </summary>
        public string GetCharacterName(int nameId)
        {
            // 检查缓存
            if (_characterNameCache.TryGetValue(nameId, out string cached))
                return cached;

            if (_characterNameData == null)
            {
                Debug.LogWarning("[LocalizationManager] Character name data not loaded!");
                return null;
            }

            // 从JArray中查找
            var idxArray = _characterNameData["idx"] as JArray;
            var nameArray = _characterNameData["name"] as JArray;

            if (idxArray == null || nameArray == null)
            {
                Debug.LogError("[LocalizationManager] Invalid character name data format!");
                return null;
            }

            for (int i = 0; i < idxArray.Count; i++)
            {
                if (idxArray[i].Value<int>() == nameId)
                {
                    string name = nameArray[i].Value<string>();
                    _characterNameCache[nameId] = name;
                    return name;
                }
            }

            Debug.LogWarning($"[LocalizationManager] Character name not found for ID: {nameId}");
            return null;
        }

        /// <summary>
        /// 获取ADV元数据（标题和摘要）
        /// </summary>
        public (string title, string summary) GetMetadata(int advId)
        {
            // 检查缓存
            if (_metadataCache.TryGetValue(advId, out var cached))
                return cached;

            if (_metadataData == null)
            {
                Debug.LogWarning("[LocalizationManager] Metadata not loaded!");
                return (null, null);
            }

            // 从JArray中查找
            var idxArray = _metadataData["idx"] as JArray;
            var titleArray = _metadataData["title"] as JArray;
            var summaryArray = _metadataData["summary"] as JArray;

            if (idxArray == null || titleArray == null || summaryArray == null)
            {
                Debug.LogError("[LocalizationManager] Invalid metadata format!");
                return (null, null);
            }

            for (int i = 0; i < idxArray.Count; i++)
            {
                if (idxArray[i].Value<int>() == advId)
                {
                    string title = titleArray[i].Value<string>();
                    string summary = summaryArray[i].Value<string>();
                    var result = (title, summary);
                    _metadataCache[advId] = result;
                    return result;
                }
            }

            Debug.LogWarning($"[LocalizationManager] Metadata not found for ADV ID: {advId}");
            return (null, null);
        }

        /// <summary>
        /// 获取场景文本
        /// 
        /// ⚠️ 重要：
        /// - 如果语言是zh-CN，返回null（对话在Lua脚本中）
        /// - 如果语言是ja/en，返回JSON中的翻译
        /// </summary>
        public string GetScenarioText(int chapterId, int koeId)
        {
            // zh-CN对话在Lua中，返回null让调用方使用Lua原文
            if (_currentLanguage == "zh-CN")
            {
                return null;
            }

            // 检查缓存
            if (_scenarioCache.TryGetValue(koeId, out string cached))
                return cached;

            if (_scenarioData == null)
            {
                Debug.LogWarning("[LocalizationManager] Scenario data not loaded!");
                return null;
            }

            // 获取该章节的数据
            var chapterData = _scenarioData[chapterId.ToString()] as JObject;
            if (chapterData == null)
            {
                Debug.LogWarning($"[LocalizationManager] No scenario data for chapter: {chapterId}");
                return null;
            }

            // 从JArray中查找
            var idxArray = chapterData["idx"] as JArray;
            var textArray = chapterData["text"] as JArray;

            if (idxArray == null || textArray == null)
            {
                Debug.LogError($"[LocalizationManager] Invalid scenario data format for chapter: {chapterId}");
                return null;
            }

            for (int i = 0; i < idxArray.Count; i++)
            {
                if (idxArray[i].Value<int>() == koeId)
                {
                    string text = textArray[i].Value<string>();
                    // 处理换行符
                    text = text.Replace("\\n", "\n");
                    _scenarioCache[koeId] = text;
                    return text;
                }
            }

            return null;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        private void ClearCache()
        {
            _characterNameCache.Clear();
            _metadataCache.Clear();
            _scenarioCache.Clear();
        }

        /// <summary>
        /// 输出数据统计信息
        /// </summary>
        private void LogDataStats()
        {
            int charCount = 0;
            int metadataCount = 0;
            int scenarioCount = 0;
            int chapterCount = 0;

            if (_characterNameData != null)
            {
                var idxArray = _characterNameData["idx"] as JArray;
                if (idxArray != null) charCount = idxArray.Count;
            }

            if (_metadataData != null)
            {
                var idxArray = _metadataData["idx"] as JArray;
                if (idxArray != null) metadataCount = idxArray.Count;
            }

            if (_scenarioData != null)
            {
                chapterCount = _scenarioData.Count;
                foreach (var chapter in _scenarioData)
                {
                    var chapterData = chapter.Value as JObject;
                    if (chapterData != null)
                    {
                        var idxArray = chapterData["idx"] as JArray;
                        if (idxArray != null) scenarioCount += idxArray.Count;
                    }
                }
            }

            Debug.Log($"[LocalizationManager] Data stats - Characters: {charCount}, Metadata: {metadataCount}, Scenarios: {scenarioCount} (across {chapterCount} chapters)");
        }

        /// <summary>
        /// 设置语言并保存到PlayerPrefs
        /// </summary>
        public void SetLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                Debug.LogWarning("[LocalizationManager] Invalid language code");
                return;
            }

            Debug.Log($"[LocalizationManager] Setting language to: {language}");

            // 保存到PlayerPrefs
            UnityEngine.PlayerPrefs.SetString("Language", language);
            UnityEngine.PlayerPrefs.Save();

            // 立即切换语言
            CurrentLanguage = language;

            Debug.Log($"[LocalizationManager] Language changed and saved: {language}");
        }

        /// <summary>
        /// 获取保存的语言设置
        /// 如果没有保存，返回null
        /// </summary>
        public static string GetSavedLanguage()
        {
            string saved = UnityEngine.PlayerPrefs.GetString("Language", "");
            return string.IsNullOrEmpty(saved) ? null : saved;
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
                default:
                    return "zh-CN";
            }
        }

        /// <summary>
        /// 获取初始语言
        /// 优先使用保存的设置，否则使用系统语言
        /// </summary>
        public static string GetInitialLanguage()
        {
            string saved = GetSavedLanguage();
            if (saved != null)
            {
                Debug.Log($"[LocalizationManager] Using saved language: {saved}");
                return saved;
            }

            string detected = DetectSystemLanguage();
            Debug.Log($"[LocalizationManager] No saved language, using system language: {detected}");
            return detected;
        }
    }
}