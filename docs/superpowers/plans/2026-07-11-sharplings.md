# sharplings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a ziglings-style learning repo where a C#-9-era Unity engineer learns C# 10–14 through fix-the-broken-file exercises run by a custom runner, plus a Unity 6.7 project with Before/After contrasts and an experimental modern-C# lab.

**Architecture:** A .NET 10 console runner executes each exercise as a standalone file-based app (`dotnet run <file>`), classifies the result (compile error / runtime error / output mismatch / timeout / pass), stops at the first failure, and shows hints. Exercises carry their own metadata (title, bilingual hints, expected output) in comments. The Unity side is content-only: paired Before (C# 9) / After~ (excluded from compilation until Unity 6.8) scripts plus a Lab isolated behind its own asmdef + csc.rsp.

**Tech Stack:** .NET 10 SDK (10.0.301 via mise), C# 14, xUnit, Unity 6000.7.0a2.

**Spec:** `docs/superpowers/specs/2026-07-11-sharplings-design.md` (approved 2026-07-11).

## Global Constraints

- .NET SDK is pinned to `10.0.301` via `global.json` (`rollForward: latestFeature`) and `mise.toml`.
- All learner-facing text (exercise explanations, hints, READMEs, docs) is written in BOTH English and Japanese. Identifiers and infrastructure code comments are English only.
- In Japanese text, insert a half-width space between full-width and half-width characters. No emojis anywhere (✓ / ✗ box-drawing marks are allowed).
- Tests never use mocks or stubs. Runner tests invoke the real `dotnet` CLI against fixture files.
- Exercise solutions are NEVER committed. Solved versions live only in the session scratchpad while authoring.
- Commits follow hashiiiii-git: one line, `<type>: <subject>`, English imperative, lowercase first word, ≤ 50 chars, no body.
- Exercise file contract (parsed by the runner, defined in Task 2): first `// [` line is the title; `// HINT EN:` and `// HINT JA:` blocks; `// EXPECTED OUTPUT:` block is the last comment block in the file.
- The runner must work when invoked from any directory inside the repo (`--root` overrides discovery).
- Measured facts the implementation relies on (verified 2026-07-11 with 10.0.301): cached `dotnet run file.cs` ≈ 0.17 s; compile errors print `error CSxxxx` lines to stdout and `The build failed.` to stderr with exit 1; unhandled exceptions print to stderr with exit 134. Child processes must run with `DOTNET_CLI_UI_LANGUAGE=en` so these markers are locale-stable.

---

## File Structure

```
sharplings/
├── .gitignore
├── global.json
├── mise.toml
├── README.md                                  # Task 17 (stub in Task 1)
├── exercises/
│   ├── 00_intro/001..003_*.cs                 # Task 8
│   ├── 01_records/011..016_*.cs               # Task 9
│   ├── 02_patterns/021..026_*.cs              # Task 10
│   ├── 03_collections/031..036_*.cs           # Task 11
│   ├── 04_strings/041..045_*.cs               # Task 12
│   ├── 05_types/051..057_*.cs                 # Task 13
│   ├── 06_generics/061..064_*.cs              # Task 14
│   ├── 07_performance/071..076_*.cs           # Task 15
│   └── 08_extensions/081..084_*.cs            # Task 16
├── tools/
│   ├── runner/
│   │   ├── Runner.csproj
│   │   ├── Program.cs                         # CLI entry + main loop
│   │   ├── Exercise.cs                        # metadata parsing
│   │   ├── ExerciseLocator.cs                 # root discovery + ordering
│   │   ├── DotnetRunner.cs                    # process exec + verdict
│   │   ├── ProgressCache.cs                   # SHA-256 pass cache
│   │   ├── Reporter.cs                        # console output
│   │   └── VerifyBroken.cs                    # quality gate mode
│   └── runner.tests/
│       ├── Runner.Tests.csproj
│       ├── Fixtures.cs
│       ├── ExerciseParseTests.cs
│       ├── ExerciseLocatorTests.cs
│       ├── OutputComparisonTests.cs
│       ├── DotnetRunnerTests.cs
│       ├── ProgressCacheTests.cs
│       └── fixtures/                          # real exercise files (excluded from compile)
├── unity/                                     # created by USER via Unity Hub (Task 19)
│   └── Assets/
│       ├── Contrasts/<topic>/{Before,After~,README.md}   # Task 20
│       └── Lab/                               # Task 21
└── docs/
    ├── feature-matrix.md                      # Task 18
    ├── unity-lab-setup.md                     # Task 18
    └── superpowers/{specs,plans}/
```

Tasks 1–18 have no dependency on Unity. Tasks 19–21 are sequential and gated on the user.

---

### Task 1: Repo scaffolding

**Files:**
- Create: `global.json`, `mise.toml`, `.gitignore`, `README.md` (stub), `exercises/.gitkeep`

**Interfaces:**
- Produces: repo root layout every later task assumes; `dotnet` resolving to 10.0.301 inside the repo.

- [ ] **Step 1: Write config files**

`global.json`:

```json
{
  "sdk": {
    "version": "10.0.301",
    "rollForward": "latestFeature"
  }
}
```

`mise.toml`:

```toml
[tools]
dotnet = "10.0.301"
```

`.gitignore`:

```gitignore
# .NET
bin/
obj/
.sharplings-cache.json
.DS_Store

# Unity (project lives under unity/)
unity/Library/
unity/Temp/
unity/Logs/
unity/UserSettings/
unity/obj/
unity/*.sln
unity/*.csproj
```

`README.md` (stub; replaced in Task 17):

```markdown
# sharplings

A ziglings-style course for learning modern C# (10-14) as a Unity engineer.
Unity エンジニアが最新 C# (10〜14) を ziglings 形式で学ぶための repo。

Work in progress. See docs/superpowers/specs/2026-07-11-sharplings-design.md.
```

- [ ] **Step 2: Verify SDK resolution**

Run from repo root: `dotnet --version`
Expected: `10.0.301`

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "chore: scaffold repo config and layout"
```

---

### Task 2: Runner project + exercise metadata parsing

**Files:**
- Create: `tools/runner/Runner.csproj`, `tools/runner/Exercise.cs`
- Create: `tools/runner.tests/Runner.Tests.csproj`, `tools/runner.tests/Fixtures.cs`, `tools/runner.tests/ExerciseParseTests.cs`
- Create: `tools/runner.tests/fixtures/parsing/exercises/00_parse/001_sample.cs`

**Interfaces:**
- Produces: `Sharplings.Runner.Exercise` record — `Exercise.Parse(string root, string absolutePath)` returning `Exercise(string RelativePath, string AbsolutePath, string Title, string ExpectedOutput, IReadOnlyList<string> Hints)`. `ExpectedOutput` is `\n`-joined lines without comment prefixes. Each hint is the full text starting with `HINT EN:` or `HINT JA:`.

- [ ] **Step 1: Create the projects**

```bash
mkdir -p tools/runner tools/runner.tests
dotnet new console -o tools/runner -n Runner
dotnet new xunit -o tools/runner.tests -n Runner.Tests
dotnet add tools/runner.tests reference tools/runner
```

Overwrite `tools/runner/Runner.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Sharplings.Runner</RootNamespace>
    <AssemblyName>runner</AssemblyName>
  </PropertyGroup>
</Project>
```

Edit `tools/runner.tests/Runner.Tests.csproj` — add inside the existing `<Project>`:

```xml
  <ItemGroup>
    <Compile Remove="fixtures/**/*.cs" />
    <Content Include="fixtures/**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```

(The `Compile Remove` is essential: fixture exercises are intentionally broken C# and must not be compiled into the test assembly.)

- [ ] **Step 2: Write the parsing fixture**

`tools/runner.tests/fixtures/parsing/exercises/00_parse/001_sample.cs`:

```csharp
// [C# 12] Sample exercise
//
// EN: Explanation in English.
// JA: 日本語の解説。

Console.WriteLine("stub");

// HINT EN: First hint line.
//          Continuation of the English hint.
// HINT JA: 日本語のヒント。
//
// EXPECTED OUTPUT:
// line one
// line two
```

- [ ] **Step 3: Write the failing test**

`tools/runner.tests/Fixtures.cs`:

```csharp
namespace Sharplings.Runner.Tests;

public static class Fixtures
{
    public static string Path(string relative) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "fixtures",
            relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
}
```

`tools/runner.tests/ExerciseParseTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ExerciseParseTests
{
    // The parser must extract the title line, the bilingual hint blocks
    // (with continuation lines), and the expected-output block.
    [Fact]
    public void Parses_title_expected_output_and_bilingual_hints()
    {
        string root = Fixtures.Path("parsing");
        var exercise = Exercise.Parse(root,
            Fixtures.Path("parsing/exercises/00_parse/001_sample.cs"));

        Assert.Equal("[C# 12] Sample exercise", exercise.Title);
        Assert.Equal("line one\nline two", exercise.ExpectedOutput);
        Assert.Equal(2, exercise.Hints.Count);
        Assert.StartsWith("HINT EN: First hint line.", exercise.Hints[0]);
        Assert.Contains("Continuation of the English hint.", exercise.Hints[0]);
        Assert.StartsWith("HINT JA:", exercise.Hints[1]);
    }

    // RelativePath is relative to the repo root so cache keys and display
    // stay stable regardless of where the runner is invoked from.
    [Fact]
    public void Relative_path_is_rooted_at_repo_root()
    {
        string root = Fixtures.Path("parsing");
        var exercise = Exercise.Parse(root,
            Fixtures.Path("parsing/exercises/00_parse/001_sample.cs"));

        Assert.Equal(
            Path.Combine("exercises", "00_parse", "001_sample.cs"),
            exercise.RelativePath);
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run: `dotnet test tools/runner.tests`
Expected: FAIL — `Exercise` does not exist (compile error `CS0246`).

- [ ] **Step 5: Implement Exercise.Parse**

`tools/runner/Exercise.cs`:

```csharp
namespace Sharplings.Runner;

/// <summary>One exercise file plus the metadata parsed from its comments.</summary>
public sealed record Exercise(
    string RelativePath,
    string AbsolutePath,
    string Title,
    string ExpectedOutput,
    IReadOnlyList<string> Hints)
{
    public static Exercise Parse(string root, string absolutePath)
    {
        string[] lines = File.ReadAllLines(absolutePath);

        string title = lines.FirstOrDefault(static l => l.StartsWith("// ["))
            is string titleLine
            ? titleLine[3..].Trim()
            : Path.GetFileName(absolutePath);

        List<string> expected = [];
        List<string> hints = [];
        int i = 0;
        while (i < lines.Length)
        {
            string line = lines[i].TrimEnd();
            if (line == "// EXPECTED OUTPUT:")
            {
                i++;
                while (i < lines.Length && lines[i].StartsWith("//"))
                    expected.Add(StripComment(lines[i++]));
            }
            else if (line.StartsWith("// HINT"))
            {
                List<string> parts = [StripComment(line)];
                i++;
                while (i < lines.Length
                       && lines[i].StartsWith("//")
                       && !lines[i].StartsWith("// HINT")
                       && lines[i].TrimEnd() != "// EXPECTED OUTPUT:")
                    parts.Add(StripComment(lines[i++]));
                hints.Add(string.Join('\n', parts).TrimEnd());
            }
            else
            {
                i++;
            }
        }

        return new Exercise(
            Path.GetRelativePath(root, absolutePath),
            absolutePath,
            title,
            string.Join('\n', expected),
            hints);
    }

    private static string StripComment(string line)
    {
        string s = line.TrimEnd();
        return s.StartsWith("// ") ? s[3..] : s.StartsWith("//") ? s[2..] : s;
    }
}
```

Also clear `tools/runner/Program.cs` down to a placeholder so the project still builds (replaced in Task 6):

```csharp
Console.WriteLine("sharplings runner: not implemented yet");
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test tools/runner.tests`
Expected: PASS (2 tests).

- [ ] **Step 7: Commit**

```bash
git add tools/
git commit -m "feat: add runner project and exercise parsing"
```

---

### Task 3: Exercise discovery and ordering

**Files:**
- Create: `tools/runner/ExerciseLocator.cs`
- Create: `tools/runner.tests/ExerciseLocatorTests.cs`
- Create: `tools/runner.tests/fixtures/locating/exercises/00_alpha/001_a.cs`, `.../00_alpha/002_b.cs`, `.../01_beta/011_c.cs` (each a copy of the Task 2 sample fixture body — content is irrelevant to ordering)

**Interfaces:**
- Consumes: `Exercise.Parse` (Task 2).
- Produces: `ExerciseLocator.FindRoot(string startDirectory) -> string` (walks up to the first directory containing `exercises/`, throws `InvalidOperationException` if none) and `ExerciseLocator.Discover(string root) -> IReadOnlyList<Exercise>` (all `*.cs` under `exercises/`, ordinal path order).

- [ ] **Step 1: Write the failing tests**

`tools/runner.tests/ExerciseLocatorTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ExerciseLocatorTests
{
    // Zero-padded numeric prefixes make ordinal string order the intended
    // course order; chapters sort before later chapters, files within them.
    [Fact]
    public void Discovers_exercises_in_ordinal_path_order()
    {
        var exercises = ExerciseLocator.Discover(Fixtures.Path("locating"));

        Assert.Equal(
            [
                Path.Combine("exercises", "00_alpha", "001_a.cs"),
                Path.Combine("exercises", "00_alpha", "002_b.cs"),
                Path.Combine("exercises", "01_beta", "011_c.cs"),
            ],
            exercises.Select(e => e.RelativePath).ToArray());
    }

    // The runner is started from arbitrary directories inside the repo;
    // the root is wherever the exercises/ directory lives.
    [Fact]
    public void Finds_root_from_nested_directory()
    {
        string nested = Fixtures.Path("locating/exercises/01_beta");
        Assert.Equal(Fixtures.Path("locating"), ExerciseLocator.FindRoot(nested));
    }

    [Fact]
    public void Throws_when_no_exercises_directory_above()
    {
        Assert.Throws<InvalidOperationException>(
            () => ExerciseLocator.FindRoot(Path.GetTempPath()));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tools/runner.tests`
Expected: FAIL — `ExerciseLocator` does not exist.

- [ ] **Step 3: Implement ExerciseLocator**

`tools/runner/ExerciseLocator.cs`:

```csharp
namespace Sharplings.Runner;

/// <summary>Finds the repo root and enumerates exercises in course order.</summary>
public static class ExerciseLocator
{
    public static string FindRoot(string startDirectory)
    {
        for (DirectoryInfo? dir = new(startDirectory); dir is not null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "exercises")))
                return dir.FullName;
        throw new InvalidOperationException(
            $"No 'exercises' directory found above '{startDirectory}'. " +
            "Run from inside the sharplings repo or pass --root <path>.");
    }

    public static IReadOnlyList<Exercise> Discover(string root) => [..
        Directory.EnumerateFiles(
                Path.Combine(root, "exercises"), "*.cs", SearchOption.AllDirectories)
            .OrderBy(static p => p, StringComparer.Ordinal)
            .Select(p => Exercise.Parse(root, p))];
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tools/runner.tests`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add tools/
git commit -m "feat: add exercise discovery and ordering"
```

---

### Task 4: Exercise execution and verdict classification

**Files:**
- Create: `tools/runner/DotnetRunner.cs`
- Create: `tools/runner.tests/OutputComparisonTests.cs`, `tools/runner.tests/DotnetRunnerTests.cs`
- Create fixtures under `tools/runner.tests/fixtures/running/exercises/00_run/`: `001_ok.cs`, `002_wrong_output.cs`, `003_compile_error.cs`, `004_crash.cs`, `005_loop.cs`

**Interfaces:**
- Consumes: `Exercise` (Task 2).
- Produces: `enum Verdict { Pass, CompileError, RuntimeError, OutputMismatch, Timeout }`; `sealed record RunResult(Verdict Verdict, string Stdout, string Stderr, int ExitCode)`; `sealed class DotnetRunner(TimeSpan timeout)` with `Task<RunResult> RunAsync(Exercise exercise, CancellationToken ct = default)` and `static bool OutputMatches(string actual, string expected)`.

- [ ] **Step 1: Write the run fixtures**

Each fixture is a real file-based app. `001_ok.cs`:

```csharp
// [C# 9] Fixture: passes
Console.WriteLine("ok");
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// ok
```

`002_wrong_output.cs`:

```csharp
// [C# 9] Fixture: output mismatch
Console.WriteLine("actual");
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// expected
```

`003_compile_error.cs`:

```csharp
// [C# 9] Fixture: does not compile
int x = "not an int";
Console.WriteLine(x);
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// never
```

`004_crash.cs`:

```csharp
// [C# 9] Fixture: throws at runtime
Console.WriteLine("before");
throw new InvalidOperationException("boom");
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// before
```

`005_loop.cs`:

```csharp
// [C# 9] Fixture: never terminates
while (true) { }
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// never
```

- [ ] **Step 2: Write the failing tests**

`tools/runner.tests/OutputComparisonTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class OutputComparisonTests
{
    // Comparison must be forgiving about line endings, trailing spaces and
    // trailing blank lines, and strict about everything else.
    [Theory]
    [InlineData("a\nb", "a\nb", true)]
    [InlineData("a\r\nb\r\n", "a\nb", true)]
    [InlineData("a  \nb", "a\nb", true)]
    [InlineData("a\nb\n\n", "a\nb", true)]
    [InlineData("a\nc", "a\nb", false)]
    [InlineData("A\nb", "a\nb", false)]
    public void Compares_normalized_output(string actual, string expected, bool match) =>
        Assert.Equal(match, DotnetRunner.OutputMatches(actual, expected));
}
```

`tools/runner.tests/DotnetRunnerTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

// These tests execute the real dotnet CLI (no mocks). First run of each
// fixture compiles it (~5 s); afterwards the file-based-app cache makes
// re-runs fast.
public class DotnetRunnerTests
{
    private static async Task<RunResult> Run(string fixture, double timeoutSeconds = 120)
    {
        string root = Fixtures.Path("running");
        var exercise = Exercise.Parse(root,
            Fixtures.Path($"running/exercises/00_run/{fixture}"));
        return await new DotnetRunner(TimeSpan.FromSeconds(timeoutSeconds))
            .RunAsync(exercise);
    }

    [Fact]
    public async Task Matching_output_is_pass()
    {
        var result = await Run("001_ok.cs");
        Assert.Equal(Verdict.Pass, result.Verdict);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Nonmatching_output_is_mismatch()
    {
        var result = await Run("002_wrong_output.cs");
        Assert.Equal(Verdict.OutputMismatch, result.Verdict);
    }

    [Fact]
    public async Task Build_failure_is_compile_error()
    {
        var result = await Run("003_compile_error.cs");
        Assert.Equal(Verdict.CompileError, result.Verdict);
        // The CSxxxx diagnostic lands in stdout (measured behavior).
        Assert.Contains("error CS", result.Stdout);
    }

    [Fact]
    public async Task Unhandled_exception_is_runtime_error()
    {
        var result = await Run("004_crash.cs");
        Assert.Equal(Verdict.RuntimeError, result.Verdict);
        Assert.Contains("InvalidOperationException", result.Stderr);
    }

    [Fact]
    public async Task Infinite_loop_times_out()
    {
        // Generous enough to cover first-time compilation of the fixture,
        // short enough to keep the suite tolerable.
        var result = await Run("005_loop.cs", timeoutSeconds: 30);
        Assert.Equal(Verdict.Timeout, result.Verdict);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `dotnet test tools/runner.tests`
Expected: FAIL — `DotnetRunner`, `Verdict`, `RunResult` do not exist.

- [ ] **Step 4: Implement DotnetRunner**

`tools/runner/DotnetRunner.cs`:

```csharp
using System.Diagnostics;

namespace Sharplings.Runner;

public enum Verdict { Pass, CompileError, RuntimeError, OutputMismatch, Timeout }

public sealed record RunResult(Verdict Verdict, string Stdout, string Stderr, int ExitCode);

/// <summary>Runs one exercise as a .NET 10 file-based app and classifies the outcome.</summary>
public sealed class DotnetRunner(TimeSpan timeout)
{
    public async Task<RunResult> RunAsync(Exercise exercise, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "run", exercise.AbsolutePath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(exercise.AbsolutePath)!,
        };
        // Keep diagnostics locale-stable so verdict classification works
        // regardless of the user's OS language.
        psi.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
        psi.Environment["DOTNET_NOLOGO"] = "1";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            return new RunResult(Verdict.Timeout, await stdoutTask, await stderrTask, -1);
        }

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        Verdict verdict = process.ExitCode switch
        {
            0 => OutputMatches(stdout, exercise.ExpectedOutput)
                ? Verdict.Pass
                : Verdict.OutputMismatch,
            _ => stderr.Contains("The build failed")
                ? Verdict.CompileError
                : Verdict.RuntimeError,
        };
        return new RunResult(verdict, stdout, stderr, process.ExitCode);
    }

    public static bool OutputMatches(string actual, string expected)
    {
        static string Normalize(string s) => string.Join('\n',
            s.ReplaceLineEndings("\n").Split('\n').Select(static l => l.TrimEnd()))
            .TrimEnd('\n');
        return Normalize(actual) == Normalize(expected);
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test tools/runner.tests`
Expected: PASS (16 tests). The suite takes ~30–60 s on first run (fixture compilation), including ~30 s for the timeout test.

- [ ] **Step 6: Commit**

```bash
git add tools/
git commit -m "feat: add exercise execution and verdicts"
```

---

### Task 5: Progress cache

**Files:**
- Create: `tools/runner/ProgressCache.cs`
- Create: `tools/runner.tests/ProgressCacheTests.cs`

**Interfaces:**
- Consumes: `Exercise` (Task 2).
- Produces: `sealed class ProgressCache(string cachePath)` with `bool IsPassed(Exercise exercise)` and `void MarkPassed(Exercise exercise)`. Cache file is JSON `{ "<RelativePath>": "<SHA-256 hex of file bytes>" }`; editing an exercise invalidates its entry.

- [ ] **Step 1: Write the failing tests**

`tools/runner.tests/ProgressCacheTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ProgressCacheTests
{
    // Build a tiny throwaway exercise in a temp dir; the cache only reads
    // file bytes, so the content does not need to compile.
    private static (Exercise Exercise, string Root) MakeExercise(string content)
    {
        string root = Directory.CreateTempSubdirectory("sharplings-cache-").FullName;
        string dir = Path.Combine(root, "exercises", "00_t");
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, "001_t.cs");
        File.WriteAllText(file, content);
        return (Exercise.Parse(root, file), root);
    }

    [Fact]
    public void Unknown_exercise_is_not_passed()
    {
        var (exercise, root) = MakeExercise("// v1");
        var cache = new ProgressCache(Path.Combine(root, "cache.json"));
        Assert.False(cache.IsPassed(exercise));
    }

    [Fact]
    public void Marked_exercise_is_passed_and_survives_reload()
    {
        var (exercise, root) = MakeExercise("// v1");
        string cachePath = Path.Combine(root, "cache.json");
        new ProgressCache(cachePath).MarkPassed(exercise);

        // A fresh instance re-reads the JSON file from disk.
        Assert.True(new ProgressCache(cachePath).IsPassed(exercise));
    }

    [Fact]
    public void Editing_the_file_invalidates_the_pass()
    {
        var (exercise, root) = MakeExercise("// v1");
        string cachePath = Path.Combine(root, "cache.json");
        var cache = new ProgressCache(cachePath);
        cache.MarkPassed(exercise);

        File.WriteAllText(exercise.AbsolutePath, "// v2");
        Assert.False(cache.IsPassed(exercise));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tools/runner.tests --filter ProgressCacheTests`
Expected: FAIL — `ProgressCache` does not exist.

- [ ] **Step 3: Implement ProgressCache**

`tools/runner/ProgressCache.cs`:

```csharp
using System.Security.Cryptography;
using System.Text.Json;

namespace Sharplings.Runner;

/// <summary>Remembers which exercises passed, keyed by content hash so any edit re-runs the file.</summary>
public sealed class ProgressCache(string cachePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly Dictionary<string, string> _passed = Load(cachePath);

    public bool IsPassed(Exercise exercise) =>
        _passed.TryGetValue(exercise.RelativePath, out string? hash)
        && hash == HashOf(exercise);

    public void MarkPassed(Exercise exercise)
    {
        _passed[exercise.RelativePath] = HashOf(exercise);
        File.WriteAllText(cachePath, JsonSerializer.Serialize(_passed, JsonOptions));
    }

    private static Dictionary<string, string> Load(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? []
            : [];

    private static string HashOf(Exercise exercise) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(exercise.AbsolutePath)));
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tools/runner.tests --filter ProgressCacheTests`
Expected: PASS (3 tests).

- [ ] **Step 5: Commit**

```bash
git add tools/
git commit -m "feat: add content-hash progress cache"
```

---

### Task 6: Reporter and CLI main loop

**Files:**
- Create: `tools/runner/Reporter.cs`
- Modify: `tools/runner/Program.cs` (replace placeholder)

**Interfaces:**
- Consumes: `ExerciseLocator`, `DotnetRunner`, `ProgressCache`, `Exercise`, `RunResult`, `Verdict`.
- Produces: CLI `dotnet run --project tools/runner [-- --root <path>] [--no-cache]`. Exit 0 when all pass, 1 at first failure, 2 on bad usage. `static class Reporter` with `PrintPass(Exercise, bool cached)`, `PrintFailure(Exercise, RunResult, int passed, int total)`, `PrintAllPassed(int total)`.

- [ ] **Step 1: Implement Reporter**

`tools/runner/Reporter.cs`:

```csharp
namespace Sharplings.Runner;

/// <summary>Console presentation. Kept free of logic so the loop stays testable by exit code.</summary>
public static class Reporter
{
    public static void PrintPass(Exercise exercise, bool cached)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  ✓ ");
        Console.ResetColor();
        Console.WriteLine(cached ? $"{exercise.RelativePath} (cached)" : exercise.RelativePath);
    }

    public static void PrintFailure(Exercise exercise, RunResult result, int passed, int total)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  ✗ ");
        Console.ResetColor();
        Console.WriteLine(exercise.RelativePath);
        Console.WriteLine();
        Console.WriteLine(result.Verdict switch
        {
            Verdict.CompileError => "The exercise does not compile:",
            Verdict.RuntimeError => "The exercise crashed while running:",
            Verdict.OutputMismatch => "The exercise runs, but its output does not match:",
            Verdict.Timeout => "The exercise did not finish in time (infinite loop?):",
            _ => result.Verdict.ToString(),
        });
        Console.WriteLine();

        if (result.Verdict is Verdict.OutputMismatch)
        {
            Console.WriteLine("--- expected ---------------------------------");
            Console.WriteLine(exercise.ExpectedOutput);
            Console.WriteLine("--- actual -----------------------------------");
            Console.WriteLine(result.Stdout.TrimEnd());
            Console.WriteLine("----------------------------------------------");
        }
        else
        {
            if (result.Stdout.Trim().Length > 0) Console.WriteLine(result.Stdout.TrimEnd());
            if (result.Stderr.Trim().Length > 0) Console.WriteLine(result.Stderr.TrimEnd());
        }

        Console.WriteLine();
        foreach (string hint in exercise.Hints)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(hint);
            Console.ResetColor();
        }
        Console.WriteLine();
        Console.WriteLine($"Progress: {passed}/{total}");
        Console.WriteLine($"Edit {exercise.RelativePath} and run the runner again.");
    }

    public static void PrintAllPassed(int total)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"All {total} exercises pass. Modern C# is yours.");
        Console.ResetColor();
    }
}
```

- [ ] **Step 2: Implement the main loop**

`tools/runner/Program.cs`:

```csharp
using Sharplings.Runner;

