# ADVLite

[English](./README.md) | [中文](./README_cn.md) | [日本語](./README_jp.md)

这是一个基于 **Unity 2022.3.36f1** 和 **XLua** 开发的文字冒险游戏（ADV/AVG）框架  
采用逻辑与视图分离的架构设计，支持多语言本地化和高扩展性

## Tech Stack

| 类别 | 技术 | 版本 | 描述 |
| :--- | :--- | :--- | :--- |
| **游戏引擎** | Unity | 2022.3.36f1 | 核心游戏开发引擎 |
| **脚本引擎** | XLua | 最新版 | 高性能 Lua 脚本集成框架 |
| **动画系统** | Spine | Runtime 3.x | 2D 骨骼动画运行时 |
| **动画辅助** | DOTween | 1.2.790 | 补间动画插件库 |
| **异步框架** | UniTask | 2.5.10 | 为 Unity 定制的异步任务处理框架 |
| **数据处理** | Newtonsoft.Json | 3.2.2 | JSON 序列化与反序列化工具 |
| **资源管理** | Addressables | 1.21.21 | 资源管理、加载与打包系统 |
| **文本渲染** | TextMeshPro | 3.0.6（UPM 包） | 高质量矢量文本渲染系统 |

---

<!--ts-->
* [ADVLite](#advlite)
    * [Tech Stack](#tech-stack)
    * [Getting Started](#getting-started)
    * [Control](#control)
    * [Architecture](#architecture)
        * [Lua Script Engine](#lua-script-engine)
        * [Logic and View Separation](#logic-and-view-separation)
        * [Addressables Resource Management](#addressables-resource-management)
    * [Core Systems](#core-systems)
        * [ADV Management System](#adv-management-system)
        * [Localization System](#localization-system)
        * [Character System](#character-system)
        * [Audio System](#audio-system)
        * [UI System](#ui-system)
        * [Resource Management System](#resource-management-system)
    * [Statement](#statement)
<!--te-->

## Getting Started

1. 从 GitHub 克隆该项目至本地
2. 使用 Unity Hub 打开项目（确保 Unity 版本 >= 2022.3.36f1）
3. 点击 Play 按钮即可运行

## Control

| 操作      | 功能说明                     |
| ----------| ------------------------ |
| `鼠标左键`        | 推进对话                |
| `Ctrl`        | 硬快进（不加 Shift）                |
| `Ctrl + Shift`        | 软快进                |
| `Tools → Language → Switch to XXX`（Editor 菜单）        | 切换语言，仅用于 Editor 下调试                |


## Architecture

本项目采用 **逻辑与视图分离** 的现代化架构设计，确保代码的可维护性和可扩展性

### Lua Script Engine

使用 **XLua** 作为脚本引擎，实现灵活的剧本编写：

- **剧本文件**：所有游戏剧情使用 Lua 编写
- **命令系统**：封装了丰富的 Lua 命令接口（对话、音效、动画等）
- **脚本外置**：Lua 脚本从 StreamingAssets 以文件形式加载执行，替换脚本无需重新编译打包（暂未实现远程分发等热更新机制）
- **协程机制**：支持复杂的时序控制和异步操作

**示例 Lua 脚本：**
```lua
function Main_Story()
    PrepareChapterAudio("Chapter01")
    FadeInStart()
    SetBackground("bg_002.png")
    SetCharacter(1, 2)  -- 显示角色
    SetTextWindowOpen()
    
    SetText(1000001, 1, "你好吗")  -- textId, charaId, text
    SetText(1000002, 2, "我很好，谢谢")
end
```

---

### Logic and View Separation

采用 **逻辑与视图分层** 的架构模式：

- **逻辑层（ADVManager）**：处理游戏核心逻辑和状态管理
- **视图层（ADVUIController）**：负责UI显示和动画表现
- **通信方式**：逻辑层持有视图层引用，直接调用其公开方法驱动表现

---

### Addressables Resource Management

使用 Unity Addressables 系统进行资源的异步加载和内存管理：

- **异步加载**：所有资源采用异步加载，避免卡顿
- **引用计数管理**：通过引用计数追踪已加载资源，需显式调用释放接口（未释放的资源会记录日志提示）
- **本地资源包**：当前资源组均配置为本地构建与加载，远程资源更新暂未启用

## Core Systems

项目已实现以下核心游戏系统：

### ADV Management System

负责游戏整体流程控制和状态管理：

- **状态机管理**：等待文本、等待按键、等待时间、等待任务等状态
- **帧循环处理**：基于 UniTask 的高性能异步帧循环
- **脚本执行**：通过 LuaScriptEngine 执行 Lua 剧本
- **对话推进**：自动文本滚动显示和用户输入处理

![ADV Management](./README_Images/ADV.gif)

---

### Localization System

专业化翻译架构：

- **TSV源文件**：使用 Tab 分隔的 TSV 格式便于翻译团队编辑
- **JSON构建工具**：Editor 工具一键将 TSV 转换为优化的 JSON
- **基础+覆盖机制**：简体中文为基础语言，日语和英语作为覆盖
- **高性能解析**：使用 Newtonsoft.Json 的 JObject 实现快速查询

**支持的语言：**
- 🇨🇳 简体中文（zh-CN）- 基础语言
- 🇯🇵 日语（ja）- 覆盖翻译
- 🇬🇧 英语（en）- 覆盖翻译

**数据格式：**

| 文件类型     | 说明                     |
| ----------| ------------------------ |
| `ADVCharacterNames.tsv`       | 角色名称翻译              |
| `ADVScenarios_ChapterXX.tsv`       | 剧情对话翻译（按章节，仅日语/英语提供）                |
| `ADVMetadata.tsv`       | 章节标题和摘要                |

> 注：简体中文为基础语言，剧情对话原文直接写在 Lua 剧本中，不经过 TSV/JSON，因此简体中文没有 `ADVScenarios_ChapterXX.tsv` 文件。

**Editor工具：**
- `Tools → Localization → Build JSON from TSV` - 构建 JSON 文件
- `Tools → Language → Switch to XXX` - 快速切换语言测试

**特点：**
- Lua 中的简体中文对话原文
- JSON 存储日语和英语翻译
- PlayerPrefs 持久化语言设置

![Language Switch](./README_Images/Language.png)

---

### Character System

基于 **Spine** 的2D骨骼动画角色系统（Spine 本身支持换装、换表情等 Skin 功能，本项目暂未实现）：

- **表情控制**：独立的表情动画轨道（Track），可与身体动作分层驱动
- **动画混合**：基于 Spine AnimationState 的动作过渡与混合

---

### Audio System

完整的音频管理解决方案：

- **BGM播放**：支持循环播放
- **语音播放**：角色语音自动管理和停止
- **音效播放**：支持多个音效同时播放
- **音量控制**：独立的BGM、语音、音效音量设置
- **配置系统**：JSON配置音频资源路径

**音频配置示例：**
```json
{
  "chapters": [
    {
      "chapterName": "Chapter01",
      "audioData": {
        "bgm": ["BGM/bgm_chapter01.mp3"],
        "voice": ["Voice/voice_1000001.mp3", "Voice/voice_1000002.mp3"],
        "sound": ["SE/se_car.mp3"]
      }
    }
  ]
}
```

---

### UI System

响应式UI控制系统：

- **对话框**：文本显示、角色名
- **淡入淡出**：场景和UI过渡效果
- **按键提示**：等待输入的视觉提示

---

### Resource Management System

基于 Addressables 的异步资源管理：

- **异步加载**：使用 UniTask 进行高性能异步加载
- **生命周期管理**：基于引用计数追踪已加载资源，需显式调用释放接口
- **类型支持**：Sprite、AudioClip、GameObject、SkeletonDataAsset 等（本地化文本等仍通过 `Resources.Load` 单独加载，未接入 Addressables）
- **预加载机制**：音频按章节批量预加载，其余资源（Sprite/GameObject/Spine 等）由 Lua 逐个触发预加载

## Statement

该项目基于 **Unity 2022.3.36f1** 和 **XLua** 开发  
采用逻辑与视图分离的架构设计，  
适用于视觉小说、文字冒险游戏、Galgame 等类型的游戏开发。 

**关于 Spine 资源：**  
由于未找到合适的立绘 Spine 资源，项目暂时使用非立绘 Spine 文件进行开发和效果展示。  
本项目所用 Spine 文件版权归属 [**上海鹰角网络有限公司**](https://www.hypergryph.com) 所有。不得用于商业用途，不得损害版权方的利益。  

