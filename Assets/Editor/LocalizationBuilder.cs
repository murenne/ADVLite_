using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

/// <summary>
/// TSV到JSON的本地化构建工具（对齐321架构）
/// 
/// 架构说明：
/// - Lua脚本 = 简体中文对话原文
/// - JSON基础(zh-CN) = 角色名 + 标题/摘要（不包含对话，因为在Lua中）
/// - JSON覆盖(ja/en) = 对话翻译 + 角色名 + 标题/摘要
/// </summary>
public class LocalizationBuilder : EditorWindow
{
    private const string TSV_SOURCE_PATH = "Assets/StreamingAssets/Localization";
    private const string JSON_OUTPUT_PATH = "Assets/Resources/Localization";
    private const string BASE_LANGUAGE = "zh-CN";

    [MenuItem("Tools/Localization/Build JSON from TSV")]
    public static void BuildLocalizationJson()
    {
        Debug.Log("[LocalizationBuilder] ========== Starting build ==========");
        Debug.Log("[LocalizationBuilder] Architecture: Lua(zh-CN dialogue) + JSON(translations + metadata)");

        // 确保输出目录存在
        if (!Directory.Exists(JSON_OUTPUT_PATH))
        {
            Directory.CreateDirectory(JSON_OUTPUT_PATH);
            Debug.Log($"[LocalizationBuilder] Created output directory: {JSON_OUTPUT_PATH}");
        }

        // 1. 构建基础JSON (zh-CN) - 只包含角色名和元数据，不包含对话
        BuildBaseJson();

        // 2. 构建覆盖JSON (ja) - 包含对话翻译、角色名和元数据
        BuildOverrideJson("ja");

        // 3. 构建覆盖JSON (en) - 包含对话翻译、角色名和元数据
        BuildOverrideJson("en");

        AssetDatabase.Refresh();
        Debug.Log("[LocalizationBuilder] ========== Build completed! ==========");

        EditorUtility.DisplayDialog("Localization Builder",
            "JSON文件已成功生成！\n\n" +
            "架构说明：\n" +
            "- Lua脚本 = 简体中文对话原文\n" +
            "- ADVText.json = 角色名 + 元数据（不含对话）\n" +
            "- ADVText-ja.json = 日语翻译 + 元数据\n" +
            "- ADVText-en.json = 英语翻译 + 元数据",
            "OK");
    }

    /// <summary>
    /// 构建基础JSON文件（简体中文）
    /// 只包含角色名和元数据，不包含对话（对话在Lua中）
    /// </summary>
    private static void BuildBaseJson()
    {
        Debug.Log($"[LocalizationBuilder] Building base JSON for language: {BASE_LANGUAGE}");
        Debug.Log($"[LocalizationBuilder] Base JSON contains: Character Names + Metadata (NO dialogue, dialogue in Lua)");

        var jObject = new JObject();

        // 读取角色名
        AddCharacterNames(jObject, BASE_LANGUAGE);

        // 读取元数据（标题、摘要）
        AddMetadata(jObject, BASE_LANGUAGE);

        // ⚠️ 不添加场景对话！对话在Lua脚本中

        // 保存
        SaveJson(jObject, "ADVText.json");

        Debug.Log($"[LocalizationBuilder] Base JSON saved: ADVText.json (CharacterNames + Metadata only)");
    }

    /// <summary>
    /// 构建覆盖JSON文件（日语或英语）
    /// 包含对话翻译、角色名和元数据
    /// </summary>
    private static void BuildOverrideJson(string language)
    {
        Debug.Log($"[LocalizationBuilder] Building override JSON for language: {language}");

        string languagePath = Path.Combine(Application.dataPath.Replace("/Assets", ""), TSV_SOURCE_PATH, language);

        // 检查该语言目录是否存在
        if (!Directory.Exists(languagePath))
        {
            Debug.LogWarning($"[LocalizationBuilder] Language directory not found: {languagePath}. Skipping {language}.");
            return;
        }

        var jObject = new JObject();

        // 读取角色名
        AddCharacterNames(jObject, language);

        // 读取元数据
        AddMetadata(jObject, language);

        // 读取所有章节的场景文本（对话翻译）
        AddAllScenarios(jObject, language);

        // 保存
        SaveJson(jObject, $"ADVText-{language}.json");

        Debug.Log($"[LocalizationBuilder] Override JSON saved: ADVText-{language}.json (Dialogue + CharacterNames + Metadata)");
    }

    /// <summary>
    /// 添加角色名数据到JSON
    /// </summary>
    private static void AddCharacterNames(JObject jObject, string language)
    {
        string tsvPath = Path.Combine(
            Application.dataPath.Replace("/Assets", ""),
            TSV_SOURCE_PATH,
            language,
            "ADVCharacterNames.tsv"
        );

        if (!File.Exists(tsvPath))
        {
            Debug.LogWarning($"[LocalizationBuilder] Character names file not found: {tsvPath}");
            return;
        }

        var lines = File.ReadAllLines(tsvPath);
        var idxList = new JArray();
        var nameList = new JArray();
        int count = 0;

        for (int i = 1; i < lines.Length; i++) // 跳过标题行
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var columns = lines[i].Split('\t');
            if (columns.Length >= 2 && int.TryParse(columns[0], out int idx))
            {
                idxList.Add(idx);
                nameList.Add(columns[1]);
                count++;
            }
        }