bool verifyBroken = false;
bool noCache = false;
string? rootOverride = null;
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--verify-broken": verifyBroken = true; break;
        case "--no-cache": noCache = true; break;
        case "--root" when i + 1 < args.Length: rootOverride = args[++i]; break;
        default:
            Console.Error.WriteLine(
                "usage: runner [--root <path>] [--no-cache] [--verify-broken]");
            return 2;
    }
}

string root = rootOverride is null
    ? ExerciseLocator.FindRoot(Environment.CurrentDirectory)
    : Path.GetFullPath(rootOverride);
IReadOnlyList<Exercise> exercises = ExerciseLocator.Discover(root);
var runner = new DotnetRunner(TimeSpan.FromSeconds(10));

if (verifyBroken)
    return await VerifyBroken.RunAsync(exercises, runner);

var cache = new ProgressCache(Path.Combine(root, ".sharplings-cache.json"));
int passed = 0;
foreach (Exercise exercise in exercises)
{
    if (!noCache && cache.IsPassed(exercise))
    {
        Reporter.PrintPass(exercise, cached: true);
        passed++;
        continue;
    }

    RunResult result = await runner.RunAsync(exercise);
    if (result.Verdict is Verdict.Pass)
    {
        cache.MarkPassed(exercise);
        Reporter.PrintPass(exercise, cached: false);
        passed++;
        continue;
    }

    Reporter.PrintFailure(exercise, result, passed, exercises.Count);
    return 1;
}

