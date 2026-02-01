using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV状态枚举
    /// </summary>
    public enum ADVState
    {
        None = 0,
        Script = 1,        // 执行Lua脚本
        End = 2,           // 结束
        WaitTask = 3,      // 等待异步任务
        WaitKey = 101,     // 等待用户输入
        WaitText = 102,    // 等待文本显示
        WaitTime = 103,    // 等待时间
    }

    /// <summary>
    /// ADV扩展状态（菜单、日志等）
    /// </summary>
    public enum ADVExState
    {
        None = 0,
        BackLog = 1,
        Summary = 2,
        Skip = 3,
        Option = 4,
    }

    /// <summary>
    /// 对象类型
    /// </summary>
    public enum ADVObjectType
    {
        None = 0,
        Sprite = 1,
        Prefab = 2,
        Spine = 3,
    }

    /// <summary>
    /// 对象状态
    /// </summary>
    public enum AdvObjectState
    {
        None = 0,
        Deleting = 3,
    }

    /// <summary>
    /// Lua对象参数
    /// </summary>
    [Serializable]
    public class LuaObjectParam
    {
        public int Show = 1;
        public int Level = 2;
        public int Order = 0;
        public float PosX = 0;
        public float PosY = 0;
        public float FadeTime = 0.5f;
        public float RenderPosX = 0;
        public float RenderPosY = 0;
        public float RenderScale = 1.0f;
        public bool IsWipe = false;
        public bool IsSafeView = false;
    }

    /// <summary>
    /// Lua音频参数
    /// </summary>
    [Serializable]
    public class LuaSoundParam
    {
        public bool Loop = false;
        public float Volume = 1.0f;
    }

    /// <summary>
    /// 回顾日志项
    /// </summary>
    [Serializable]
    public class BackLogItem
    {
        public int CharaId;
        public string CharaName;
        public int TextId;
        public string Text;
    }

    [Serializable]
    public class ChapterAudioData
    {
        public List<string> bgm = new List<string>();
        public List<string> voice = new List<string>();
        public List<string> sound = new List<string>();
    }

    [Serializable]
    public class ChapterAudioEntry
    {
        public string chapterName;
        public ChapterAudioData audioData;
    }

    [Serializable]
    public class AudioConfigData
    {
        public List<ChapterAudioEntry> chapters = new List<ChapterAudioEntry>();
    }

    /// <summary>
    /// ADV配置
    /// </summary>
    [CreateAssetMenu(fileName = "ADVConfig", menuName = "ADV/Config")]
    public class ADVConfig : ScriptableObject
    {
        [Header("Text Settings")]
        public float textSpeed = 30f;
        public float autoModeMinWait = 1.0f;
        public float autoModeMojiWait = 0.05f;

        [Header("Render Settings")]
        public int renderTextureSize = 1200;
        public int screenHeight = 900;

        [Header("Camera Settings")]
        public string advCameraTag = "ADVCamera";
        public string spineRenderCameraTag = "SpineRenderCamera";
    }
}