        var characterData = new JObject();
        characterData["idx"] = idxList;
        characterData["name"] = nameList;

        jObject["survivor_MasterAdvCharacterNameForClient"] = characterData;

        Debug.Log($"[LocalizationBuilder] Added {count} character names for {language}");
    }

    /// <summary>
    /// 添加元数据（标题、摘要）到JSON
    /// </summary>
    private static void AddMetadata(JObject jObject, string language)
    {
        string tsvPath = Path.Combine(
            Application.dataPath.Replace("/Assets", ""),
            TSV_SOURCE_PATH,
            language,
            "ADVMetadata.tsv"
        );

        if (!File.Exists(tsvPath))
        {
            Debug.LogWarning($"[LocalizationBuilder] Metadata file not found: {tsvPath}");
            return;
        }

        var lines = File.ReadAllLines(tsvPath);
        var idxList = new JArray();
        var titleList = new JArray();
        var summaryList = new JArray();
        int count = 0;

        for (int i = 1; i < lines.Length; i++) // 跳过标题行
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var columns = lines[i].Split('\t');
            if (columns.Length >= 3 && int.TryParse(columns[0], out int idx))
            {
                idxList.Add(idx);
                titleList.Add(columns[1]);
                summaryList.Add(columns[2]);
                count++;
            }
        }

        var metadataData = new JObject();
        metadataData["idx"] = idxList;
        metadataData["title"] = titleList;
        metadataData["summary"] = summaryList;

        jObject["survivor_MasterAdvForClient"] = metadataData;

        Debug.Log($"[LocalizationBuilder] Added {count} ADV metadata entries for {language}");
    }

    /// <summary>
    /// 添加所有章节的场景文本到JSON
    /// </summary>
    private static void AddAllScenarios(JObject jObject, string language)
    {
        string languagePath = Path.Combine(
            Application.dataPath.Replace("/Assets", ""),
            TSV_SOURCE_PATH,
            language
        );

        if (!Directory.Exists(languagePath))
        {
            Debug.LogWarning($"[LocalizationBuilder] Language directory not found: {languagePath}");
            return;
        }

        // 查找所有场景TSV文件
        var scenarioFiles = Directory.GetFiles(languagePath, "ADVScenarios_Chapter*.tsv");

        if (scenarioFiles.Length == 0)
        {
            Debug.LogWarning($"[LocalizationBuilder] No scenario files found in {languagePath}");
            return;
        }

        var scenariosData = new JObject();
        int totalCount = 0;

        foreach (var scenarioFile in scenarioFiles)
        {
            // 从文件名提取章节ID
            string fileName = Path.GetFileNameWithoutExtension(scenarioFile);
            string chapterIdStr = fileName.Replace("ADVScenarios_Chapter", "");

            if (int.TryParse(chapterIdStr, out int chapterId))
            {
                var chapterData = LoadScenarioFile(scenarioFile, out int count);
                if (chapterData != null)
                {
                    scenariosData[chapterId.ToString()] = chapterData;
                    totalCount += count;
                    Debug.Log($"[LocalizationBuilder] Added {count} scenarios for Chapter {chapterId} ({language})");
                }
            }
        }

        jObject["survivor_MasterAdvScenarioForClient"] = scenariosData;
        Debug.Log($"[LocalizationBuilder] Total scenarios added: {totalCount} for {language}");
    }

    /// <summary>
    /// 加载单个场景TSV文件
    /// </summary>
    private static JObject LoadScenarioFile(string tsvPath, out int count)
    {
        count = 0;

        if (!File.Exists(tsvPath))
        {
            Debug.LogWarning($"[LocalizationBuilder] Scenario file not found: {tsvPath}");
            return null;
        }

        var lines = File.ReadAllLines(tsvPath);
        var idxList = new JArray();
        var textList = new JArray();

        for (int i = 1; i < lines.Length; i++) // 跳过标题行
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var columns = lines[i].Split('\t');
            if (columns.Length >= 2 && int.TryParse(columns[0], out int idx))
            {
                idxList.Add(idx);
                textList.Add(columns[1]);
                count++;
            }
        }

        var chapterData = new JObject();
        chapterData["idx"] = idxList;
        chapterData["text"] = textList;

        return chapterData;
    }

    /// <summary>
    /// 保存JSON文件
    /// </summary>
    private static void SaveJson(JObject jObject, string fileName)
    {
        string outputPath = Path.Combine(
            Application.dataPath.Replace("/Assets", ""),
            JSON_OUTPUT_PATH,
            fileName
        );

        // 美化JSON格式
        string jsonString = jObject.ToString(Newtonsoft.Json.Formatting.Indented);

        File.WriteAllText(outputPath, jsonString);

        Debug.Log($"[LocalizationBuilder] Saved: {outputPath} ({jsonString.Length} bytes)");
    }

    [MenuItem("Tools/Localization/Open Localization Folder")]
    public static void OpenLocalizationFolder()
    {
        string path = Path.Combine(Application.dataPath.Replace("/Assets", ""), TSV_SOURCE_PATH);
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Localization/Open JSON Output Folder")]
    public static void OpenJsonOutputFolder()
    {
        string path = Path.Combine(Application.dataPath.Replace("/Assets", ""), JSON_OUTPUT_PATH);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        EditorUtility.RevealInFinder(path);
    }
}