Reporter.PrintAllPassed(exercises.Count);
return 0;
```

Note: `VerifyBroken` does not exist until Task 7. To keep this task green, add a temporary stub `tools/runner/VerifyBroken.cs` now (fully replaced in Task 7):

```csharp
namespace Sharplings.Runner;

public static class VerifyBroken
{
    public static Task<int> RunAsync(IReadOnlyList<Exercise> exercises, DotnetRunner runner)
        => Task.FromResult(0);
}
```

- [ ] **Step 3: Verify end-to-end against the run fixtures**

The fixtures from Task 4 form a valid fixture repo (root = `fixtures/running` inside the tests project source tree, which also works uncompiled):

```bash
dotnet run --project tools/runner -- --root tools/runner.tests/fixtures/running --no-cache
echo "exit=$?"
```

Expected: `✓ .../001_ok.cs`, then `✗ .../002_wrong_output.cs` with the expected/actual diff, `Progress: 1/5`, `exit=1`.

- [ ] **Step 4: Verify cache behavior**

```bash
dotnet run --project tools/runner -- --root tools/runner.tests/fixtures/running
dotnet run --project tools/runner -- --root tools/runner.tests/fixtures/running
```

Expected: second invocation shows `✓ .../001_ok.cs (cached)` before failing at 002 again. Then delete the stray cache file so fixtures stay pristine:

```bash
rm tools/runner.tests/fixtures/running/.sharplings-cache.json
```

- [ ] **Step 5: Run the full test suite (regression)**

Run: `dotnet test tools/runner.tests`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add tools/
git commit -m "feat: add runner cli main loop and reporter"
```

