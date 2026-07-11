# Feature matrix

**EN:** This is the lab notebook for which C# 10-14 language features actually run inside Unity 6000.7.0a2 (Unity 6.7 alpha), across three environments: compiling via Unity's own Roslyn through a `csc.rsp` response file, running under the Mono editor/player (today's default backend), and running under the experimental CoreCLR Desktop Player. The table below is seeded from the [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) README's per-feature research (checked 2026-07-11) plus one known community report about file-scoped namespaces. It exists to be updated as real experiments happen — see `docs/unity-lab-setup.md` for the Stage 0-2 procedures that produce these results, and for the PolySharp and IDE-alignment steps some rows depend on.

Two columns are intentionally provisional right now:

- **`CoreCLR player (6.7 exp)`** starts at `untested` for every row below. Nobody has run these features under Unity's experimental CoreCLR Desktop Player yet — answering that is the whole point of this lab notebook.
- **`Compiles via csc.rsp?`** is `unverified (probe: Stage 0)` for every row below, because the exact C# language version the bundled 6000.7.0a2 Roslyn actually accepts is not yet known — the Unity project doesn't exist yet, so nobody has run the Stage 0 probe (`docs/unity-lab-setup.md`, section 2). The one situation where this column could be answered without probing — a feature whose introducing C# version is 9 or lower, which Unity already compiles by default — doesn't apply to any row here: every seeded feature is C# 10 or later.

Status legend used in the `Mono editor/player` column:

- **Working** — the feature behaves correctly once it compiles.
- **Needs PolySharp** — the feature depends on an attribute or BCL type Mono doesn't ship with; [PolySharp](https://github.com/Sergio0694/PolySharp)'s source-generated polyfills supply it (install steps: `docs/unity-lab-setup.md`, section 6).
- **Not supported** — the feature needs a runtime or BCL capability Unity's Mono backend doesn't have; expect a compiler error even after Stage 1/2 raise the language version.
- **Crash** — compiles, but has been reported to crash at runtime. Treat as a hazard to confirm once, not a feature to build anything on.
- **verify locally** — a specific known report exists for this feature; reproducibility on this exact Unity version hasn't been confirmed yet.

**JA:** これは、C# 10〜14 の言語機能が Unity 6000.7.0a2（Unity 6.7 alpha）の中で実際にどう動くかを記録する lab notebook です。対象は 3 つの環境——`csc.rsp` response file 経由で Unity 自身の Roslyn でコンパイルする場合、今日の既定バックエンドである Mono editor/player で実行する場合、そして実験的な CoreCLR Desktop Player で実行する場合——です。下の表は、[UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) の README にある機能別調査（2026-07-11 確認）と、file-scoped namespace に関する既知のコミュニティ報告 1 件を種として作成しています。実際の実験結果で更新され続けることを前提にした文書です——結果を生み出す Stage 0〜2 の手順、そしていくつかの行が依存する PolySharp・IDE 整合の手順は `docs/unity-lab-setup.md` を参照してください。

以下の 2 列は、現時点では意図的に未確定のままにしています。

- **`CoreCLR player (6.7 exp)`** は、下表のすべての行で `untested` から始まります。実験的な CoreCLR Desktop Player でこれらの機能を実行した人はまだ誰もいません——それに答えることこそが、この lab notebook の存在理由です。
- **`Compiles via csc.rsp?`** は、下表のすべての行で `unverified (probe: Stage 0)` です。理由は、6000.7.0a2 に同梱された Roslyn が実際にどの C# バージョンまで受け付けるかがまだ分かっていないためです——Unity project 自体がまだ存在しないため、誰も Stage 0 の probe（`docs/unity-lab-setup.md` 2 節）を実行していません。probe なしでもこの列に答えられる唯一のケース——導入 C# バージョンが 9 以下で、Unity が既定でコンパイルできる機能——は、ここには該当しません。ここに載せた機能はすべて C# 10 以降です。

`Mono editor/player` 列で使うステータスの凡例:

