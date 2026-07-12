# Feature matrix

**EN:** This is the lab notebook for which C# 10-14 language features actually run inside Unity 6000.7.0a2 (Unity 6.7 alpha), across three environments: compiling via Unity's own Roslyn through a `csc.rsp` response file, running under the Mono editor/player (today's default backend), and running under the experimental CoreCLR Desktop Player. The table below is seeded from the [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) README's per-feature research (checked 2026-07-11) plus one known community report about file-scoped namespaces. It exists to be updated as real experiments happen — see `docs/unity-lab-setup.md` for the Stage 0-2 procedures that produce these results, and for the PolySharp and IDE-alignment steps some rows depend on.

Two columns are intentionally provisional right now:

- **`CoreCLR player (6.7 exp)`** starts at `untested` for every row below. Nobody has run these features under Unity's experimental CoreCLR Desktop Player yet — answering that is the whole point of this lab notebook.
- **`Compiles via csc.rsp?`** is now half-answered. The Stage 0 probe ran on 2026-07-12 (results below): the bundled Roslyn 4.10.0 accepts `-langversion` up to **12.0** (its own default) plus `preview` / `latest` / `latestmajor`. Cells therefore read `expected — accepts ≤ 12 (Stage 1)` for C# 10-12 features, `preview only, if at all (Stage 1)` for C# 13 features (Roslyn 4.10's `preview` predates final C# 13), `no — needs Stage 2 swap` for C# 14 features, and `langversion OK; compile expected to fail (runtime gap)` where the compiler itself demands runtime/BCL support Mono lacks. What Stage 0 cannot prove is whether Unity's build pipeline honors a `csc.rsp` language-version override in-editor — that is Stage 1, which still requires the Unity project to exist.

Status legend used in the `Mono editor/player` column:

- **Working** — the feature behaves correctly once it compiles.
- **Needs PolySharp** — the feature depends on an attribute or BCL type Mono doesn't ship with; [PolySharp](https://github.com/Sergio0694/PolySharp)'s source-generated polyfills supply it (install steps: `docs/unity-lab-setup.md`, section 6).
- **Not supported** — the feature needs a runtime or BCL capability Unity's Mono backend doesn't have; expect a compiler error even after Stage 1/2 raise the language version.
- **Crash** — compiles, but has been reported to crash at runtime. Treat as a hazard to confirm once, not a feature to build anything on.
- **verify locally** — a specific known report exists for this feature; reproducibility on this exact Unity version hasn't been confirmed yet.

**JA:** これは、C# 10〜14 の言語機能が Unity 6000.7.0a2（Unity 6.7 alpha）の中で実際にどう動くかを記録する lab notebook です。対象は 3 つの環境——`csc.rsp` response file 経由で Unity 自身の Roslyn でコンパイルする場合、今日の既定バックエンドである Mono editor/player で実行する場合、そして実験的な CoreCLR Desktop Player で実行する場合——です。下の表は、[UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) の README にある機能別調査（2026-07-11 確認）と、file-scoped namespace に関する既知のコミュニティ報告 1 件を種として作成しています。実際の実験結果で更新され続けることを前提にした文書です——結果を生み出す Stage 0〜2 の手順、そしていくつかの行が依存する PolySharp・IDE 整合の手順は `docs/unity-lab-setup.md` を参照してください。

以下の 2 列は、現時点では意図的に未確定のままにしています。

- **`CoreCLR player (6.7 exp)`** は、下表のすべての行で `untested` から始まります。実験的な CoreCLR Desktop Player でこれらの機能を実行した人はまだ誰もいません——それに答えることこそが、この lab notebook の存在理由です。
- **`Compiles via csc.rsp?`** は半分だけ答えが出ました。Stage 0 の probe を 2026-07-12 に実行した結果（下記参照）、同梱の Roslyn 4.10.0 は `-langversion` を **12.0**（それ自身の default）まで受け付け、加えて `preview` / `latest` / `latestmajor` を受け付けます。そのため各セルは、C# 10〜12 の機能は `expected — accepts ≤ 12 (Stage 1)`、C# 13 の機能は `preview only, if at all (Stage 1)`（Roslyn 4.10 の `preview` は C# 13 確定より前の時点のもの）、C# 14 の機能は `no — needs Stage 2 swap`、そしてコンパイラ自身が Mono にない runtime / BCL サポートを要求する行は `langversion OK; compile expected to fail (runtime gap)` としています。Stage 0 で証明できないのは、Unity の build pipeline が editor 内で `csc.rsp` の言語バージョン上書きを実際に尊重するかどうかです——それが Stage 1 であり、Unity project の存在が必要です。