---

### Task 7: verify-broken quality gate

**Files:**
- Modify: `tools/runner/VerifyBroken.cs` (replace stub)
- Create: `tools/runner.tests/VerifyBrokenTests.cs`

**Interfaces:**
- Consumes: `Exercise`, `DotnetRunner`, `Verdict`.
- Produces: `VerifyBroken.RunAsync(IReadOnlyList<Exercise>, DotnetRunner) -> Task<int>` — exit 0 iff every exercise (a) does NOT pass as committed, (b) has a non-empty `ExpectedOutput`, (c) has at least one `HINT EN` and one `HINT JA`. Violations print to stderr prefixed `VIOLATION:`.

- [ ] **Step 1: Write the failing tests**

`tools/runner.tests/VerifyBrokenTests.cs`:

```csharp
using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class VerifyBrokenTests
{
    private static readonly DotnetRunner Runner = new(TimeSpan.FromSeconds(120));

    private static Exercise Fixture(string name) =>
        Exercise.Parse(Fixtures.Path("running"),
            Fixtures.Path($"running/exercises/00_run/{name}"));

    // A broken exercise with full metadata is exactly what we want to ship.
    [Fact]
    public async Task Broken_exercise_with_metadata_is_ok()
    {
        int exit = await VerifyBroken.RunAsync([Fixture("002_wrong_output.cs")], Runner);
        Assert.Equal(0, exit);
    }

    // An exercise that already passes teaches nothing; the gate must fail.
    [Fact]
    public async Task Passing_exercise_is_a_violation()
    {
        int exit = await VerifyBroken.RunAsync([Fixture("001_ok.cs")], Runner);
        Assert.Equal(1, exit);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tools/runner.tests --filter VerifyBrokenTests`
