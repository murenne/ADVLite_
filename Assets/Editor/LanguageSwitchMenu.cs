using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity编辑器工具 - 语言快速切换菜单
/// </summary>
public class LanguageSwitchMenu
{
    private const string MENU_PATH = "Tools/Language/";

    [MenuItem(MENU_PATH + "Switch to Chinese (简体中文)")]
    public static void SwitchToChinese()
    {
        SwitchLanguage("zh-CN", "简体中文");
    }

    [MenuItem(MENU_PATH + "Switch to Japanese (日本語)")]
    public static void SwitchToJapanese()
    {
        SwitchLanguage("ja", "日本語");
    }

    [MenuItem(MENU_PATH + "Switch to English")]
    public static void SwitchToEnglish()
    {
        SwitchLanguage("en", "English");
    }

    [MenuItem(MENU_PATH + "Clear Saved Language")]
    public static void ClearSavedLanguage()
    {
        PlayerPrefs.DeleteKey("Language");
        PlayerPrefs.Save();

        Debug.Log("[LanguageSwitch] Cleared saved language. Will use system language on next startup.");

        EditorUtility.DisplayDialog(
            "Language Setting Cleared",
            "语言设置已清除。\n下次启动将使用系统语言。\n\nLanguage setting cleared.\nWill use system language on next startup.",
            "OK"
        );
    }

    [MenuItem(MENU_PATH + "Show Current Language")]
    public static void ShowCurrentLanguage()
    {
        string saved = PlayerPrefs.GetString("Language", "");
        string message;

        if (string.IsNullOrEmpty(saved))
        {
            message = $"No saved language.\nWill use system language: {UnityADV.Localization.LocalizationManager.DetectSystemLanguage()}";
        }
        else
        {
            string languageName = GetLanguageName(saved);
            message = $"Current saved language: {saved} ({languageName})";
        }

        Debug.Log($"[LanguageSwitch] {message}");
        EditorUtility.DisplayDialog("Current Language", message, "OK");
    }

    private static void SwitchLanguage(string languageCode, string languageName)
    {
        PlayerPrefs.SetString("Language", languageCode);
        PlayerPrefs.Save();

        Debug.Log($"[LanguageSwitch] Language switched to: {languageCode} ({languageName})");

        // 如果游戏正在运行，尝试立即切换
        if (Application.isPlaying)
        {
            var advManager = GameObject.FindObjectOfType<UnityADV.Core.ADVManager>();
            if (advManager != null && advManager.LocalizationManager != null)
            {
                advManager.LocalizationManager.SetLanguage(languageCode);

                EditorUtility.DisplayDialog(
                    "Language Switched",
                    $"语言已切换到: {languageName}\n\nLanguage switched to: {languageName}\n\n⚠️ 当前显示的文本可能需要重新加载场景才能更新。",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Language Saved",
                    $"语言设置已保存: {languageName}\n重启游戏后生效。\n\nLanguage saved: {languageName}\nRestart the game to apply.",
                    "OK"
                );
            }
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Language Saved",
                $"语言设置已保存: {languageName}\n启动游戏后生效。\n\nLanguage saved: {languageName}\nStart the game to apply.",
                "OK"
            );
        }
    }

    private static string GetLanguageName(string languageCode)
    {
        switch (languageCode)
        {
            case "zh-CN": return "简体中文";
            case "ja": return "日本語";
            case "en": return "English";
            default: return languageCode;
        }
    }
}
