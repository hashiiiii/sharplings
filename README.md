# sharplings

A ziglings-style course for modern C# (10-14), built for Unity engineers stuck at C# 9.
Unity エンジニアが最新 C#（10〜14）を ziglings 形式で学ぶための、C# 9 で足止めされている人向けの repo。

## 1. What this is

**EN:** sharplings is a ziglings-style exercise course for the C# language features introduced in C# 10 through C# 14. It's built for Unity engineers whose day job keeps them on C# 9 — Unity's current default `LangVersion`, paired with a .NET Standard 2.1 API surface. Each exercise is one broken `.cs` file: it either fails to compile or compiles and produces the wrong output. You fix it, a runner checks it, you move to the next one. There are 47 exercises across 9 chapters, all gate-verified — every exercise is confirmed to fail as committed, and every exercise carries an EN + JA explanation, an EN + JA hint, and an `EXPECTED OUTPUT` block the runner diffs your program's stdout against.

This project is inspired by [ziglings](https://ziglings.org/) — the same "fix one broken file, run a checker, repeat" workflow — but it is an independent project, not affiliated with or endorsed by the ziglings project.

**JA:** sharplings は、C# 10 から C# 14 で追加された言語機能を学ぶための、ziglings 形式の演習コースです。対象は、日々の仕事が C# 9 —— Unity の現行デフォルト `LangVersion` と、.NET Standard 2.1 の API —— で止まっている Unity エンジニアです。各演習は 1 つの壊れた `.cs` ファイルです。コンパイルが通らないか、通っても出力が期待と違います。直して runner でチェックし、次へ進みます。全 9 章 47 exercises、すべて gate 検証済みです。commit された時点で必ず失敗すること、EN/JA の解説・EN/JA の hint・runner が stdout を diff する `EXPECTED OUTPUT` ブロックを備えていることを確認済みです。

このプロジェクトは [ziglings](https://ziglings.org/) からインスピレーションを受けています——「1 つの壊れたファイルを直し、チェッカーを走らせ、繰り返す」という同じワークフローです——が、独立したプロジェクトであり、ziglings プロジェクトと提携・公認関係にあるわけではありません。

## 2. Why .NET 10 and not Unity (yet)

**EN:** Short version: Unity doesn't run C# 14 yet, and won't for a while.

- Unity 6.7 alpha (6000.7.0a2) ships an experimental CoreCLR **Desktop Player**, but the scripting language stays at **C# 9** and the API surface stays **.NET Standard 2.1**. CoreCLR here is a new player runtime backend, not a new compiler.
- **C# 14** and the **.NET 10** toolchain arrive with **Unity 6.8** (where Mono is removed).
- Sources: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299), [Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279).

So the fastest way to actually learn C# 10-14 today is a plain .NET 10 console environment — no editor recompiles, no domain reloads, no waiting on a Unity beta. `exercises/` in this repo is exactly that: fast, disposable, file-based .NET 10 apps. The `unity/` side (section 6) is where the same features eventually land inside a real Unity project, once 6.8 ships.

**JA:** 結論から言うと、Unity は C# 14 をまだ実行できません。しばらくの間は、です。

- Unity 6.7 alpha（6000.7.0a2）は実験的な CoreCLR **Desktop Player** を搭載しますが、スクリプト言語は **C# 9** のまま、API は **.NET Standard 2.1** のままです。ここでの CoreCLR は新しい player 実行基盤であり、新しいコンパイラではありません。
- **C# 14** と **.NET 10** の toolchain が来るのは **Unity 6.8**（Mono 廃止）からです。
- 情報源: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299)、[Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279)。

つまり、今 C# 10〜14 を実際に学ぶ一番早い方法は、素の .NET 10 console 環境です——エディタの再コンパイルも、ドメインリロードも、次の Unity beta 待ちもありません。この repo の `exercises/` はまさにそれで、高速で使い捨てできる file-based な .NET 10 アプリです。`unity/` 側（6 節）は、6.8 が来たときに同じ機能が実際の Unity project に着地する場所です。

## 3. Setup

**EN:**

