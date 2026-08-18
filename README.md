# ADVLite

[English](./README.md) | [中文](./README_cn.md) | [日本語](./README_jp.md)

This is a text adventure game (ADV/AVG) framework based on **Unity 2022.3.36f1** and **XLua**  
Features logic-view separation architecture with multi-language localization support and high extensibility

## Tech Stack

| Category | Technology | Version | Description |
| :--- | :--- | :--- | :--- |
| **Game Engine** | Unity | 2022.3.36f1 | Core game development engine |
| **Scripting** | XLua | Latest | High-performance Lua scripting integration |
| **Animation** | Spine | Runtime 3.x | 2D skeletal animation runtime |
| **Tweening** | DOTween | 1.2.790 | Tween animation library |
| **Async Framework** | UniTask | 2.5.10 | Efficient async/await integration for Unity |
| **Data** | Newtonsoft.Json | 3.2.2 | JSON serialization and deserialization |
| **Assets** | Addressables | 1.21.21 | Asset management and loading system |
| **Graphics/UI** | TextMeshPro | 3.0.6 (UPM package) | Advanced text rendering and typography |

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

1. Clone this project from GitHub to your local machine
2. Open the project with Unity Hub (ensure Unity version >= 2022.3.36f1)
3. Click the Play button to run

## Control

| Operation      | Description                     |
| ----------| ------------------------ |
| `Left Mouse Button`        | Advance dialogue                |
| `Ctrl`        | Hard skip (without Shift)                |
| `Ctrl + Shift`        | Soft skip                |
| `Tools → Language → Switch to XXX` (Editor menu)        | Switch language, for Editor debugging only                |


## Architecture

This project adopts a modern **logic-view separation** architecture to ensure code maintainability and extensibility

### Lua Script Engine

Uses **XLua** as the scripting engine for flexible story writing:

- **Script Files**: All game stories are written in Lua
- **Command System**: Rich Lua command interfaces for dialogue, sound effects, animations, etc.
- **Externalized Scripts**: Lua scripts are loaded as files from StreamingAssets and executed at runtime, so they can be replaced without recompiling or repackaging (no remote-distribution hot-update mechanism is implemented yet)
- **Coroutine Mechanism**: Supports complex timing control and asynchronous operations

**Example Lua Script:**
```lua
function Main_Story()
    PrepareChapterAudio("Chapter01")
    FadeInStart()
    SetBackground("bg_002.png")
    SetCharacter(1, 2)  -- Show character
    SetTextWindowOpen()
    
    SetText(1000001, 1, "How are you")  -- textId, charaId, text
    SetText(1000002, 2, "I'm fine, thank you")
end
```

---

### Logic and View Separation

Adopts a **layered logic/view** architecture pattern:

- **Logic Layer (ADVManager)**: Handles core game logic and state management
- **View Layer (ADVUIController)**: Responsible for UI display and animation
- **Communication**: The logic layer holds a direct reference to the view layer and calls its public methods

---

### Addressables Resource Management

Uses Unity Addressables system for asynchronous resource loading and memory management:

- **Async Loading**: All resources use async loading to avoid stuttering
- **Reference-Counted Tracking**: Loaded resources are tracked by reference count and must be explicitly released (unreleased resources are logged)
- **Local Asset Bundles**: All groups are currently configured for local build/load; remote resource updates are not yet enabled

## Core Systems

The project has implemented the following core game systems:

### ADV Management System

Responsible for overall game flow control and state management:

- **State Machine**: Waiting for text, key input, time, tasks, and other states
- **Frame Loop**: High-performance async frame loop based on UniTask
- **Script Execution**: Execute Lua scripts through LuaScriptEngine
- **Dialogue Advancement**: Auto text scrolling and user input handling

![ADV Management](./README_Images/ADV.gif)

---

### Localization System

Professional translation architecture:

- **TSV Source Files**: Uses Tab-separated TSV format for easy translation team editing
- **JSON Build Tool**: Editor tool to convert TSV to optimized JSON with one click
- **Base + Override**: Simplified Chinese as base language, Japanese and English as overrides
- **High Performance**: Fast queries using Newtonsoft.Json's JObject

**Supported Languages:**
- 🇨🇳 Simplified Chinese (zh-CN) - Base Language
- 🇯🇵 Japanese (ja) - Override Translation
- 🇬🇧 English (en) - Override Translation

**Data Format:**

| File Type     | Description                     |
| ----------| ------------------------ |
| `ADVCharacterNames.tsv`       | Character name translations              |
| `ADVScenarios_ChapterXX.tsv`       | Dialogue translations (by chapter, Japanese/English only)                |
| `ADVMetadata.tsv`       | Chapter titles and summaries                |

> Note: Simplified Chinese is the base language; dialogue source text is written directly in Lua scripts and does not go through TSV/JSON, so there is no `ADVScenarios_ChapterXX.tsv` for zh-CN.

**Editor Tools:**
- `Tools → Localization → Build JSON from TSV` - Build JSON files
- `Tools → Language → Switch to XXX` - Quick language switching for testing

**Features:**
- Simplified Chinese dialogue source text in Lua
- JSON storage for Japanese and English translations
- PlayerPrefs persistent language settings

![Language Switch](./README_Images/Language.png)

---

### Character System

2D skeletal animation character system based on **Spine** (Spine itself supports outfit/expression switching via its Skin system, but this project does not implement it yet):

- **Expression Control**: Independent expression animation track, driven separately from body motion
- **Animation Blending**: Smooth transitions and blending based on Spine's AnimationState

---

### Audio System

Complete audio management solution:

- **BGM Playback**: Loop playback support
- **Voice Playback**: Auto character voice management and stopping
- **Sound Effects**: Multiple sound effects playback support
- **Volume Control**: Independent BGM, voice, and sound effect volume
- **Configuration System**: JSON configuration for audio resource paths

**Audio Configuration Example:**
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

Responsive UI control system:

- **Dialogue Box**: Text display, character names
- **Fade Effects**: Scene and UI transition effects
- **Key Prompt**: Visual prompt for input waiting

---

### Resource Management System

Async resource management based on Addressables:

- **Async Loading**: High-performance async loading using UniTask
- **Lifecycle Management**: Reference-counted tracking of loaded resources; requires explicit release calls
- **Type Support**: Sprite, AudioClip, GameObject, SkeletonDataAsset, etc. (localization text and similar are still loaded via `Resources.Load` and are not routed through Addressables)
- **Preload Mechanism**: Audio is preloaded in batches per chapter; other resources (Sprite/GameObject/Spine, etc.) are preloaded individually from Lua

## Statement

This project is developed based on **Unity 2022.3.36f1** and **XLua**  
Adopts logic-view separation architecture,  
Suitable for visual novels, adventure games, Galgame and other types of game development.

**About Spine Resources:**  
Due to the lack of suitable portrait Spine resources, the project temporarily uses non-portrait Spine files for development and effect demonstration.  
The Spine files used in this project are copyrighted by [**Hypergryph Co., Ltd.**](https://www.hypergryph.com). Commercial use is prohibited and must not harm the copyright holder's interests.  