- **Working** —— コンパイルさえ通れば正しく動作します。
- **Needs PolySharp** —— Mono が持たない attribute や BCL の型に依存する機能です。[PolySharp](https://github.com/Sergio0694/PolySharp) の source-generated polyfill がそれを補います（install 手順は `docs/unity-lab-setup.md` 6 節）。
- **Not supported** —— Unity の Mono backend にない runtime / BCL の能力を必要とする機能です。Stage 1/2 で言語バージョンを上げてもコンパイルエラーになる見込みです。
- **Crash** —— コンパイルは通りますが、runtime でクラッシュするという報告があります。何かを積み上げる対象ではなく、一度確認しておくべき既知の危険として扱ってください。
- **verify locally** —— この機能について具体的な既知の報告がありますが、この正確な Unity バージョンでの再現性はまだ確認されていません。

## Table

| Feature (C# ver) | Compiles via csc.rsp? | Mono editor/player | CoreCLR player (6.7 exp) | Unity 6.8 (expected) | Notes |
|---|---|---|---|---|---|
| **Working on Mono once compilable** | — | — | — | — | — |
| Collection expressions (C# 12) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `03_collections`. |
| Primary constructors (C# 12) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `01_records`. |
| Raw string literals (C# 11) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `04_strings`. |
| List patterns (C# 11) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `02_patterns`. |
| UTF-8 string literals (C# 11) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `04_strings`. |
| File-local types (C# 11) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `05_types`. |
| Params collections (C# 13) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `03_collections`. |
| Extension members (C# 14) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `08_extensions`. |
| `field` keyword (C# 14) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `05_types`. |
| Null-conditional assignment (C# 14) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `08_extensions`. |
| `nameof` improvements — unbound generic types (C# 14) | unverified (probe: Stage 0) | Working | untested | Yes — official | Exercised in `05_types`. |
| **Needs PolySharp polyfill** | — | — | — | — | — |
| Required members (C# 11) | unverified (probe: Stage 0) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Exercised in `05_types`. Needs PolySharp's `[RequiredMember]` / `[SetsRequiredMembers]` / `[CompilerFeatureRequired]` shims — see `unity-lab-setup.md` section 6. |
| Interpolated string handlers (C# 10) | unverified (probe: Stage 0) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Needs PolySharp's `[InterpolatedStringHandler]` / `[InterpolatedStringHandlerArgument]` shims. |
| `CallerArgumentExpression` (C# 10) | unverified (probe: Stage 0) | Needs PolySharp | untested | Yes — official (PolySharp no longer needed) | Needs PolySharp's `[CallerArgumentExpression]` shim. |
| **Not supported on Mono** | — | — | — | — | — |
| Static abstract members (C# 11) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Generic math; runtime/BCL-dependent. Exercised in `06_generics` as a .NET-10-only feature. |
| Ref fields (C# 11) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Requires runtime support Mono lacks. |
| Inline arrays (C# 12) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Exercised in `07_performance` as a .NET-10-only feature. |
| Interceptors (C# 12) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Requires compiler+runtime plumbing Mono lacks. |
| Ref struct interfaces (C# 13) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | Exercised in `07_performance` as a .NET-10-only feature. |
| New `Lock` type (C# 13) | unverified (probe: Stage 0) | Not supported | untested | Yes — official (needs the .NET 10 runtime 6.8 ships with) | New `System.Threading.Lock` type; needs a BCL type Mono doesn't ship. |
| Generic attributes (C# 11) | unverified (probe: Stage 0) | **Crash** | untested | Expected fixed — unconfirmed until 6.8 ships | Compiles, but reported to crash at runtime per UnityRoslynUpdater. Exercised in `06_generics` as a .NET-10-only feature — do not push this past confirming the crash. |
| **Known reports — verify locally** | — | — | — | — | — |
| File-scoped namespaces (C# 10) | unverified (probe: Stage 0) | verify locally | untested | Yes — already valid syntax since C# 10; a detection bug, if reproduced, is independent of 6.8 | Known report: compiles, but older Unity versions reportedly failed to detect a MonoBehaviour-derived class declared with `namespace Foo;` syntax, leaving the script un-attachable as a component. Reproducibility on 6000.7.0a2 is unconfirmed — smoke-test with a trivial MonoBehaviour in `Assets/Lab` before relying on this syntax elsewhere. Exercised in `05_types`. |

**EN:** Sources for the seed data: [UnityRoslynUpdater README](https://github.com/DaZombieKiller/UnityRoslynUpdater#language-support) (Working / PolySharp / Not Supported / Crash classifications, checked 2026-07-11), [PolySharp](https://github.com/Sergio0694/PolySharp) (the polyfills that back the "Needs PolySharp" rows), and the design spec's risk log (`docs/superpowers/specs/2026-07-11-sharplings-design.md`, "リスク・未検証事項") for the file-scoped-namespace report.

**JA:** 種データの情報源: [UnityRoslynUpdater README](https://github.com/DaZombieKiller/UnityRoslynUpdater#language-support)（Working / PolySharp / Not Supported / Crash の分類、2026-07-11 確認）、[PolySharp](https://github.com/Sergio0694/PolySharp)（「Needs PolySharp」行を支える polyfill 本体）、そして file-scoped namespace の報告については設計書のリスクログ（`docs/superpowers/specs/2026-07-11-sharplings-design.md` の「リスク・未検証事項」）です。