1. Install the pinned .NET 10 SDK. This repo uses [mise](https://mise.jdx.dev/) (`mise.toml` pins `dotnet = "10.0.301"`). If you have mise installed:

   ```
   mise install
   ```

   Any .NET 10.0.3xx SDK works — `global.json` pins the exact version with `rollForward: latestFeature`, so the SDK resolver will accept the closest 10.0.3xx patch you have installed.

2. Verify:

   ```
   dotnet --version
   ```

   Run from the repo root, this should print `10.0.301` (or your installed 10.0.3xx patch).

**JA:**

1. pin された .NET 10 SDK を install します。この repo は [mise](https://mise.jdx.dev/) を使います（`mise.toml` が `dotnet = "10.0.301"` を pin）。mise が入っていれば:

   ```
   mise install
   ```

   .NET 10.0.3xx の SDK であればどれでも動きます。`global.json` が正確な version と `rollForward: latestFeature` を pin しているため、SDK resolver は手元にある最も近い 10.0.3xx patch を受け入れます。

2. 確認:

   ```
   dotnet --version
   ```

   repo root で実行すると、`10.0.301`（または手元の 10.0.3xx patch version）が表示されるはずです。

## 4. How to work

**EN:** From the repo root:

```
dotnet run --project tools/runner
```

The runner walks `exercises/` in order and stops at the first exercise that doesn't pass, printing a hint. Fix that file, re-run the same command. Passed exercises are cached by content hash (section 7), so re-runs only re-check what you actually changed.

Each exercise file is self-contained: an EN + JA explanation of the feature, a Unity note connecting it to a Unity idiom, a docs link, the broken code itself (look for a `???` placeholder, a straight-up compile error, or a subtly wrong implementation), an EN + JA `HINT`, and an `EXPECTED OUTPUT` block the runner diffs your program's stdout against.

Solutions are intentionally not included in this repo. If you get stuck, work the ladder in order:

1. Re-read `EXPECTED OUTPUT` — it usually pins down exactly what's wrong.
2. Re-read the `HINT`.
3. Follow the `Docs:` link.
4. Ask an AI — last resort, not first.

**JA:** リポジトリのルートから:

```
dotnet run --project tools/runner
```

runner は `exercises/` を順番に走査し、最初に通らない exercise で止まって hint を表示します。そのファイルを直して同じコマンドを再実行してください。合格済みの exercise は content hash で cache される（7 節）ため、再実行では実際に変更した箇所だけが再チェックされます。

各 exercise ファイルは自己完結しています: 機能の EN + JA 解説、Unity のイディオムへつなげる Unity note、docs へのリンク、壊れたコード本体（`???` プレースホルダ、素直なコンパイルエラー、あるいは微妙に間違った実装のいずれか）、EN + JA の `HINT`、そして runner がプログラムの stdout と diff する `EXPECTED OUTPUT` ブロックです。

解答はこの repo に意図的に含まれていません。詰まったら、この順で進めてください。

1. `EXPECTED OUTPUT` を読み直す —— たいてい何が間違っているかがそこで特定できます。
2. `HINT` を読み直す。
3. `Docs:` リンクをたどる。
4. AI に聞く —— 最後の手段であって、最初の手段ではありません。

## 5. Course map

**EN:**

| Chapter | Exercises | C# / .NET | Focus |
|---|---|---|---|
| `00_intro` | 3 | .NET 10 | file-based apps (`dotnet run file.cs`), top-level statements refresher |
| `01_records` | 6 | C# 10, 12 | record structs, primary constructors |
| `02_patterns` | 6 | C# 10, 11 | extended property patterns, list & slice patterns |
| `03_collections` | 6 | C# 12, 13 | collection expressions + spread, params collections |
| `04_strings` | 5 | C# 10, 11 | interpolated string improvements, raw strings, UTF-8 literals |
| `05_types` | 7 | C# 10-14 | file-scoped namespaces, required members, the `field` keyword |
| `06_generics` | 4 | C# 11 | generic math, static abstract members — runtime-dependent (see section 6; not available in Unity's Mono runtime today) |
| `07_performance` | 6 | C# 12-14 | `Span<T>`, inline arrays, `ref struct` evolution |
| `08_extensions` | 4 | C# 14 | extension members + capstone exercise |

47 exercises total. Verified with `--verify-broken`: 0 violations — every exercise fails as committed and carries both hints and an expected-output block.

**JA:**

| 章 | Exercises 数 | C# / .NET | 内容 |
|---|---|---|---|
| `00_intro` | 3 | .NET 10 | file-based apps（`dotnet run file.cs`）、top-level statements の復習 |
| `01_records` | 6 | C# 10, 12 | record struct、primary constructor |
| `02_patterns` | 6 | C# 10, 11 | 拡張されたプロパティパターン、list / slice パターン |
| `03_collections` | 6 | C# 12, 13 | collection expression + spread、params collections |
| `04_strings` | 5 | C# 10, 11 | 文字列補間の改善、raw string、UTF-8 literal |
| `05_types` | 7 | C# 10-14 | file-scoped namespace、required members、`field` キーワード |
| `06_generics` | 4 | C# 11 | generic math、static abstract members —— runtime 依存（6 節参照。今日の Unity の Mono runtime では動きません） |
| `07_performance` | 6 | C# 12-14 | `Span<T>`、inline array、`ref struct` の進化 |
| `08_extensions` | 4 | C# 14 | extension members + capstone exercise |

全 47 exercises。`--verify-broken` で検証済み: 違反 0 件 —— すべての exercise が commit された時点で失敗し、hint と expected-output ブロックの両方を備えています。

## 6. Unity side

**EN:** The Unity project itself (`unity/`) doesn't exist in this repo yet — you create it via Unity Hub (Unity 6000.7.0a2 or later). Scripts, `asmdef` files, `csc.rsp`, and docs get added on top once it exists. Two docs already in this repo are the Unity-side entry points once that lands:

- `docs/unity-lab-setup.md` — setup steps for the unofficial-trick lab that lets modern C# syntax compile inside Unity today.
- `docs/feature-matrix.md` — a living table of which language features actually run where (Mono editor / CoreCLR player / future 6.8), updated as Lab experiments produce results.

Two zones are planned under `Assets/`:

- **`Contrasts/<topic>/`** — official, no-tricks-required Before/After pairs. `Before/` holds the C# 9 Unity idiom (compiles today, on 6.7). `After~/` holds the modern-C# rewrite. Note the trailing `~`: folders and files ending in `~` are invisible to Unity's compilation pipeline (the same convention Unity itself uses for hidden folders), so `After~/` sits in the project without touching today's build. When Unity 6.8 ships C# 14 support, you rename `After~/` to `After/` and it goes live as-is.
- **`Lab/`** — an experimental zone, isolated in its own `asmdef` with a local `csc.rsp`, for pushing past what Unity 6.7 officially supports, using the tricks documented in `unity-lab-setup.md`.

**JA:** Unity project 自体（`unity/`）は、この repo にはまだ存在しません。Unity Hub で作成します（Unity 6000.7.0a2 以降）。スクリプト・`asmdef`・`csc.rsp`・docs は、それができてから追加で載せます。それが来たときの Unity 側の入り口となるのは、この repo に既にある次の 2 つの docs です。

- `docs/unity-lab-setup.md` —— 今日の Unity 内で最新 C# 構文をコンパイルできるようにする、非公式の裏技 lab の setup 手順。
- `docs/feature-matrix.md` —— どの言語機能がどこで実際に動くか（Mono editor / CoreCLR player / 将来の 6.8）を記録し続ける表。Lab の実験結果で更新されていきます。

`Assets/` 配下には 2 つの zone を計画しています。

- **`Contrasts/<topic>/`** —— 裏技不要の公式な Before/After 対。`Before/` には C# 9 の Unity イディオム（今日の 6.7 でコンパイル可）、`After~/` には最新 C# での書き換えが入ります。末尾の `~` に注目してください。`~` で終わるフォルダ・ファイルは Unity のコンパイルパイプラインから見えません（Unity 自身が隠しフォルダに使うのと同じ規約）。そのため `After~/` は project 内に存在していても、今日の build には影響しません。Unity 6.8 で C# 14 対応が来たら、`After~/` を `After/` に rename するだけで、そのまま有効になります。
- **`Lab/`** —— 独立した `asmdef` とローカルの `csc.rsp` を持つ実験ゾーン。`unity-lab-setup.md` に書かれた裏技を使って、Unity 6.7 の公式サポート範囲を超えて試すための場所です。

## 7. Runner reference

**EN:** Run from the repo root — `--project` is resolved relative to your current directory, so the command below only works as written from there:

```
dotnet run --project tools/runner [--root <path>] [--no-cache] [--verify-broken]
```

- `--root <path>` — override repo-root auto-detection. By default the runner walks up from the current directory until it finds an `exercises/` folder.
- `--no-cache` — ignore the pass cache and re-check every exercise from scratch.
- `--verify-broken` — quality-gate mode: checks that every exercise is broken as committed and fully annotated (`EXPECTED OUTPUT`, `HINT EN`, `HINT JA` all present). Doesn't stop at the first problem — it reports every violation found. This is the mode a CI gate would run.

**Cache file:** `.sharplings-cache.json`, at the repo root, gitignored. Keyed by each exercise's SHA-256 content hash, so editing a previously-passed exercise invalidates its cache entry automatically.

Each exercise runs under a 10-second timeout (catches infinite loops).

**Exit codes:**

- `0` — normal mode: all exercises passed. `--verify-broken`: zero violations found.
- `1` — normal mode: stopped at the first failing exercise. `--verify-broken`: one or more violations found.
- `2` — usage error (unrecognized flag, or `--root` given without a value).

**JA:** リポジトリのルートから実行してください —— `--project` は現在のディレクトリからの相対パスとして解決されるため、下のコマンドはそこから実行した場合にのみそのまま動きます:

```
dotnet run --project tools/runner [--root <path>] [--no-cache] [--verify-broken]
```

- `--root <path>` —— repo root の自動検出を上書きします。既定では、runner は現在のディレクトリから `exercises/` フォルダが見つかるまで親へ遡ります。
- `--no-cache` —— 合格 cache を無視し、すべての exercise を最初からチェックし直します。
- `--verify-broken` —— 品質ゲートモード: すべての exercise が commit された時点で壊れており、必要な注釈（`EXPECTED OUTPUT`、`HINT EN`、`HINT JA` すべて）が揃っているかを検証します。最初の問題で止まらず、見つかった違反をすべて報告します。CI が実行するのはこのモードです。

**Cache file:** `.sharplings-cache.json`。repo root に置かれ、gitignore 対象です。各 exercise の SHA-256 content hash を key にしているため、合格済み exercise を編集すると、その cache entry は自動的に無効になります。

各 exercise は 10 秒の timeout の下で実行されます（無限ループを検知するためです）。

**Exit code:**

- `0` —— 通常モード: 全 exercise が合格。`--verify-broken`: 違反 0 件。
- `1` —— 通常モード: 最初に失敗した exercise で停止。`--verify-broken`: 1 件以上の違反あり。
- `2` —— usage エラー（未知の flag、または値なしの `--root`）。
