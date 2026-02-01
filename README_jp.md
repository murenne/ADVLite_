# ADVLite

[English](./README.md) | [中文](./README_cn.md) | [日本語](./README_jp.md)

これは **Unity 2022.3.36f1** と **XLua** をベースに開発されたテキストアドベンチャーゲーム（ADV/AVG）フレームワークです  
ロジックとビューの分離アーキテクチャを採用し、多言語ローカライゼーションと高い拡張性をサポートしています

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

1. GitHub からプロジェクトをローカルにクローンする
2. Unity Hub でプロジェクトを開く（Unity バージョン >= 2022.3.36f1 を確保）
3. Play ボタンをクリックして実行

## Control

| 操作      | 機能説明                     |
| ----------| ------------------------ |
| `マウス左クリック`        | 会話を進める                |


## Architecture

本プロジェクトは **ロジックとビューの分離** という現代的なアーキテクチャ設計を採用し、コードの保守性と拡張性を確保しています

### Lua Script Engine

**XLua** をスクリプトエンジンとして使用し、ホットアップデートと柔軟なシナリオ作成を実現：

- **スクリプトファイル**：すべてのゲームストーリーは Lua で記述
- **コマンドシステム**：対話、サウンド、アニメーションなどの豊富な Lua コマンドインターフェース
- **ホットアップデート対応**：Lua スクリプトは再パッケージなしで更新可能
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

**イベント駆動型** アーキテクチャパターンを採用：

- **ロジック層（ADVManager）**：ゲームのコアロジックと状態管理を処理
- **ビュー層（ADVUIController）**：UI 表示とアニメーション表現を担当
- **通信方式**：Event と Callback を通じて疎結合を実現

---

### Addressables Resource Management

Unity Addressables システムを使用したリソースの非同期読み込みとメモリ管理：

- **非同期読み込み**：すべてのリソースは非同期読み込みを採用し、カクつきを回避
- **動的解放**：リソースのライフサイクルを自動管理し、メモリ使用量を最適化
- **ホットアップデート対応**：リモートリソース更新をサポート

## Core Systems

プロジェクトには以下のコアゲームシステムが実装されています：

### ADV Management System

ゲーム全体のフロー制御と状態管理を担当：

- **ステートマシン管理**：テキスト待機、キー入力待機、時間待機、タスク待機などの状態
- **フレームループ処理**：UniTask ベースの高性能非同期フレームループ
- **スクリプト実行**：LuaScriptEngine を通じて Lua スクリプトを実行
- **会話進行**：自動テキストスクロール表示とユーザー入力処理
- **早送り/オートモード**：既読会話のスキップと自動再生をサポート

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
| `ADVScenarios_ChapterXX.tsv`       | 会話翻訳（チャプター別）                |
| `ADVMetadata.tsv`       | チャプタータイトルと要約                |

**エディターツール：**
- `Tools → Localization → Build JSON from TSV` - JSON ファイルをビルド
- `Tools → Language → Switch to XXX` - テスト用の言語クイックスイッチ

**特徴：**
- Lua 内の簡体字中国語会話原文
- 日本語と英語の翻訳を JSON で保存
- PlayerPrefs による言語設定の永続化
- ランタイム動的言語切り替え

---

### Character System

**Spine** ベースの 2D スケルタルアニメーションキャラクターシステム：

- **キャラクター着せ替え**：衣装、表情、アクセサリーの切り替えをサポート
- **スキンシステム**：Spine Skin による複数の衣装組み合わせ
- **表情制御**：独立した表情アニメーションレイヤー
- **アニメーションブレンド**：スムーズなアクション遷移とブレンド

---

### Audio System

完全なオーディオ管理ソリューション：

- **BGM 再生**：フェードイン/アウト、ループ再生をサポート
- **ボイス再生**：キャラクターボイスの自動管理と停止
- **効果音再生**：複数の効果音の同時再生をサポート
- **音量制御**：BGM、ボイス、効果音の独立した音量設定
- **設定システム**：オーディオリソースパスの JSON 設定

**オーディオ設定例：**
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

レスポンシブ UI 制御システム：

- **会話ボックス**：テキスト表示、キャラクター名
- **フェードエフェクト**：シーンと UI のトランジションエフェクト
- **キープロンプト**：入力待機の視覚的プロンプト

---

### Resource Management System

Addressables ベースの非同期リソース管理：

- **非同期読み込み**：UniTask を使用した高性能非同期読み込み
- **ライフサイクル管理**：リソースの自動追跡と解放
- **型サポート**：Sprite、AudioClip、GameObject、TextAsset など
- **プリロード機構**：シーンリソースのプリロードをサポート

## Tech Stack

| 技術 | バージョン | 説明 |
|------|------|------|
| Unity | 2022.3.36f1 | ゲームエンジン |
| XLua | 最新版 | Lua スクリプトエンジン |
| Spine | Runtime 3.x | 2D スケルタルアニメーション |
| DOTween | 1.2.790 | アニメーショントゥイーンライブラリ |
| UniTask | 2.5.10 | 非同期タスクフレームワーク |
| Newtonsoft.Json | 3.2.2 | JSON パース |
| Addressables | 1.x | リソース管理システム |
| TextMeshPro | 内蔵 | 高品質テキストレンダリング |

## Statement

このプロジェクトは **Unity 2022.3.36f1** と **XLua** をベースに開発されています  
ロジックとビューの分離アーキテクチャを採用し、  
ビジュアルノベル、アドベンチャーゲーム、ギャルゲーなどのゲーム開発に適しています。

**Spine リソースについて：** 
適切な立ち絵 Spine リソースが見つからなかったため、プロジェクトでは一時的に非立ち絵 Spine ファイルを使用して開発と効果確認を行っています。   
本プロジェクトで使用されている Spine ファイルの著作権は [**上海鷹角網絡有限公司（Hypergryph）**](https://www.hypergryph.com) に帰属します。商用利用は禁止されており、著作権者の利益を損なってはなりません。  