Expected: FAIL — the stub returns 0 for both, so `Passing_exercise_is_a_violation` fails.

- [ ] **Step 3: Implement VerifyBroken**

Replace `tools/runner/VerifyBroken.cs`:

```csharp
namespace Sharplings.Runner;

/// <summary>Quality gate: every committed exercise must be broken and fully annotated.</summary>
public static class VerifyBroken
{
    public static async Task<int> RunAsync(
        IReadOnlyList<Exercise> exercises, DotnetRunner runner)
    {
        List<string> violations = [];
        foreach (Exercise exercise in exercises)
        {
            if (exercise.ExpectedOutput.Length == 0)
                violations.Add($"{exercise.RelativePath}: missing EXPECTED OUTPUT");
            if (!exercise.Hints.Any(static h => h.StartsWith("HINT EN")))
                violations.Add($"{exercise.RelativePath}: missing HINT EN");
            if (!exercise.Hints.Any(static h => h.StartsWith("HINT JA")))
                violations.Add($"{exercise.RelativePath}: missing HINT JA");

            RunResult result = await runner.RunAsync(exercise);
            if (result.Verdict is Verdict.Pass)
                violations.Add($"{exercise.RelativePath}: already passes (not broken)");
            Console.WriteLine($"checked {exercise.RelativePath}: {result.Verdict}");
        }

        foreach (string violation in violations)
            Console.Error.WriteLine($"VIOLATION: {violation}");
        Console.WriteLine($"{exercises.Count} exercise(s) checked, {violations.Count} violation(s)");
        return violations.Count == 0 ? 0 : 1;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tools/runner.tests`
Expected: PASS (full suite).

- [ ] **Step 5: Commit**

```bash
git add tools/
git commit -m "feat: add verify-broken quality gate"
```

---

## Chapter authoring procedure (applies to Tasks 8–16)

Every chapter task follows the same steps; the exercise tables below are the content spec. Format contract for every exercise file (established by the Task 8 exemplar):

