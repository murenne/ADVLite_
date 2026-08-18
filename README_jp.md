# ADVLite

[English](./README.md) | [中文](./README_cn.md) | [日本語](./README_jp.md)

これは **Unity 2022.3.36f1** と **XLua** をベースに開発されたテキストアドベンチャーゲーム（ADV/AVG）フレームワークです  
ロジックとビューの分離アーキテクチャを採用し、多言語ローカライゼーションと高い拡張性をサポートしています

## Tech Stack

| カテゴリ | テクノロジー | バージョン | 説明 |
| :--- | :--- | :--- | :--- |
| **ゲームエンジン** | Unity | 2022.3.36f1 | コアゲーム開発エンジン |
| **スクリプト** | XLua | 最新版 | 高性能なLuaスクリプト統合フレームワーク |
| **アニメーション** | Spine | Runtime 3.x | 2D骨格アニメーションランタイム |
| **アニメーション** | DOTween | 1.2.790 | トゥイーンアニメーションライブラリ |
| **非同期** | UniTask | 2.5.10 | Unity向け最適化非同期処理フレームワーク |
| **データ** | Newtonsoft.Json | 3.2.2 | JSONシリアライズ/デシリアライズツール |
| **アセット管理** | Addressables | 1.21.21 | アセット管理およびロードシステム |
| **UI/表示** | TextMeshPro | 3.0.6（UPM パッケージ） | 高品質なテキストレンダリングシステム |

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

1. GitHub からプロジェクトをローカルにクローンする
2. Unity Hub でプロジェクトを開く（Unity バージョン >= 2022.3.36f1 を確保）
3. Play ボタンをクリックして実行

## Control

| 操作      | 機能説明                     |
| ----------| ------------------------ |
| `マウス左クリック`        | 会話を進める                |
| `Ctrl`        | ハードスキップ（Shift なし）                |
| `Ctrl + Shift`        | ソフトスキップ                |
| `Tools → Language → Switch to XXX`（Editor メニュー）        | 言語切り替え、Editor でのデバッグ専用                |


## Architecture

本プロジェクトは **ロジックとビューの分離** という現代的なアーキテクチャ設計を採用し、コードの保守性と拡張性を確保しています

### Lua Script Engine

**XLua** をスクリプトエンジンとして使用し、柔軟なシナリオ作成を実現：

- **スクリプトファイル**：すべてのゲームストーリーは Lua で記述
- **コマンドシステム**：対話、サウンド、アニメーションなどの豊富な Lua コマンドインターフェース
- **スクリプトの外部化**：Lua スクリプトは StreamingAssets からファイルとして読み込まれ実行されるため、再コンパイル・再パッケージなしで置き換え可能（リモート配信によるホットアップデート機構は未実装）
- **コルーチン機構**：複雑なタイミング制御と非同期操作をサポート

**Lua スクリプトの例：**
```lua
function Main_Story()
    PrepareChapterAudio("Chapter01")
    FadeInStart()
    SetBackground("bg_002.png")
    SetCharacter(1, 2)  -- キャラクターを表示
    SetTextWindowOpen()
    
    SetText(1000001, 1, "元気ですか")  -- textId, charaId, text
    SetText(1000002, 2, "元気です、ありがとう")
end
```

---

### Logic and View Separation

**ロジックとビューの階層分離** アーキテクチャパターンを採用：

- **ロジック層（ADVManager）**：ゲームのコアロジックと状態管理を処理
- **ビュー層（ADVUIController）**：UI 表示とアニメーション表現を担当
- **通信方式**：ロジック層がビュー層への参照を直接保持し、その公開メソッドを呼び出す

---

### Addressables Resource Management

Unity Addressables システムを使用したリソースの非同期読み込みとメモリ管理：

- **非同期読み込み**：すべてのリソースは非同期読み込みを採用し、カクつきを回避
- **参照カウント管理**：読み込んだリソースを参照カウントで追跡し、明示的な解放呼び出しが必要（未解放のリソースはログに記録）
- **ローカルアセットバンドル**：現時点ではすべてのグループがローカルビルド／ロードとして構成されており、リモートリソース更新は未対応

## Core Systems

プロジェクトには以下のコアゲームシステムが実装されています：

### ADV Management System

ゲーム全体のフロー制御と状態管理を担当：

