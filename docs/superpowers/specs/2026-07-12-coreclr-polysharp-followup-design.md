# Stage 2 フォローアップ設計書 — CoreCLR 検証 + PolySharp 導入

- 日付: 2026-07-12
- status: draft（ユーザー review 待ち）
- 関連: [`2026-07-11-sharplings-design.md`](2026-07-11-sharplings-design.md)、`docs/unity-lab-setup.md`、`docs/feature-matrix.md`

## 目的

Stage 2（同梱 Roslyn を .NET 10 SDK の Roslyn 5.6.0 へ差し替え、Lab assembly で C# 14 を解禁）が完了し、C# 14 の 5 機能が Mono editor backend 上で compile + runtime PASS まで確認できた。本設計は、そこで `untested` のまま残った 2 領域を埋める。

1. **CoreCLR Desktop Player 検証**: 既存 probe を CoreCLR backend の standalone player で実行し、さらに Mono で `Not supported` だった runtime 依存機能が CoreCLR で動くかを段階的に検証する。
2. **PolySharp 導入**: attribute 依存機能（required members、interpolated string handlers、`CallerArgumentExpression`）を Mono 上で polyfill し、`feature-matrix.md` の `Needs PolySharp` 行を確認済みへ進める。

いずれも `exercises/` を終えるのに必須ではない、Lab の任意探求。既存の「probe + headless 実行 + `feature-matrix.md` 記録」パターンの拡張であり、変更は `Assets/Lab/` と `docs/` に閉じる。

## 前提となる事実（2026-07-12 確認）

- **CoreCLR player ビルドサポートは install 済み**: エディタに `Variations/macos_arm64_player_development_coreclr` / `Variations/macos_arm64_player_nondevelopment_coreclr` と `libcoreclr.dylib` が実在。macOS arm64 standalone player を CoreCLR backend でビルドするターゲットは物理的に存在する。
- **6.7 alpha の CoreCLR は「新しい player 実行基盤」であって新コンパイラではない**: 言語は C# 9 相当、API 表面は .NET Standard 2.1 のまま（`2026-07-11-sharplings-design.md` 前提より）。ゆえに runtime 依存機能が CoreCLR で動く保証はなく、そこが未解決の実験課題。
- **Stage 2 swap は現在有効**: `Assets/Lab/csc.rsp` は `-langversion:preview` + `-define:SHARPLINGS_STAGE2`。C# 14 probe の guard された分岐が live。
- **既存 probe の規約**: 各 probe は `Sharplings.Lab` asmdef 内の `MonoBehaviour` で、private `void Probe()` が `PASS:` / `SKIP:` を `Debug.Log` 出力する。editor 版 `Assets/Lab/Editor/ProbeRunner.cs`（editor 専用 asmdef、`csc.rsp` 無し = 素の C# 9）が reflection で全 probe 型を集め、一時 GameObject に付けて `Probe()` を呼び、結果は `-logFile` のログから読む。
- **PolySharp は NuGet 配布の source generator**（UPM package ではない）。Unity 公式マニュアルの手動 DLL 手順（`RoslynAnalyzer` label で asmdef 単位に scope）が文書化された導入方式。

## スコープ

### 対象

- CoreCLR player 用の runtime harness とビルドスクリプト、および既存 7 probe の CoreCLR 実行と記録。
- CoreCLR 上での runtime 依存機能の探索 probe（厳選セット）。
- PolySharp の手動 DLL 導入と attribute 依存 3 機能の probe・記録。

### 対象外

- UnityRoslynUpdater ツール自体の macOS 実行（依然 unrun のまま）。
- Windows / iOS / Android 等、macOS arm64 以外のビルドターゲット。
- IL2CPP backend。
- CoreCLR の BCL に存在しない型を要する機能（new `Lock` 型 等）—— spike で BCL が晒していると判明した場合のみ再検討。
- `exercises/` と `tools/runner` への変更。

## 決定事項（brainstorming での合意）