1. Title line `// [C# NN] <Feature name>`.
2. `EN:` / `JA:` explanation block (what the feature is, what the C# 9 way was).
3. `Unity note:` block, `EN:`/`JA:` (how the idiom appears in Unity code today, and — where relevant — whether the feature needs the 6.8 runtime; link `docs/feature-matrix.md`).
4. `Docs:` line with a Microsoft Learn URL. Verify each URL with `curl -sI <url> | head -1` (expect `HTTP/2 200`) before committing.
5. The broken code. Break styles: `placeholder` (`???` the learner replaces — produces a compile error as committed), `compile` (idiomatic-but-illegal old-style code the learner must rewrite), `output` (compiles but prints the wrong thing until modernized).
6. `HINT EN:` / `HINT JA:` blocks.
7. `EXPECTED OUTPUT:` block, last in the file.

Steps per chapter:

- [ ] **Step A: Author solved versions in the scratchpad** — one solved `.cs` per exercise under `<scratchpad>/solutions/<chapter>/`. Run each with `dotnet run <file>`; the printed output becomes the `EXPECTED OUTPUT` block verbatim. A solution that does not run or produces unstable output (timestamps, hash ordering, locale-dependent formatting) must be redesigned before proceeding.
- [ ] **Step B: Write the broken exercise files** under `exercises/<chapter>/`, applying the break style from the table. The diff between solution and broken file must be exactly the teaching point — nothing else.
- [ ] **Step C: Run the quality gate** — `dotnet run --project tools/runner -- --verify-broken`. Expected: every committed exercise reports CompileError/OutputMismatch (never Pass), `0 violation(s)`, exit 0.
- [ ] **Step D: Sanity-check the course flow** — `dotnet run --project tools/runner -- --no-cache` must stop at the chapter's first exercise with the intended verdict and readable hints.
- [ ] **Step E: Commit** — `git add exercises/ && git commit -m "feat: add <chapter> exercises"`.

Solutions in the scratchpad are deleted when the chapter is committed.

---

### Task 8: Chapter 00_intro (3 exercises) — includes the format exemplar

**Files:**
- Create: `exercises/00_intro/001_hello.cs`, `002_expected_output.cs`, `003_implicit_usings.cs`

**Interfaces:**
- Produces: the exercise format exemplar every later chapter copies.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 001_hello.cs | file-based apps (.NET 10), top-level statements (C# 9 review) | placeholder | one file = one program, `dotnet run file.cs`; Unity never shows you an entry point |
| 002_expected_output.cs | runner mechanics | output | how OutputMismatch verdicts look; fix a wrong string literal |
| 003_implicit_usings.cs | implicit usings (C# 10 era SDK default) | compile | uses `StringBuilder` without `using System.Text;` — implicit usings cover System/Linq/IO/Tasks but not System.Text |

- [ ] **Step 1: Write 001_hello.cs exactly as follows (format exemplar)**

```csharp
// [.NET 10] Welcome to sharplings!
//
// EN: Each exercise is a single C# file that runs on its own thanks to
//     .NET 10 "file-based apps" (dotnet run 001_hello.cs). No .csproj
//     required. Fix the file until its output matches the EXPECTED
//     OUTPUT block at the bottom, then run the runner again.
// JA: 各 exercise は .NET 10 の「file-based apps」機能で単体実行できる
//     1 つの C# ファイルです（dotnet run 001_hello.cs）。.csproj は不要。
//     末尾の EXPECTED OUTPUT block と出力が一致するまで修正し、runner を
//     再実行してください。
//
// Unity note:
// EN: In Unity you never write an entry point — the engine calls into
//     your MonoBehaviours. Here the statement at the top of the file IS
//     the program: C# 9 top-level statements.
// JA: Unity ではエントリポイントを書きませんが、ここではファイル先頭の
//     文がそのままプログラムになります（C# 9 の top-level statements）。
//
// Docs: https://learn.microsoft.com/dotnet/csharp/fundamentals/program-structure/top-level-statements

Console.WriteLine(???);

// HINT EN: Replace ??? with a string literal. The exact text (including
//          punctuation) is in EXPECTED OUTPUT below.
// HINT JA: ??? を文字列リテラルに置き換えます。正確な文字列（記号を含む）
//          は下の EXPECTED OUTPUT にあります。
//
// EXPECTED OUTPUT:
// Hello, sharplings!
```

- [ ] **Step 2–5: Apply the chapter authoring procedure (Steps A–E)** for 002 and 003, then the gate, flow check, and commit `feat: add 00_intro exercises`.

---

### Task 9: Chapter 01_records (6 exercises)

**Files:** Create `exercises/01_records/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 011_record_review.cs | records + `with` (C# 9 review) | placeholder | warm-up on ground the learner half-knows; Unity devs rarely used records because Unity serialization ignores them |
| 012_record_struct.cs | `record struct` (C# 10) | placeholder | value-type records; equality/copy semantics printed; Unity note: struct-heavy gameplay data |
| 013_readonly_record_struct.cs | `readonly record struct` (C# 10) | compile | mutable-by-default surprise of record struct; make it readonly to fix an illegal mutation the exercise relies on |
| 014_deconstruction.cs | positional deconstruction (C# 9/10 review) | placeholder | `var (x, y) = point;` — pairs with Unity's Vector2 tuples habit |
| 015_primary_ctor_class.cs | primary constructors on classes (C# 12) | placeholder | kill constructor boilerplate; Unity note: plain C# service classes, not MonoBehaviours |
| 016_primary_ctor_capture.cs | primary ctor capture semantics (C# 12) | output | parameter captured once, shared by members — the classic double-capture gotcha printed and fixed |

- [ ] **Steps A–E per the chapter authoring procedure.** Commit: `feat: add 01_records exercises`.

---

### Task 10: Chapter 02_patterns (6 exercises)

**Files:** Create `exercises/02_patterns/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 021_property_patterns.cs | extended property patterns (C# 10) | placeholder | `{ Enemy.Hp: > 0 }` dotted access vs C# 9 nesting |
| 022_list_patterns.cs | list patterns (C# 11) | placeholder | `[1, 2, ..]` matching; Unity note: input combo buffers |
| 023_slice_patterns.cs | slice patterns (C# 11) | placeholder | `[var first, .. var rest]` |
| 024_switch_state_machine.cs | switch expressions + tuples (C# 9/10 review applied) | output | enum + if-chains rewritten as a state-transition table; the classic Unity FSM |
| 025_span_string_pattern.cs | `ReadOnlySpan<char>` matched against constant strings (C# 11) | placeholder | zero-alloc token dispatch; Unity note: parsing console commands without garbage |
| 026_damage_calc.cs | combined `and`/`or`/`not` + relational patterns (C# 9 review) | output | a damage calculation with a subtly wrong pattern to fix |

- [ ] **Steps A–E.** Commit: `feat: add 02_patterns exercises`.

---

### Task 11: Chapter 03_collections (6 exercises)

**Files:** Create `exercises/03_collections/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 031_collection_expressions.cs | collection expressions (C# 12) | placeholder | `[1, 2, 3]` replaces `new[] {...}` / `new List<int> {...}`; Unity note: waypoint/loot tables |
| 032_target_typing.cs | one syntax, many targets (C# 12) | placeholder | same `[..]` initializing `int[]`, `List<int>`, `Span<int>`, `ImmutableArray<int>` |
| 033_spread.cs | spread element (C# 12) | placeholder | `[..a, ..b, bonus]` merging; Unity note: combining drop tables |
| 034_empty_collections.cs | `[]` empty literal (C# 12) | output | replaces `Array.Empty<T>()` / `new List<T>()`; allocation notes in comments |
| 035_params_collections.cs | `params ReadOnlySpan<T>` (C# 13) | placeholder | params without the hidden array allocation; Unity note: per-frame variadic helpers |
| 036_index_in_initializer.cs | implicit indexer access in object initializers (C# 13) | placeholder | `^1` inside an initializer |

- [ ] **Steps A–E.** Commit: `feat: add 03_collections exercises`.

Representative exemplar for the `placeholder` style in this chapter (031, abbreviated to the code section; full metadata per contract):

```csharp
int[] fibonacci = ???;                       // was: new int[] { 1, 1, 2, 3, 5 }
List<string> party = [???, "Ryu", "Ken"];    // one member is missing
int[] merged = [..fibonacci, ..fibonacci];

Console.WriteLine(string.Join(' ', fibonacci));
Console.WriteLine(string.Join(' ', party));
Console.WriteLine(merged.Length);
```

---

### Task 12: Chapter 04_strings (5 exercises)

**Files:** Create `exercises/04_strings/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 041_raw_strings.cs | raw string literals (C# 11) | compile | escaped-JSON hell rewritten as `"""..."""`; Unity note: shader snippets, JSON assets |
| 042_raw_interpolation.cs | interpolated raw strings (C# 11) | placeholder | `$$"""` and `{{value}}` when the payload itself uses braces |
| 043_utf8_literals.cs | UTF-8 string literals (C# 11) | placeholder | `"..."u8` → `ReadOnlySpan<byte>`; Unity note: network payloads without encoding cost |
| 044_const_interpolation.cs | const interpolated strings (C# 10) | compile | `const string` built from other consts via `$"..."` |
| 045_expressions_in_holes.cs | newlines/complex expressions in interpolation holes (C# 11) | placeholder | a switch expression inside `{...}` |

- [ ] **Steps A–E.** Commit: `feat: add 04_strings exercises`.

---

### Task 13: Chapter 05_types (7 exercises)

**Files:** Create `exercises/05_types/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 051_file_scoped_namespace.cs | file-scoped namespaces (C# 10) | placeholder | one indent level gone; Unity note: known editor quirk report — see feature matrix |
| 052_alias_any_type.cs | `using X = (int A, int B);` alias any type (C# 12) | placeholder | tuple aliases for lightweight domain types |
| 053_required_members.cs | `required` members (C# 11) | compile | object-initializer safety without constructor explosion; Unity note: needs PolySharp under Mono |
| 054_required_and_init.cs | `required` + `init` interplay (C# 11) | output | who can set what, when |
| 055_field_keyword.cs | `field` keyword (C# 14) | placeholder | validated auto-property without a named backing field |
| 056_file_local_types.cs | `file` types (C# 11) | placeholder | file-private helpers — exactly what these exercise files use |
| 057_nameof_unbound.cs | `nameof(List<>)` unbound generics (C# 14) | placeholder | small but emblematic C# 14 nicety |

- [ ] **Steps A–E.** Commit: `feat: add 05_types exercises`.

---

### Task 14: Chapter 06_generics (4 exercises)

**Files:** Create `exercises/06_generics/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 061_static_abstract.cs | static abstract interface members (C# 11) | compile | the mechanism behind generic math; Unity note: NOT available on Mono — 6.8/CoreCLR territory, see feature matrix |
| 062_generic_math.cs | `INumber<T>` (C# 11 + .NET BCL) | placeholder | one `Sum<T>` for int/float/decimal; Unity note: no more per-type math helpers |
| 063_generic_attributes.cs | generic attributes (C# 11) | placeholder | `[Handler<TEvent>]`; Unity note: crashes Mono at runtime per matrix — reflection-read in the exercise |
| 064_parsable.cs | `IParsable<T>` (C# 11 era BCL) | output | generic parsing helper fixed to use the interface |

- [ ] **Steps A–E.** Commit: `feat: add 06_generics exercises`.

---

### Task 15: Chapter 07_performance (6 exercises)

**Files:** Create `exercises/07_performance/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 071_span_slicing.cs | `Span<T>`/`AsSpan` slicing (C# 7.3+, new to C#-9 Unity habits) | placeholder | zero-alloc substring/segment views; Unity note: string GC pressure per frame |
| 072_stackalloc_span.cs | `stackalloc` into `Span<T>` | placeholder | scratch buffers without the heap |
| 073_inline_arrays.cs | inline arrays (C# 12) | compile | fixed-size buffer struct + `[InlineArray]`; Unity note: NOT supported on Mono — matrix link |
| 074_ref_readonly.cs | `ref readonly` parameters (C# 12) | placeholder | big-struct passing semantics vs `in` |
| 075_first_class_spans.cs | first-class span conversions (C# 14) | compile | span extension method called directly on an array — legal only in C# 14 |
| 076_allows_ref_struct.cs | `allows ref struct` + ref struct interfaces (C# 13) | compile | generic APIs that accept `Span<T>`; Unity note: NOT supported on Mono — matrix link |

- [ ] **Steps A–E.** Commit: `feat: add 07_performance exercises`.

---

### Task 16: Chapter 08_extensions (4 exercises)

**Files:** Create `exercises/08_extensions/` files per table.

| file | feature (version) | break | teaching point / Unity note angle |
|---|---|---|---|
| 081_extension_members.cs | `extension` blocks, extension properties (C# 14) | placeholder | properties on types you don't own (System.Numerics.Vector2) |
| 082_static_extensions.cs | static extension members (C# 14) | placeholder | `int.ParseOrZero(...)`-style factory helpers |
| 083_null_conditional_assignment.cs | `target?.Member = value` (C# 14) | placeholder | assignment that no-ops on null; Unity note: the fake-null trap — UnityEngine.Object's overloaded == makes this pattern misleading there |
| 084_capstone.cs | everything combined | placeholder | a small C#-9-style inventory system modernized end-to-end: records, patterns, collection expressions, extension members; multiple ??? sites |

- [ ] **Steps A–E.** Commit: `feat: add 08_extensions exercises`.

---

### Task 17: Bilingual README

**Files:**
- Modify: `README.md` (replace stub)

**Interfaces:**
- Consumes: everything user-facing built so far.

- [ ] **Step 1: Write README.md** with this exact section plan, every section EN first then JA:

1. **What this is** — ziglings-style course for C# 10–14 aimed at Unity engineers stuck at C# 9; not affiliated with ziglings, inspired by it.
2. **Why .NET 10 and not Unity (yet)** — the verified facts: Unity 6.7 alpha ships an experimental CoreCLR Desktop Player but stays at C# 9 / .NET Standard 2.1; C# 14 + .NET 10 arrive with Unity 6.8. Links to the two Unity Discussions sources from the spec.
3. **Setup** — `mise install` (or any .NET 10.0.3xx SDK; `global.json` enforces it), verify with `dotnet --version`.
4. **How to work** — run `dotnet run --project tools/runner` from anywhere in the repo; edit the failing exercise; re-run. Hints are in the file; solutions are intentionally not in the repo; escalation path: EXPECTED OUTPUT → HINT → Docs link → ask an AI as the last resort.
5. **Course map** — the 9-chapter table from the spec with per-chapter feature/version summary.
6. **Unity side** — pointer to `unity/` zones (Contrasts, Lab), `docs/unity-lab-setup.md`, `docs/feature-matrix.md`, and the After~ folder trick (`~` folders are invisible to Unity's compilation pipeline; rename when 6.8 lands).
7. **Runner reference** — flags (`--root`, `--no-cache`, `--verify-broken`), cache file location, exit codes.

- [ ] **Step 2: Verify claims** — every command in the README must be run once as written; both Unity source links checked with `curl -sI` (expect 200).

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "docs: write bilingual readme"
```

---

### Task 18: Unity docs — lab setup guide and feature matrix

**Files:**
- Create: `docs/unity-lab-setup.md`, `docs/feature-matrix.md`

**Interfaces:**
- Produces: the documents Tasks 20–21 and the learner's experiments reference.

- [ ] **Step 1: Write docs/feature-matrix.md** — EN/JA intro plus one table. Columns: `Feature (C# ver)` | `Compiles via csc.rsp?` | `Mono editor/player` | `CoreCLR player (6.7 exp)` | `Unity 6.8 (expected)` | `Notes`. Seed rows from the UnityRoslynUpdater README research (2026-07-11), all CoreCLR-player cells start as `untested`:
  - Working on Mono once compilable: collection expressions (12), primary constructors (12), raw strings (11), list patterns (11), UTF-8 literals (11), file-local types (11), params collections (13), extension members (14), `field` (14), null-conditional assignment (14), `nameof` improvements (14).
  - Needs PolySharp polyfill: required members (11), interpolated string handlers (10), `CallerArgumentExpression` (10).
  - Not supported on Mono: static abstract members (11), ref fields (11), inline arrays (12), interceptors (12), ref struct interfaces (13), new `Lock` type (13); generic attributes (11) marked `crash`.
  - Include the known report: file-scoped namespaces compiled but MonoBehaviour detection failed in older Unity versions — status `verify locally`.
- [ ] **Step 2: Write docs/unity-lab-setup.md** — EN/JA, sections:
  1. Facts and risk disclaimer (unofficial, alpha, editor-install-modifying; everything reversible).
  2. Stage 0 — probe the bundled compiler: `find /Applications/Unity/Hub/Editor/6000.7.0a2 -maxdepth 5 -iname 'csc*' -o -iname '*Roslyn*' | head`, then invoke the found csc with `-langversion:?` to list accepted versions. Record results in feature-matrix.md.
  3. Stage 1 (non-invasive) — `Assets/Lab/csc.rsp` with `-langversion:preview`; what to expect per the matrix.
  4. Stage 2 (invasive) — UnityRoslynUpdater: clone/build/run steps from its README, macOS caveats (unverified — document actual results), how an editor update reverts it, and manual restore (re-copy the backed-up `DotNetSdkRoslyn` directory; back it up BEFORE patching).
  5. IDE alignment — `com.unity.ide.visualstudio` ≥ 2.0.24 or CsprojLangVersionProcessor (check its README for the current UPM install URL).
  6. PolySharp installation for the polyfill rows (NuGetForUnity or manual DLL + RoslynAnalyzer label; verify against PolySharp README).
- [ ] **Step 3: Verify all external links with `curl -sI` (expect 200), commit**

```bash
git add docs/
git commit -m "docs: add unity lab setup and feature matrix"
```

---

### Task 19: USER GATE — create the Unity project

Blocked on the user; nothing here is Claude-executable.

- [ ] User: in Unity Hub, create a new project, editor version **6000.7.0a2**, default (Universal 3D) template, **location = the sharplings repo root, project name = `unity`** (Hub creates `<location>/<name>`, so this yields `sharplings/unity/`). Include the experimental CoreCLR Desktop Player component in the editor install if offered.
- [ ] User: open the project once so `Library/` generates; confirm no compile errors in Console.
- [ ] Claude (after the above): `git add unity/ && git status` — confirm only `Assets/`, `Packages/`, `ProjectSettings/` are tracked (the Task 1 .gitignore keeps `Library/` etc. out), then `git commit -m "chore: add unity project skeleton"`.
- [ ] Claude: run the Stage 0 compiler probe from `docs/unity-lab-setup.md`, record findings in `docs/feature-matrix.md`, commit `docs: record bundled roslyn probe results`.

---

### Task 20: Unity Contrasts (Before/After pairs)

**Files:**
- Create per topic under `unity/Assets/Contrasts/<topic>/`: `Before/<Topic>Before.cs`, `After~/<Topic>After.cs`, `README.md`

**Interfaces:**
- Consumes: Unity project from Task 19; feature facts from `docs/feature-matrix.md`.
- Produces: six self-contained comparison studies. Before scripts MUST compile under Unity 6.7 (C# 9). After~ scripts target C# 14 and live in `~`-suffixed folders Unity ignores.

Topics (each README: EN/JA — the idiom, why the modern form is better, which Unity version/runtime it needs):

| topic | Before (C# 9 Unity idiom) | After~ (modern C#) |
|---|---|---|
| 01_null_handling | `if (target != null && target.enabled)` chains + fake-null explanation | pattern matching `is { enabled: true }`, `?.` caveats with UnityEngine.Object, null-conditional assignment (14) |
| 02_data_modeling | mutable `[Serializable]` class DTOs with ctor boilerplate | `record`/`record struct`, `required`, `init` — plus an honest note: Unity's serializer does not handle records; use them for runtime domain data, not inspector data |
| 03_state_machines | enum field + `switch` statements with `break` | `switch` expressions over `(state, trigger)` tuples, list patterns for combo detection |
| 04_coroutines_async | `IEnumerator`/`yield return new WaitForSeconds(...)` | `async Awaitable` (Unity 6 API) + modern async idioms |
| 05_collections_pooling | `new List<T>()` per call, manual array juggling | collection expressions, spread, `Span<T>` slicing over pooled buffers |
| 06_static_utilities | `static class VectorUtils { public static ... }` call-site noise | C# 14 `extension` members: properties and static extensions on Vector3 |

- [ ] **Step 1: Author 01_null_handling fully** (exemplar): Before script as a MonoBehaviour with `[ContextMenu("Run")]` demo method logging via `Debug.Log`; After~ mirror with modern syntax; README explaining the UnityEngine.Object `==` overload vs C# null-conditionals — the one place where modern C# can silently change Unity behavior.
- [ ] **Step 2: Author the remaining five topics** to the same shape (Before compiles today; After~ compiles on 6.8 — noted in each README header).
- [ ] **Step 3: Verify** — user (or batchmode CLI if available: `Unity -batchmode -quit -projectPath unity -logFile -`) confirms the project still compiles with zero errors: After~ folders must be invisible to Unity.
- [ ] **Step 4: Commit** — `git add unity/Assets/Contrasts && git commit -m "feat: add unity before/after contrasts"`.

---

### Task 21: Unity Lab zone

**Files:**
- Create: `unity/Assets/Lab/Sharplings.Lab.asmdef`, `unity/Assets/Lab/csc.rsp`, `unity/Assets/Lab/README.md`, plus three probe scripts.

**Interfaces:**
- Consumes: Task 19 project, Task 18 docs.
- Produces: an isolated assembly where compiler experiments cannot break the rest of the project.

- [ ] **Step 1: Create the isolation assembly**

`unity/Assets/Lab/Sharplings.Lab.asmdef`:

```json
{
    "name": "Sharplings.Lab",
    "rootNamespace": "Sharplings.Lab",
    "references": [],
    "autoReferenced": false
}
```

`unity/Assets/Lab/csc.rsp`:

```
-langversion:preview
```

- [ ] **Step 2: Add three probe scripts**, each a tiny MonoBehaviour with a `[ContextMenu("Probe")]` method logging PASS via `Debug.Log`, one per compile-time feature family: `ProbeCollectionExpressions.cs` (C# 12), `ProbeRawStrings.cs` (C# 11), `ProbeExtensionMembers.cs` (C# 14). Each file header comments (EN/JA) state: which csc.rsp/RoslynUpdater stage it needs, and where to record the result in `docs/feature-matrix.md`.
- [ ] **Step 3: Lab README.md** (EN/JA): the two-stage workflow, link to `docs/unity-lab-setup.md`, and the rule that experiment outcomes get recorded in the feature matrix (the lab notebook contract).
- [ ] **Step 4: User runs Stage 1** (probes with bundled Roslyn), records results; optionally Stage 2 (UnityRoslynUpdater) and the CoreCLR-player runtime experiments.
- [ ] **Step 5: Commit** — `git add unity/Assets/Lab && git commit -m "feat: add unity lab zone with csc.rsp"`.

---

## Verification (whole project)

- `dotnet test tools/runner.tests` — full suite green.
- `dotnet run --project tools/runner -- --verify-broken` — 47 exercises checked, 0 violations.
- `dotnet run --project tools/runner` — stops at `001_hello.cs` with a CompileError verdict and bilingual hints (the intended day-one experience).
- README instructions reproduce from a clean clone (`mise install` → `dotnet --version` → runner).
- Unity project compiles clean with Contrasts Before + Lab (Stage 1) in place; After~ invisible.
