# sharplings 設計書

- 日付: 2026-07-11
- status: draft（ユーザー review 待ち）

## 目的

Unity エンジニア（知識が C# 9 で停止）が、C# 10〜14 の言語機能を ziglings 形式で学ぶための学習用 repo。「C# 9 まではこう書いていたが、最新 C# ではこう書く」という対比を軸に、Unity ゲーム開発の文脈へ接続する。

## 前提となる事実（2026-07-11 調査）

- Unity 6.7 alpha（6000.7.0a2）は CoreCLR **Desktop Player** を実験的に搭載するが、言語は **C# 9 のまま**、API は **.NET Standard 2.1 のまま**。
- .NET 10 toolchain + **C# 14** が解禁されるのは **Unity 6.8**（2026 年後半、Mono 廃止）。
- 非公式の「裏技」は 2 段階で存在する:
  1. `csc.rsp` に `-langversion:preview` — Unity 同梱 Roslyn が受け付ける範囲で新構文が通る。6000.7.0a2 の同梱 Roslyn version は未確認（install 完了後にローカルで確認する）。
  2. [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) — エディタの Roslyn を手元の .NET SDK のものに差し替え、C# 14 構文までコンパイル可能にする。
- 言語機能には「コンパイル時に完結する機能」（Unity で動く: collection expressions、primary constructors、extension members、`field` keyword 等）と「runtime / BCL 依存の機能」（Mono では動かない: static abstract members、ref fields、inline arrays、ref struct interfaces、generic attributes 等）がある。required members 等の attribute 依存機能は [PolySharp](https://github.com/Sergio0694/PolySharp) の polyfill で動く。

情報源:

- [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299)
- [Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279)
- [UnityRoslynUpdater README](https://github.com/DaZombieKiller/UnityRoslynUpdater)（機能別動作表）
- [Unity missing support of C# version 10 or greater](https://discussions.unity.com/t/unity-missing-support-of-c-version-10-or-greater-version-9-as-default/1617516)

## 戦略

ハイブリッド構成を取る。

1. **.NET 10 側（主戦場）**: 純粋な .NET 10 console 環境で C# 10〜14 を ziglings 形式で学ぶ。高速な feedback loop。
2. **Unity 側（接続先)**: Unity 固有 idiom の Before/After 対比集 + 裏技による実験ゾーン。6.8 到来時に After コードがそのまま生きる構造にする。

## 決定事項（brainstorming での合意）

| 論点 | 決定 |
|---|---|
| repo 構成 | ハイブリッド（.NET 10 演習 + Unity 6.7 プロジェクト併設） |
| 章立ての軸 | テーマ別 + 各 exercise に C# version タグ |
| チェッカー | ziglings 忠実型の専用 runner（C# 14 製 console app） |
| Unity 側の形式 | Before/After 対比集 + 実験ゾーンの 2 層 |
| ヒント方針 | ヒントのみ、解答は repo に含めない |
| 解説の言語 | 英語・日本語の併記 |

## Repo 構成

```
sharplings/
├── README.md            # セットアップ・進め方（EN/JA 併記）
├── global.json          # .NET 10 SDK を pin
├── mise.toml            # dotnet 10 を指定
├── exercises/           # ziglings 形式の演習（.NET 10 file-based apps）
│   ├── 00_intro/
│   ├── 01_records/
│   ├── ...
├── tools/
│   └── runner/          # チェッカー本体（C# 14 console app + xUnit テスト）
├── unity/               # Unity 6000.7.0a2 プロジェクト（ユーザーが Unity Hub で作成）
│   └── Assets/
│       ├── Contrasts/   # 公式ゾーン: Before (C# 9) / After~ (6.8 待ち)
│       └── Lab/         # 実験ゾーン: 裏技で modern C# を今動かす
└── docs/
    ├── unity-lab-setup.md       # 裏技の setup 手順（EN/JA）
    ├── feature-matrix.md        # 言語機能 × 環境の動作表（実験で更新する lab notebook）
    └── superpowers/specs/       # 設計書・実装計画
```

## .NET 演習（exercises/）

### 形式

- 1 exercise = 1 つの `.cs` ファイル。.NET 10 の file-based apps（`dotnet run file.cs`）で単体実行する。この機能自体が C# 14 世代の教材。
- 初期状態は「コンパイルが通らない」または「実行結果が期待と違う」。ユーザーが修正して runner を通す。
- 解答は repo に含めない。

### ファイル構造

```csharp
// [C# 12] Collection expressions
//
// EN: <English explanation>
// JA: <日本語解説>
//
// Unity note:
// EN: <how this maps to Unity idioms>
// JA: <Unity ではこう書いていたはず、という接続>
//
// Docs: https://learn.microsoft.com/...

<壊れた、または穴のあるコード>

// HINT EN: ...
// HINT JA: ...
//
// EXPECTED OUTPUT:
// <runner が比較する期待出力>
```

- 解説・ヒント・README は EN/JA 併記。識別子と runner 実装コメントは英語。
- `???` プレースホルダ、意図的なコンパイルエラー、間違った実装の 3 パターンを使い分ける。

### カリキュラム（intro + 8 章、約 40〜50 exercises）

| 章 | テーマ | 主な機能（version） |
|---|---|---|
| 00_intro | file-based apps と repo の使い方 | .NET 10 `dotnet run file.cs`、top-level statements 復習 |
| 01_records | record struct、primary constructor | record structs (10)、primary constructors (12) |
| 02_patterns | パターンマッチングの拡張 | extended property patterns (10)、list patterns (11) |
| 03_collections | コレクション初期化の現代化 | collection expressions + spread (12)、params collections (13) |
| 04_strings | 文字列の現代化 | interpolated string 改善 (10)、raw strings・UTF-8 literals (11) |
| 05_types | 型定義の現代化 | file-scoped namespace・global using (10)、required members (11)、`field` keyword (14) |
| 06_generics | generic math | static abstract members (11)、generic attributes (11) |
| 07_performance | 低 allocation プログラミング | Span 実践、inline arrays (12)、ref struct 進化 (13)、Span 暗黙変換 (14) |
| 08_extensions | 拡張の再発明 | extension members (14)、null 条件付き代入 (14) |

- 章の並びは易→難。各 exercise の解説に必ず Unity note を入れる。
- 06/07 章の runtime 依存機能には「Unity (Mono) では動かない、6.8 / CoreCLR から」という注記を含め、feature-matrix.md へ link する。

## Runner（tools/runner）

### 動作

1. `dotnet run --project tools/runner` で起動。`exercises/` を番号順に走査。
2. 各 exercise を `dotnet run <file>` で個別コンパイル・実行し、`// EXPECTED OUTPUT:` block と stdout を比較。
3. 最初の失敗で停止。失敗種別（コンパイルエラー / 実行時例外 / 出力不一致）に応じた表示 + HINT を提示。進捗（例: `12/42`）を表示。
4. ファイル内容の SHA-256 hash で合格済み exercise を cache（`.sharplings-cache.json`、gitignore）し、再実行を skip。
5. 実行 timeout（無限ループ対策、既定 10 秒）。
6. `--verify-broken` mode: 全 exercise が「未修正状態では失敗する」ことを検証する（教材の品質保証、CI 用）。

### 実装方針

- C# 14 で書き、runner 自体を最新 C# の実例にする。
- テストは xUnit。mock / stub は使わず、fixture の実 exercise ファイルを実際に `dotnet` で実行して検証する。

## Unity 側（unity/）

### プロジェクト作成の分担

Unity プロジェクトはユーザーが Unity Hub で `unity/` に作成する（alpha 版の生成物を手書きしない）。スクリプト・asmdef・csc.rsp・docs は Claude が追加する。

### Zone 1: Contrasts（公式ゾーン）

- `Assets/Contrasts/<topic>/Before/` — C# 9 の Unity idiom。今日の 6.7 でコンパイル可。
- `Assets/Contrasts/<topic>/After~/` — 最新 C# での書き換え。末尾 `~` フォルダは Unity のコンパイル対象外なので、6.8 到来時に `~` を外すだけで有効化できる。
- 各 topic に README（EN/JA）: なぜ After が良いか、どの version から使えるか。

対比テーマ（6〜8 本）:

1. null 地獄 — Unity の偽 null と `?.` の罠 → 最新の null 対処 + null 条件付き代入
2. データ運搬 class → record / required members（Unity serialization との相性の注意書き付き）
3. enum + switch 文の state machine → switch 式 + パターンマッチング
4. コルーチン → async/await + Awaitable
5. 配列初期化・object pool → collection expressions / Span
6. 静的 utility class → extension members (C# 14)

### Zone 2: Lab（実験ゾーン）

- `Assets/Lab/` を独立した asmdef で隔離し、同フォルダの `csc.rsp` で `-langversion` を上げる。
- 段階 1（非侵襲）: 同梱 Roslyn + `-langversion:preview` でどこまで通るか検証。
- 段階 2（侵襲）: UnityRoslynUpdater で Roslyn を差し替え、C# 14 構文を解禁。PolySharp（source generator）で attribute 依存機能を polyfill。
- 実験結果は `docs/feature-matrix.md`（言語機能 × 環境: Mono editor / CoreCLR player / 将来の 6.8）に記録し、lab notebook として育てる。CoreCLR Desktop Player で runtime 依存機能が動くかの検証もここで行う。
- IDE 整合: 生成 csproj の LangVersion 問題への対処（`com.unity.ide.visualstudio` 2.0.24+ または [CsprojLangVersionProcessor](https://github.com/annulusgames/CsprojLangVersionProcessor)）を `docs/unity-lab-setup.md` に記載。

## テスト・検証

- runner: xUnit による実テスト（mock なし）。
- 教材: `--verify-broken` で全 exercise の「意図した壊れ方」を検証。
- Unity 側: Contrasts の Before が 6.7 でコンパイル・動作することをエディタで確認。Lab は feature-matrix.md の実験記録が検証を兼ねる。

## リスク・未検証事項

- 6000.7.0a2 同梱の Roslyn version が未確認（install 完了後、`Unity.app/Contents/DotNetSdkRoslyn` で確認する）。
- UnityRoslynUpdater の macOS / Unity 6.7 alpha への対応が未検証。動かない場合は Roslyn の手動差し替え手順を docs 化して代替する。
- file-scoped namespace は「コンパイルは通るが Unity が MonoBehaviour を検出できない」既知報告あり。現行 version での再現性を Lab で検証する。
- alpha 版 Unity の editor update により Lab の前提（差し替えた Roslyn 等）が壊れる可能性。restore 手順を docs に含める。
- 裏技はすべて非公式・自己責任。Lab を asmdef で隔離することで本体（Contrasts / 将来の実プロジェクト流用）への影響を遮断する。

## Out of scope

- IL2CPP での動作検証
- OSS として公開するための整備（license 選定・CONTRIBUTING 等）は将来判断
- Unity 側の ziglings 形式チェッカー（Unity Test Runner 化）— Before/After + Lab で十分と判断

## 分担

- **Claude**: runner 実装、exercise 作成（壊れた状態 + ヒント）、docs、Unity 側スクリプト・asmdef・csc.rsp。
- **ユーザー**: exercise を解く（詰まったら公式 docs を読む。AI への質問は最後の手段）、Unity プロジェクト作成、Lab の実験実施と feature-matrix.md の更新。