| 論点 | 決定 |
|---|---|
| 進める領域 | CoreCLR 検証 + PolySharp 導入の 2 トラック |
| spec の粒度 | 1 本の spec に 2 セクション、実装は Track A → Track B の順 |
| CoreCLR のスコープ | 段階的（既存 probe で pipeline 確立 → runtime 依存機能へ拡張） |
| PolySharp の導入方式 | 手動 DLL（Unity 公式手順、repo 内完結、新規パッケージ依存なし） |
| player 実行 | GUI player を `-batchmode -nographics` で headless 実行 |
| 記録 | Stage 0-2 と同じ EN/JA 併記の結果ブロック + `feature-matrix.md` テーブルセル更新 |

## Track A — CoreCLR Desktop Player 検証（段階的）

### A0. Spike: backend 選択方法の確定（最初に実施）

6000.7.0a2 で macOS arm64 standalone player を CoreCLR backend で headless ビルドする正確な API を確定する。候補:

- Build Profiles asset 経由の backend 指定。
- `PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ...)` に CoreCLR 相当の `ScriptingImplementation` enum 値がある場合、それを設定。
- `BuildPlayerOptions` の standardBuild 指定 + variation フラグ。

成果物は「再現可能なビルド呼び出し 1 つ」。この spike が失敗（CoreCLR を build API から選べない）した場合は、Track A を保留してその事実を `feature-matrix.md` に記録し、Track B を先行させる。

### A1. Runtime harness + ビルドスクリプト

- **Runtime harness** `Assets/Lab/ProbeRuntimeRunner.cs`（Lab asmdef 内の `MonoBehaviour`）。`Start()` で自 assembly を reflection 走査し、private `Probe()` を持つ全 probe（自身は除外）を一時 GameObject に付けて呼び出す。各 `Debug.Log` は Player.log に出る。全 probe 実行後に `Application.Quit()`。editor 版 `ProbeRunner` の reflection ロジックをそのまま player 側へ移すだけで、**既存 7 probe は無改変で再利用**する。harness 自身は reflection と `Debug.Log` のみで C# 14 構文を使わない。
- **ビルドスクリプト** `Assets/Lab/Editor/PlayerBuilder.cs`（editor 専用 asmdef、例 `BuildCoreCLR` メソッドを `-executeMethod` で呼ぶ）。harness を載せた bootstrap scene を含む headless CoreCLR player を temp ディレクトリ（commit しない、`.gitignore` 済みの領域か OS temp）へビルドする。

### A2. 実行 + 記録

ビルドした player を `-batchmode -nographics -logFile <file>` で実行し、Player.log から `PASS/SKIP/FAIL` 行と runtime 値（例 `value.Doubled = 42`）を読む。editor 版とまったく同じログ読み方式。7 probe 全 PASS と正しい runtime 値を確認し、`feature-matrix.md` の "CoreCLR player (6.7 exp)" 列を、C# 14 の 5 行 + Stage 1 の 2 機能について更新。日付付き結果ブロック（EN/JA）を追記。

### A3. runtime 依存機能への拡張（A2 が緑になってから）

Mono で "Not supported" だった機能に新 probe を追加。各 probe は既存 C# 14 probe と同じく `#if` guard で包み、compile 可否と runtime 挙動を個別に記録する（コンパイル失敗・runtime crash も正当な記録結果でありブロッカーではない）。

**提案する厳選セット**（signal が高く、6.8 の .NET 10 BCL を必須としない範囲）:

- inline arrays (C# 12)
- ref fields (C# 11)
- static abstract members / generic math (C# 11)

各 probe を editor `ProbeRunner` と CoreCLR player harness の両方で走らせ、Mono editor と CoreCLR player の結果差分を `feature-matrix.md` に記録する。この差分こそが本トラックの核心的な成果（同じ機能が Mono では動かず CoreCLR では動く、あるいは両方で動かない、を実データで示す）。

## Track B — PolySharp 導入（手動 DLL）

### B1. analyzer のインストール

- PolySharp 1.16.0 の `.nupkg` を nuget.org から取得（ネットワーク手順）、`analyzers/dotnet/cs/PolySharp.SourceGenerators.dll` を抽出。
- `Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll` に配置し、git に commit。
- **`.dll.meta` を直接記述**（headless のため Plugin Inspector を使えない）: `PluginImporter` で Any Platform をオフ、Include Platforms から Editor / Standalone を除外、`RoslynAnalyzer` asset label を付与。Unity がこの DLL を、それを含む asmdef（`Sharplings.Lab`）に scope された Roslyn analyzer / source generator として扱うようにする。meta の YAML を正確に書くことが本トラックの唯一の難所。

### B2. probe + smoke test

- `docs/unity-lab-setup.md` 6 節の smoke test どおり、`required` member を持つ小さな型の probe を `Assets/Lab` に追加。`RequiredMemberAttribute` 系の `CS0246` が出ずにコンパイルできることで PolySharp の配線を確認。
- 残る attribute 依存 2 行の probe も追加: interpolated string handlers、`CallerArgumentExpression`。
- これらは C# 10/11 の attribute 依存機能であり **Stage 2 swap を必要としない**（PolySharp こそが Mono でこれらを可能にする手段）。既存 editor `ProbeRunner.RunAll` に含めて実行し、結果を `feature-matrix.md` の `Required members` / interpolated string handlers / `CallerArgumentExpression` 各行の `Compiles via csc.rsp?` / `Mono editor/player` セルへ記録。

## 横断事項

- **記録形式**: Stage 0-2 と同一。`feature-matrix.md` に日付付き EN/JA 結果ブロックを追記し、テーブルの該当セルを更新。
- **隔離**: すべての変更は `Assets/Lab`（asmdef 隔離）+ `docs` に閉じる。PolySharp DLL は `Assets/Lab/Plugins` 配下に commit。CoreCLR ビルド成果物は temp（commit しない）。
- **可逆性**: PolySharp DLL とその meta は削除で戻せる。CoreCLR 検証はビルド/実行のみで、editor install を書き換えない（Stage 2 swap は既に有効なので、それに乗るだけ）。
- **言語規約**: probe / harness の identifier とインフラコメントは英語のみ。`feature-matrix.md` の記録は EN/JA 併記。全角と半角の間に半角スペース。emoji 不可（PASS/SKIP や ✓/✗ は可）。
- **テスト方針**: mock / stub 不可。すべて実機ビルド / 実行のログを証跡とする。

## Acceptance criteria

- **Track A**:
  - A0: CoreCLR player を選ぶ再現可能なビルド呼び出しが確定し、spec / plan に記録されている（または「build API から選択不可」と判明し記録されている）。
  - A2: headless CoreCLR player 実行のログに、既存 7 probe の `PASS` と正しい runtime 値が出ており、exit 0。`feature-matrix.md` の "CoreCLR player" 列該当 7 セルが日付付きで更新されている。
  - A3: 厳選 3 機能の probe が追加され、Mono editor と CoreCLR player の両方で実行され、各結果（compile / runtime の成否）が `feature-matrix.md` に記録されている。
- **Track B**:
  - `required` member probe が `CS0246` なしでコンパイルできる（PolySharp 配線の証明）。
  - attribute 依存 3 機能の probe が `ProbeRunner.RunAll` で実行され、結果が `feature-matrix.md` の該当 3 行に記録されている。
- **共通**: 変更が `Assets/Lab` + `docs` に閉じており、`exercises/` と `tools/runner` に差分が無い。

## リスクと未検証事項

1. **CoreCLR backend 選択 API**: build API から CoreCLR を選べるかは A0 spike まで未知。選べない場合は Track A を保留し記録する。
2. **runtime 依存機能の不確実性**: A3 の probe は .NET Standard 2.1 の API 表面ゆえコンパイル / 実行が通らない可能性がある。これは記録対象であり失敗ではない。
3. **ネットワーク依存**: PolySharp `.nupkg` 取得にネットワークが必要。取得できない場合は B トラックを保留する。
4. **Stage 2 swap の揮発性**: エディタ更新は swap を黙って戻すため、CoreCLR ビルド内の C# 14 probe も影響を受ける。実行前に `unity-lab-setup.md` 手順 5（オフライン検証）で swap の生存を確認する。
5. **PolySharp meta の正確性**: `.dll.meta` の `PluginImporter` 設定と `RoslynAnalyzer` label を手書きするため、誤ると analyzer が有効化されず smoke test が `CS0246` で落ちる。smoke test がその検知手段になる。