`Mono editor/player` 列で使うステータスの凡例:

- **Working** —— コンパイルさえ通れば正しく動作します。
- **Needs PolySharp** —— Mono が持たない attribute や BCL の型に依存する機能です。[PolySharp](https://github.com/Sergio0694/PolySharp) の source-generated polyfill がそれを補います（install 手順は `docs/unity-lab-setup.md` 6 節）。
- **Not supported** —— Unity の Mono backend にない runtime / BCL の能力を必要とする機能です。Stage 1/2 で言語バージョンを上げてもコンパイルエラーになる見込みです。
- **Crash** —— コンパイルは通りますが、runtime でクラッシュするという報告があります。何かを積み上げる対象ではなく、一度確認しておくべき既知の危険として扱ってください。
- **verify locally** —— この機能について具体的な既知の報告がありますが、この正確な Unity バージョンでの再現性はまだ確認されていません。

## Stage 0 probe results (2026-07-12)

**EN:** Probed the editor install at `/Applications/Unity/Hub/Editor/6000.7.0a2` (macOS, arm64) without any Unity project. Findings:

- The bundled toolchain lives at `Unity.app/Contents/Resources/Scripting/DotNetSdk` — a full .NET SDK **8.0.318** — with the compiler at `sdk/8.0.318/Roslyn/bincore/csc.dll`, reporting **Roslyn 4.10.0** (`4.10.0-3.25064.8`). The design spec's earlier path guess (`Unity.app/Contents/DotNetSdkRoslyn`) does not exist in this 6.7 alpha layout; the `csc` shell script under `Resources/Scripting/MonoBleedingEdge/bin/` is vestigial (it hardcodes a Unity build-machine path and cannot run).
- `dotnet csc.dll -langversion:?` accepts: `default, 1-7.3, 8.0, 9.0, 10.0, 11.0, 12.0 (default), latestmajor, preview, latest`. Note the compiler's own default is C# 12 — Unity pins projects to 9.0 at a higher layer, which is exactly what a `csc.rsp` override targets.
- The experimental CoreCLR Desktop Player is installed: `Variations/macos_arm64_player_development_coreclr`, `Variations/macos_arm64_player_nondevelopment_coreclr`, `Variations/CoreCLRShared` (via the "Mac Build Support (CoreCLR)" Hub module).

**JA:** Unity project なしで、editor install（`/Applications/Unity/Hub/Editor/6000.7.0a2`、macOS arm64）を probe しました。判明したこと:

- 同梱 toolchain は `Unity.app/Contents/Resources/Scripting/DotNetSdk` にあり、これは丸ごとの .NET SDK **8.0.318** です。コンパイラは `sdk/8.0.318/Roslyn/bincore/csc.dll` で、**Roslyn 4.10.0**（`4.10.0-3.25064.8`）と報告します。設計書が事前に推測していたパス（`Unity.app/Contents/DotNetSdkRoslyn`）はこの 6.7 alpha のレイアウトには存在しません。また `Resources/Scripting/MonoBleedingEdge/bin/` の `csc` shell script は遺物です（Unity のビルドマシンのパスが hardcode されており実行できません）。
- `dotnet csc.dll -langversion:?` の受け付ける値: `default, 1〜7.3, 8.0, 9.0, 10.0, 11.0, 12.0 (default), latestmajor, preview, latest`。注目すべきは、コンパイラ自身の default が C# 12 だという点です——Unity はもっと上のレイヤーで project を 9.0 に固定しており、`csc.rsp` の上書きが狙うのはまさにそこです。
- 実験的な CoreCLR Desktop Player は install 済みです: `Variations/macos_arm64_player_development_coreclr`、`Variations/macos_arm64_player_nondevelopment_coreclr`、`Variations/CoreCLRShared`（Unity Hub の「Mac Build Support (CoreCLR)」module 経由）。

## Stage 1 results (2026-07-12)

**EN:** Created `Assets/Lab/Sharplings.Lab.asmdef` (`autoReferenced: false`) plus `Assets/Lab/csc.rsp` containing `-langversion:preview`, alongside three probe `MonoBehaviour`s: `ProbeRawStrings.cs` (C# 11 raw string literal), `ProbeCollectionExpressions.cs` (C# 12 collection expression), and `ProbeExtensionMembers.cs` (C# 14 extension member, guarded by `#if SHARPLINGS_STAGE2`, which is undefined today so that file's probe body compiles down to its `#else`/skip branch). Ran a headless batchmode compile (`Unity -batchmode -nographics -quit -projectPath unity -logFile ...`); exit code 0, zero `error CS` lines anywhere in the log. The generated compiler response file for this assembly (`Sharplings.Lab.rsp`, under `Library/Bee/artifacts/`) shows Unity's own `-langversion:9.0` followed later by the `-langversion:preview` appended from `Assets/Lab/csc.rsp` — and, decisively, `Sharplings.Lab.dll` still compiled cleanly with both the raw string literal (C# 11) and the collection expression (C# 12) present in source: had the earlier `9.0` won, both would have failed with a language-version error. That compile success is the proof, not just the flag ordering. This confirms Stage 1 works exactly as designed: Unity's build pipeline honors a per-assembly `csc.rsp` language-version override, scoped to `Assets/Lab/` only. Updated the `Compiles via csc.rsp?` cells for "Collection expressions (C# 12)" and "Raw string literals (C# 11)" above from `expected` to confirmed; no other cells were touched, since no other feature was exercised by these two probes.

**JA:** `Assets/Lab/Sharplings.Lab.asmdef`（`autoReferenced: false`）と、`-langversion:preview` を内容とする `Assets/Lab/csc.rsp`、そして 3 つの probe 用 `MonoBehaviour` —— `ProbeRawStrings.cs`（C# 11 の raw string literal）、`ProbeCollectionExpressions.cs`（C# 12 の collection expression）、`ProbeExtensionMembers.cs`（C# 14 の extension member。`#if SHARPLINGS_STAGE2` で guard されており、今日はこの define が未定義のため、probe の本体は `#else`/skip 分岐にコンパイルされます）—— を作成しました。headless の batchmode compile（`Unity -batchmode -nographics -quit -projectPath unity -logFile ...`）を実行した結果、exit code は 0、log 全体で `error CS` の行はゼロでした。この assembly 用に生成されたコンパイラの response file（`Library/Bee/artifacts/` 配下の `Sharplings.Lab.rsp`）を見ると、Unity 自身の `-langversion:9.0` のあとに、`Assets/Lab/csc.rsp` から追記された `-langversion:preview` が続いています —— そして決定的なのは、raw string literal（C# 11）と collection expression（C# 12）の両方が source に存在する状態で `Sharplings.Lab.dll` が問題なくコンパイルできたことです。もし先の `9.0` の方が有効だったなら、両方とも言語バージョンエラーで失敗していたはずです。証拠は flag の順序ではなく、このコンパイル成功そのものです。これは Stage 1 が設計通りに機能することの確認です —— Unity の build pipeline は、`Assets/Lab/` だけに限定された assembly 単位の `csc.rsp` 言語バージョン上書きを、実際に尊重します。上の表の「Collection expressions (C# 12)」と「Raw string literals (C# 11)」の `Compiles via csc.rsp?` セルを `expected` から確認済みへ更新しました。この 2 つの probe が検証したのはこの 2 機能だけなので、他のセルには手を加えていません。

## Table

| Feature (C# ver) | Compiles via csc.rsp? | Mono editor/player | CoreCLR player (6.7 exp) | Unity 6.8 (expected) | Notes |
|---|---|---|---|---|---|
| **Working on Mono once compilable** | — | — | — | — | — |
| Collection expressions (C# 12) | yes — confirmed 2026-07-12 (Stage 1, editor batchmode) | Working | untested | Yes — official | Exercised in `03_collections`. |
| Primary constructors (C# 12) | expected — accepts ≤ 12 (Stage 1) | Working | untested | Yes — official | Exercised in `01_records`. |
| Raw string literals (C# 11) | yes — confirmed 2026-07-12 (Stage 1, editor batchmode) | Working | untested | Yes — official | Exercised in `04_strings`. |
| List patterns (C# 11) | expected — accepts ≤ 12 (Stage 1) | Working | untested | Yes — official | Exercised in `02_patterns`. |
| UTF-8 string literals (C# 11) | expected — accepts ≤ 12 (Stage 1) | Working | untested | Yes — official | Exercised in `04_strings`. |
| File-local types (C# 11) | expected — accepts ≤ 12 (Stage 1) | Working | untested | Yes — official | Exercised in `05_types`. |
| Params collections (C# 13) | preview only, if at all (Stage 1) | Working | untested | Yes — official | Exercised in `03_collections`. |
| Extension members (C# 14) | no — needs Stage 2 swap | Working | untested | Yes — official | Exercised in `08_extensions`. |
| `field` keyword (C# 14) | no — needs Stage 2 swap | Working | untested | Yes — official | Exercised in `05_types`. |
| Null-conditional assignment (C# 14) | no — needs Stage 2 swap | Working | untested | Yes — official | Exercised in `08_extensions`. |
| `nameof` improvements — unbound generic types (C# 14) | no — needs Stage 2 swap | Working | untested | Yes — official | Exercised in `05_types`. |
| First-class span conversions (C# 14) | no — needs Stage 2 swap | Working | untested | Yes — official | Compile-time-only conversion, resolved entirely by the compiler — needs nothing new from the runtime. Exercised in `07_performance` (`075_first_class_spans.cs`). |
| **Needs PolySharp polyfill** | — | — | — | — | — |
| Required members (C# 11) | expected — accepts ≤ 12 (Stage 1) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Exercised in `05_types`. Needs PolySharp's `[RequiredMember]` / `[SetsRequiredMembers]` / `[CompilerFeatureRequired]` shims — see `unity-lab-setup.md` section 6. |
| Interpolated string handlers (C# 10) | expected — accepts ≤ 12 (Stage 1) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Needs PolySharp's `[InterpolatedStringHandler]` / `[InterpolatedStringHandlerArgument]` shims. |
| `CallerArgumentExpression` (C# 10) | expected — accepts ≤ 12 (Stage 1) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Needs PolySharp's `[CallerArgumentExpression]` shim. |
| **Not supported on Mono** | — | — | — | — | — |
| Static abstract members (C# 11) | langversion OK; compile expected to fail (runtime gap) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Generic math; runtime/BCL-dependent. Exercised in `06_generics` as a .NET-10-only feature. |
| Ref fields (C# 11) | langversion OK; compile expected to fail (runtime gap) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Requires runtime support Mono lacks. |
| Inline arrays (C# 12) | langversion OK; compile expected to fail (runtime gap) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Exercised in `07_performance` as a .NET-10-only feature. |
| Interceptors (C# 12) | langversion OK; compile expected to fail (runtime gap) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Requires compiler+runtime plumbing Mono lacks. |
| Ref struct interfaces (C# 13) | preview only, if at all (Stage 1) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Exercised in `07_performance` as a .NET-10-only feature. |
| New `Lock` type (C# 13) | preview only, if at all (Stage 1) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | New `System.Threading.Lock` type; needs a BCL type Mono doesn't ship. |
| Generic attributes (C# 11) | expected — accepts ≤ 12 (Stage 1) | **Crash** | untested | Expected fixed — unconfirmed until 6.8 ships | Compiles, but reported to crash at runtime per UnityRoslynUpdater. Exercised in `06_generics` as a .NET-10-only feature — do not push this past confirming the crash. |
| **Known reports — verify locally** | — | — | — | — | — |
| File-scoped namespaces (C# 10) | expected — accepts ≤ 12 (Stage 1) | verify locally | untested | Yes — already valid syntax since C# 10; a detection bug, if reproduced, is independent of 6.8 | Known report: compiles, but older Unity versions reportedly failed to detect a MonoBehaviour-derived class declared with `namespace Foo;` syntax, leaving the script un-attachable as a component. Reproducibility on 6000.7.0a2 is unconfirmed — smoke-test with a trivial MonoBehaviour in `Assets/Lab` before relying on this syntax elsewhere. Exercised in `05_types`. |

**EN:** Sources for the seed data: [UnityRoslynUpdater README](https://github.com/DaZombieKiller/UnityRoslynUpdater#language-support) (Working / PolySharp / Not Supported / Crash classifications, checked 2026-07-11), [PolySharp](https://github.com/Sergio0694/PolySharp) (the polyfills that back the "Needs PolySharp" rows), and the design spec's risk log (`docs/superpowers/specs/2026-07-11-sharplings-design.md`, "リスク・未検証事項") for the file-scoped-namespace report.

**JA:** 種データの情報源: [UnityRoslynUpdater README](https://github.com/DaZombieKiller/UnityRoslynUpdater#language-support)（Working / PolySharp / Not Supported / Crash の分類、2026-07-11 確認）、[PolySharp](https://github.com/Sergio0694/PolySharp)（「Needs PolySharp」行を支える polyfill 本体）、そして file-scoped namespace の報告については設計書のリスクログ（`docs/superpowers/specs/2026-07-11-sharplings-design.md` の「リスク・未検証事項」）です。
