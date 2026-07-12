# sharplings

[English](./README.md) | [日本語](./README_JA.md)

A ziglings-style course for modern C# (10-14), built for Unity engineers stuck at C# 9.

## 1. What this is

sharplings is a ziglings-style exercise course for the C# language features introduced in C# 10 through C# 14. It's built for Unity engineers whose day job keeps them on C# 9 — Unity's current default `LangVersion`, paired with a .NET Standard 2.1 API surface. Each exercise is one broken `.cs` file: it either fails to compile or compiles and produces the wrong output. You fix it, a runner checks it, you move to the next one. There are 47 exercises across 9 chapters, all gate-verified — every exercise is confirmed to fail as committed, and every exercise carries an EN + JA explanation, an EN + JA hint, and an `EXPECTED OUTPUT` block the runner diffs your program's stdout against.

This project is inspired by [ziglings](https://ziglings.org/) — the same "fix one broken file, run a checker, repeat" workflow — but it is an independent project, not affiliated with or endorsed by the ziglings project.

## 2. Why .NET 10 and not Unity (yet)

Short version: Unity doesn't run C# 14 yet, and won't for a while.

- Unity 6.7 alpha (6000.7.0a2) ships an experimental CoreCLR **Desktop Player**, but the scripting language stays at **C# 9** and the API surface stays **.NET Standard 2.1**. CoreCLR here is a new player runtime backend, not a new compiler.
- **C# 14** and the **.NET 10** toolchain arrive with **Unity 6.8** (where Mono is removed).
- Sources: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299), [Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279).

So the fastest way to actually learn C# 10-14 today is a plain .NET 10 console environment — no editor recompiles, no domain reloads, no waiting on a Unity beta. `exercises/` in this repo is exactly that: fast, disposable, file-based .NET 10 apps. The `unity/` side (section 6) is where the same features land inside a real Unity project — officially once 6.8 ships, though the Lab's `csc.rsp` trick already unlocks C# ≤ 12 syntax there today.

## 3. Setup

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

## 4. How to work

From the repo root:

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

## 5. Course map

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

## 6. Unity side

The Unity project lives at `unity/` (Unity 6000.7.0a2, Universal template). Two docs are the Unity-side entry points:

- `docs/unity-lab-setup.md` — setup steps for the unofficial-trick lab that lets modern C# syntax compile inside Unity today.
- `docs/feature-matrix.md` — a living table of which language features actually run where (Mono editor / CoreCLR player / future 6.8), updated as Lab experiments produce results.

Two zones are live under `Assets/`:

- **`Contrasts/<topic>/`** — official, no-tricks-required Before/After pairs; six studies today. `Before/` holds the C# 9 Unity idiom (compiles today, on 6.7). `After~/` holds the modern-C# rewrite. Note the trailing `~`: folders and files ending in `~` are invisible to Unity's compilation pipeline (the same convention Unity itself uses for hidden folders), so `After~/` sits in the project without touching today's build. When Unity 6.8 ships C# 14 support, you rename `After~/` to `After/` and it goes live as-is.
- **`Lab/`** — an experimental zone, isolated in its own `asmdef` with a local `csc.rsp`, for pushing past what Unity 6.7 officially supports, using the tricks documented in `unity-lab-setup.md`.

Stage 0 (probing the bundled Roslyn compiler), Stage 1 (proving Unity's build pipeline honors a per-assembly `csc.rsp` language-version override), and Stage 2 (swapping the bundled Roslyn for the .NET 10 SDK's Roslyn 5.6.0) all ran on 2026-07-12. Headline: C# 11/12 syntax compiles in the Lab with `csc.rsp` alone, and with the Stage 2 swap active, C# 14 extension members compile inside Unity today. The swap is machine state, not something this repo carries — an editor update silently reverts it — and official, swap-free C# 14 still arrives with Unity 6.8. See `docs/feature-matrix.md` ("Stage 1 results", "Stage 2 results") and `docs/unity-lab-setup.md` for the full evidence.

## 7. Runner reference

Run from the repo root — `--project` is resolved relative to your current directory, so the command below only works as written from there:

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