- **ステートマシン管理**：テキスト待機、キー入力待機、時間待機、タスク待機などの状態
- **フレームループ処理**：UniTask ベースの高性能非同期フレームループ
- **スクリプト実行**：LuaScriptEngine を通じて Lua スクリプトを実行
- **会話進行**：自動テキストスクロール表示とユーザー入力処理

![ADV Management](./README_Images/ADV.gif)

---

### Localization System

プロフェッショナルな翻訳アーキテクチャ：

- **TSV ソースファイル**：Tab 区切りの TSV 形式で翻訳チームが編集しやすい
- **JSON ビルドツール**：エディターツールでワンクリックで TSV を最適化された JSON に変換
- **ベース+オーバーライド機構**：簡体字中国語をベース言語とし、日本語と英語をオーバーライド
- **高性能解析**：Newtonsoft.Json の JObject を使用した高速クエリ

**対応言語：**
- 🇨🇳 簡体字中国語（zh-CN）- ベース言語
- 🇯🇵 日本語（ja）- オーバーライド翻訳
- 🇬🇧 英語（en）- オーバーライド翻訳

**データ形式：**

| ファイルタイプ     | 説明                     |
| ----------| ------------------------ |
| `ADVCharacterNames.tsv`       | キャラクター名翻訳              |
| `ADVScenarios_ChapterXX.tsv`       | 会話翻訳（チャプター別、日本語/英語のみ提供）                |
| `ADVMetadata.tsv`       | チャプタータイトルと要約                |

> 注：簡体字中国語はベース言語であり、会話原文は Lua スクリプトに直接記述されているため TSV/JSON を経由しません。そのため簡体字中国語には `ADVScenarios_ChapterXX.tsv` は存在しません。

**エディターツール：**
- `Tools → Localization → Build JSON from TSV` - JSON ファイルをビルド
- `Tools → Language → Switch to XXX` - テスト用の言語クイックスイッチ

**特徴：**
- Lua 内の簡体字中国語会話原文
- 日本語と英語の翻訳を JSON で保存
- PlayerPrefs による言語設定の永続化

![Language Switch](./README_Images/Language.png)

---

### Character System

**Spine** ベースの 2D スケルタルアニメーションキャラクターシステム（Spine 自体は Skin 機能により着せ替え・表情切り替えに対応していますが、本プロジェクトでは未実装です）：

- **表情制御**：身体の動きとは別に駆動できる、独立した表情アニメーショントラック
- **アニメーションブレンド**：Spine の AnimationState によるスムーズな遷移とブレンド

---

### Audio System

完全なオーディオ管理ソリューション：

- **BGM 再生**：ループ再生をサポート
- **ボイス再生**：キャラクターボイスの自動管理と停止
- **効果音再生**：複数の効果音の同時再生をサポート
- **音量制御**：BGM、ボイス、効果音の独立した音量設定
- **設定システム**：オーディオリソースパスの JSON 設定

**オーディオ設定例：**
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

レスポンシブ UI 制御システム：

- **会話ボックス**：テキスト表示、キャラクター名
- **フェードエフェクト**：シーンと UI のトランジションエフェクト
- **キープロンプト**：入力待機の視覚的プロンプト

---

### Resource Management System

Addressables ベースの非同期リソース管理：

- **非同期読み込み**：UniTask を使用した高性能非同期読み込み
- **ライフサイクル管理**：参照カウントで読み込み済みリソースを追跡し、明示的な解放呼び出しが必要
- **型サポート**：Sprite、AudioClip、GameObject、SkeletonDataAsset など（ローカライズ用テキストなどは引き続き `Resources.Load` で読み込まれ、Addressables を経由しません）
- **プリロード機構**：オーディオはチャプター単位でバッチプリロードされ、その他のリソース（Sprite/GameObject/Spine など）は Lua から個別にプリロードされます

## Statement

このプロジェクトは **Unity 2022.3.36f1** と **XLua** をベースに開発されています  
ロジックとビューの分離アーキテクチャを採用し、  
ビジュアルノベル、アドベンチャーゲーム、ギャルゲーなどのゲーム開発に適しています。

**Spine リソースについて：** 
適切な立ち絵 Spine リソースが見つからなかったため、プロジェクトでは一時的に非立ち絵 Spine ファイルを使用して開発と効果確認を行っています。   
本プロジェクトで使用されている Spine ファイルの著作権は [**上海鷹角網絡有限公司（Hypergryph）**](https://www.hypergryph.com) に帰属します。商用利用は禁止されており、著作権者の利益を損なってはなりません。  
