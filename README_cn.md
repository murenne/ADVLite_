# ADVLite

[English](./README.md) | [中文](./README_cn.md) | [日本語](./README_jp.md)

这是一个基于 **Unity 2022.3.36f1** 和 **XLua** 开发的文字冒险游戏（ADV/AVG）框架  
采用逻辑与视图分离的架构设计，支持多语言本地化和高扩展性

<!--ts-->
* [ADVLite](#advlite)
    * [Getting Started](#getting-started)
    * [Control](#control)
    * [Architecture](#architecture)
        * [Lua Script Engine](#lua-script-engine)
        * [Logic and View Separation](#logic-and-view-separation)
        * [Addressables Resource Management](#addressables-resource-management)
    * [Core System](#core-system)
        * [ADV Management System](#adv-management-system)
        * [Localization System](#localization-system)
        * [Character System](#character-system)
        * [Audio System](#audio-system)
        * [UI System](#ui-system)
        * [Resource Management System](#resource-management-system)
    * [Technological Stack](#technological-stack)
    * [Directory Structure](#directory-structure)
    * [Development Instructions](#development-instructions)
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


## Architecture

本项目采用 **逻辑与视图分离** 的现代化架构设计，确保代码的可维护性和可扩展性

### Lua Script Engine

使用 **XLua** 作为脚本引擎，实现热更新和灵活的剧本编写：

- **剧本文件**：所有游戏剧情使用 Lua 编写
- **命令系统**：封装了丰富的 Lua 命令接口（对话、音效、动画等）
- **热更新支持**：Lua 脚本可在不重新打包的情况下更新
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

采用 **事件驱动** 的架构模式：

- **逻辑层（ADVManager）**：处理游戏核心逻辑和状态管理
- **视图层（ADVUIController）**：负责UI显示和动画表现
- **通信方式**：通过 Event 和 Callback 实现解耦

---

### Addressables Resource Management

使用 Unity Addressables 系统进行资源的异步加载和内存管理：

- **异步加载**：所有资源采用异步加载，避免卡顿
- **动态释放**：自动管理资源生命周期，优化内存占用
- **热更新支持**：支持远程资源更新

## Core Systems

项目已实现以下核心游戏系统：

### ADV Management System

负责游戏整体流程控制和状态管理：

- **状态机管理**：等待文本、等待按键、等待时间、等待任务等状态
- **帧循环处理**：基于 UniTask 的高性能异步帧循环
- **脚本执行**：通过 LuaScriptEngine 执行 Lua 剧本
- **对话推进**：自动文本滚动显示和用户输入处理
- **快进/自动模式**：支持跳过已读对话和自动播放

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
| `ADVScenarios_ChapterXX.tsv`       | 剧情对话翻译（按章节）                |
| `ADVMetadata.tsv`       | 章节标题和摘要                |

**Editor工具：**
- `Tools → Localization → Build JSON from TSV` - 构建 JSON 文件
- `Tools → Language → Switch to XXX` - 快速切换语言测试

**特点：**
- Lua 中的简体中文对话原文
- JSON 存储日语和英语翻译
- PlayerPrefs 持久化语言设置
- 运行时动态切换语言

---

### Character System

基于 **Spine** 的2D骨骼动画角色系统：

- **角色换装**：支持角色服装、表情、配件的切换
- **皮肤系统**：通过 Spine Skin 实现多套服装组合
- **表情控制**：独立的表情动画层级
- **动画混合**：流畅的动作过渡和混合

---

### Audio System

完整的音频管理解决方案：

- **BGM播放**：支持淡入淡出、循环播放
- **语音播放**：角色语音自动管理和停止
- **音效播放**：支持多个音效同时播放
- **音量控制**：独立的BGM、语音、音效音量设置
- **配置系统**：JSON配置音频资源路径

**音频配置示例：**
```json
{
  "bgm": {
    "Chapter01_BGM": "Audio/BGM/bgm_001"
  },
  "voice": {
    "1000001": "Audio/Voice/Chapter01/v_001"
  },
  "se": {
    "button_click": "Audio/SE/ui_click"
  }
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
- **生命周期管理**：自动追踪和释放资源
- **类型支持**：Sprite、AudioClip、GameObject、TextAsset等
- **预加载机制**：支持场景资源预加载

## Tech Stack

| 技术 | 版本 | 说明 |
|------|------|------|
| Unity | 2022.3.36f1 | 游戏引擎 |
| XLua | 最新版 | Lua脚本引擎 |
| Spine | Runtime 3.x | 2D骨骼动画 |
| DOTween | 1.2.790 | 动画补间库 |
| UniTask | 2.5.10 | 异步任务框架 |
| Newtonsoft.Json | 3.2.2 | JSON解析 |
| Addressables | 1.x | 资源管理系统 |
| TextMeshPro | 内置 | 高质量文本渲染 |

## Statement

该项目基于 **Unity 2022.3.36f1** 和 **XLua** 开发  
采用逻辑与视图分离的架构设计，  
适用于视觉小说、文字冒险游戏、Galgame 等类型的游戏开发。 

**关于 Spine 资源：**  
由于未找到合适的立绘 Spine 资源，项目暂时使用非立绘 Spine 文件进行开发和效果展示。  
本项目所用 Spine 文件版权归属 [**上海鹰角网络有限公司**](https://www.hypergryph.com) 所有。不得用于商业用途，不得损害版权方的利益。  

