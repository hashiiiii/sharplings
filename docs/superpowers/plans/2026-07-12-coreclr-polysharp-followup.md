# CoreCLR verification and PolySharp enablement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Verify the Lab's modern-C# probes on the CoreCLR Desktop Player and enable PolySharp-backed attribute features on Mono, recording both in `docs/feature-matrix.md`.

**Architecture:** Extend the existing "probe + headless run + feature-matrix record" pattern. Track A adds a player-side runtime harness (reflection over the same probe classes the editor runner already uses) plus a headless CoreCLR build script, then a staged set of runtime-dependent frontier probes. Track B installs the PolySharp analyzer DLL into the Lab assembly and adds attribute-driven probes. All changes stay inside `unity/Assets/Lab/` and `docs/`.

**Tech Stack:** Unity 6000.7.0a2 (CoreCLR Desktop Player), C# (Lab assembly at `-langversion:preview` + `SHARPLINGS_STAGE2`), PolySharp 1.16.0 source generator.

## Global Constraints

- Verification is not xUnit. Each probe/harness task's "test" is a headless Unity batchmode run whose log is grepped for `PASS:` / `SKIP:` / `FAIL:` / `error CS`. No mocks or stubs.
- The Unity editor binary is `/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity`. Referred to below as `$UNITY`. Export it once: `UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"`.
- Unity locks the project; close any open editor before every batchmode command. Batchmode runs take minutes.
- All changes stay inside `unity/Assets/Lab/` and `docs/`. Do not touch `exercises/` or `tools/`.
- Stage 2 swap must be live before any C# 14 or CoreCLR probe run: verify with `docs/unity-lab-setup.md` section 4, step 5 (offline `csc.dll -version` prints `5.6.x`, `-langversion:?` lists 14.0). If reverted, redo the swap or drop the `-define:SHARPLINGS_STAGE2` line first.
- Probe convention (unchanged): each probe is a `MonoBehaviour` in the `Sharplings.Lab` namespace/assembly with a `[ContextMenu("Probe")] private void Probe()` that `Debug.Log`s a line starting `PASS:` or `SKIP:`.
- CoreCLR player build output goes to OS temp (`$TMPDIR` / `Path.GetTempPath()`), never into the repo — nothing under `unity/` build output is committed.
- Identifiers and infra code comments are English only. `docs/feature-matrix.md` records are EN/JA bilingual, half-width space between full-width and half-width characters, no emojis (PASS/SKIP and ✓/✗ allowed).
- Commits follow hashiiiii-git: one line, `<type>: <subject>`, English imperative, lowercase first word, ≤ 50 chars, no body. Work on branch `docs/coreclr-polysharp-followup` (already created) or a fresh `feat/…` branch.
- Spec: `docs/superpowers/specs/2026-07-12-coreclr-polysharp-followup-design.md`.

---

## File Structure

- `unity/Assets/Lab/ProbeRuntimeRunner.cs` — NEW. Runtime (player) counterpart of the editor `ProbeRunner`; runs every probe on `Start()` and quits.
- `unity/Assets/Lab/Editor/PlayerBuilder.cs` — NEW. Headless CoreCLR player build (editor-only assembly).
- `unity/Assets/Lab/Editor/CoreClrSpike.cs` — NEW then DELETED in Task 1. Throwaway backend-selection probe.
- `unity/Assets/Lab/ProbeInlineArrays.cs`, `ProbeRefFields.cs`, `ProbeStaticAbstract.cs` — NEW. Frontier (runtime-dependent) probes, each behind its own define.
- `unity/Assets/Lab/ProbeRequiredMembers.cs`, `ProbeInterpolatedHandler.cs`, `ProbeCallerArgumentExpression.cs` — NEW. PolySharp-backed probes.
- `unity/Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll` (+ `.dll.meta`) — NEW. The analyzer.
- `unity/Assets/Lab/Editor/PolySharpConfigurator.cs` — NEW then DELETED in Task 7. One-shot plugin-import configurator.
- `docs/feature-matrix.md` — MODIFY. New dated EN/JA result blocks and table cell updates.

---

## Track A — CoreCLR Desktop Player verification

### Task 1: Spike — confirm the CoreCLR backend selection API

**Files:**
- Create then delete: `unity/Assets/Lab/Editor/CoreClrSpike.cs`

**Interfaces:**
- Produces: the exact API to select CoreCLR for a StandaloneOSX build, consumed by Task 3. Either `ScriptingImplementation.CoreCLR` (enum value) or the Build Profile API, recorded in the plan notes / commit message.

- [ ] **Step 1: Write the spike script**

`unity/Assets/Lab/Editor/CoreClrSpike.cs`:

```csharp
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Sharplings.Lab.Editor
{
    // Throwaway: discover how 6000.7.0a2 exposes the CoreCLR Desktop Player
    // to the build API. Deleted at the end of Task 1.
    public static class CoreClrSpike
    {
        public static void Report()
        {
            Debug.Log("SPIKE: ScriptingImplementation = " +
                string.Join(", ", Enum.GetNames(typeof(ScriptingImplementation))));
            Debug.Log("SPIKE: Standalone backend = " +
                PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone));
        }
    }
}
```

- [ ] **Step 2: Run the spike**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-spike.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.CoreClrSpike.Report -logFile "$LOG"
grep "SPIKE:" "$LOG"
```

Expected: a line listing the `ScriptingImplementation` enum names.

- [ ] **Step 3: Decide the build API and record it**

- If `CoreCLR` appears in the enum list → Task 3 uses `PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.CoreCLR)`.
- If not → inspect `UnityEditor.Build.Profile.BuildProfile` (6.7 build profiles) for a CoreCLR standalone profile and record that API instead.
- If neither can select CoreCLR headlessly → record "CoreCLR not selectable via build API on 6000.7.0a2" in `docs/feature-matrix.md` (new dated note), **skip Tasks 2–6, and jump to Track B (Task 7).**

Write the confirmed API into the Task 3 commit message body-less subject is not enough, so note it in `docs/feature-matrix.md` under a new "CoreCLR build API (2026-07-12)" line.

- [ ] **Step 4: Delete the spike and commit the note**

```bash
rm unity/Assets/Lab/Editor/CoreClrSpike.cs unity/Assets/Lab/Editor/CoreClrSpike.cs.meta
git add docs/feature-matrix.md
git commit -m "docs: record coreclr build api probe"
```

---

### Task 2: Player-side runtime harness

**Files:**
- Create: `unity/Assets/Lab/ProbeRuntimeRunner.cs`

**Interfaces:**
- Consumes: the probe convention (`MonoBehaviour` + private `Probe()`).
- Produces: `Sharplings.Lab.ProbeRuntimeRunner` (a `MonoBehaviour`) — consumed by Task 3's build (placed on the bootstrap scene's GameObject).

- [ ] **Step 1: Write the harness**

`unity/Assets/Lab/ProbeRuntimeRunner.cs`:

```csharp
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sharplings.Lab
{
    // Runtime counterpart of Assets/Lab/Editor/ProbeRunner.cs. A built player
    // has no -executeMethod entry point, so this MonoBehaviour runs every Lab
    // probe on Start and quits. PlayerBuilder puts it on the bootstrap scene.
    // Uses only reflection + Debug.Log, so it stays valid C# 9 even though the
    // Lab assembly compiles at -langversion:preview.
    public sealed class ProbeRuntimeRunner : MonoBehaviour
    {
        private void Start()
        {
            var probeTypes = typeof(ProbeRuntimeRunner).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                    && type != typeof(ProbeRuntimeRunner)
                    && typeof(MonoBehaviour).IsAssignableFrom(type))
                .OrderBy(type => type.Name)
                .ToArray();

            Debug.Log($"ProbeRuntimeRunner: running {probeTypes.Length} probe(s).");

            foreach (var type in probeTypes)
            {
                var probe = type.GetMethod("Probe",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (probe == null)
                {
                    Debug.Log($"NO-PROBE: {type.Name} has no private Probe(); skipped.");
                    continue;
                }

                var component = gameObject.AddComponent(type);
                try
                {
                    probe.Invoke(component, null);
                }
                catch (Exception exception)
                {
                    Debug.Log($"FAIL: {type.Name}.Probe() threw: {exception.InnerException ?? exception}");
                }
            }

            Application.Quit(0);
        }
    }
}
```

- [ ] **Step 2: Compile-check the Lab assembly headlessly**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-compile.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity -logFile "$LOG"
grep -c "error CS" "$LOG"
```

Expected: `0`. (The editor `ProbeRunner` will treat `ProbeRuntimeRunner` as NO-PROBE and skip it — that is correct.)

- [ ] **Step 3: Commit**

```bash
git add unity/Assets/Lab/ProbeRuntimeRunner.cs unity/Assets/Lab/ProbeRuntimeRunner.cs.meta
git commit -m "feat: add player-side probe runtime runner"
```

---

### Task 3: Headless CoreCLR player build script

**Files:**
- Create: `unity/Assets/Lab/Editor/PlayerBuilder.cs`

**Interfaces:**
- Consumes: `Sharplings.Lab.ProbeRuntimeRunner` (Task 2); the backend-selection API confirmed in Task 1.
- Produces: `Sharplings.Lab.Editor.PlayerBuilder.BuildCoreCLR` — an `-executeMethod` entry that builds a headless macOS CoreCLR player to a temp path.

- [ ] **Step 1: Write the builder**

`unity/Assets/Lab/Editor/PlayerBuilder.cs` (replace the `SetScriptingBackend` line with the exact API from Task 1 if it differs):

```csharp
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sharplings.Lab.Editor
{
    // Headless build of a macOS standalone player that runs the Lab probes via
    // ProbeRuntimeRunner under the CoreCLR backend. Output goes to a temp path
    // (via -buildOutput <path> or SHARPLINGS_BUILD_OUTPUT), never into the repo.
    public static class PlayerBuilder
    {
        public static void BuildCoreCLR()
        {
            string output = ArgOrEnv("-buildOutput", "SHARPLINGS_BUILD_OUTPUT")
                ?? Path.Combine(Path.GetTempPath(), "sharplings-coreclr-player", "Lab.app");
            string scenePath = "Assets/Lab/ProbeBootstrap.unity";

            try
            {
                var scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var host = new GameObject("ProbeRuntimeRunner");
                host.AddComponent<ProbeRuntimeRunner>();
                EditorSceneManager.SaveScene(scene, scenePath);

                // Backend selection confirmed in Task 1.
                PlayerSettings.SetScriptingBackend(
                    NamedBuildTarget.Standalone, ScriptingImplementation.CoreCLR);

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { scenePath },
                    locationPathName = output,
                    target = BuildTarget.StandaloneOSX,
                    options = BuildOptions.None,
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                Debug.Log($"BUILD: result={report.summary.result} output={output}");
                EditorApplication.Exit(
                    report.summary.result == BuildResult.Succeeded ? 0 : 1);
            }
            finally
            {
                AssetDatabase.DeleteAsset(scenePath);
            }
        }

        private static string ArgOrEnv(string argName, string envName)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == argName) return args[i + 1];
            string env = Environment.GetEnvironmentVariable(envName);
            return string.IsNullOrEmpty(env) ? null : env;
        }
    }
}
```

- [ ] **Step 2: Run the build**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
OUT="$TMPDIR/sharplings-coreclr-player/Lab.app"
LOG="$TMPDIR/sharplings-build.log"
rm -rf "$TMPDIR/sharplings-coreclr-player"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.PlayerBuilder.BuildCoreCLR \
  -buildOutput "$OUT" -logFile "$LOG"
grep -E "BUILD:|error CS" "$LOG"; test -d "$OUT" && echo "APP EXISTS"
```

Expected: `BUILD: result=Succeeded ...` and `APP EXISTS`. If the build fails on the backend line, correct it per the Task 1 finding and rerun.

- [ ] **Step 3: Commit**

```bash
git add unity/Assets/Lab/Editor/PlayerBuilder.cs unity/Assets/Lab/Editor/PlayerBuilder.cs.meta
git commit -m "feat: add headless coreclr player build"
```

---

### Task 4: Run existing probes on CoreCLR and record

**Files:**
- Modify: `docs/feature-matrix.md`

**Interfaces:**
- Consumes: the built player from Task 3.

- [ ] **Step 1: Run the built player and capture the log**

```bash
OUT="$TMPDIR/sharplings-coreclr-player/Lab.app"
BIN="$(ls "$OUT/Contents/MacOS/"* | head -1)"
PLOG="$TMPDIR/sharplings-player.log"
"$BIN" -batchmode -nographics -logFile "$PLOG"
grep -E "PASS:|SKIP:|FAIL:" "$PLOG"
```

Expected: seven `PASS:` lines (5 C# 14 + 2 Stage 1 probes) with correct runtime values, e.g. `PASS: extension members (C# 14) compiled. value.Doubled = 42`. Zero `SKIP:`/`FAIL:`.

- [ ] **Step 2: Record results in the feature matrix**

Add a dated EN/JA result block to `docs/feature-matrix.md` (mirroring the "Stage 2 results" block) stating the CoreCLR player run passed all 7 probes with values, and update the `CoreCLR player (6.7 exp)` column from `untested` to `Working — CoreCLR player PASS 2026-07-12` for these rows: Collection expressions, Raw string literals, Extension members, `field` keyword, Null-conditional assignment, `nameof` improvements, First-class span conversions.

- [ ] **Step 3: Commit**

```bash
git add docs/feature-matrix.md
git commit -m "docs: record coreclr player probe pass"
```

---

### Task 5: Add frontier (runtime-dependent) probes

**Files:**
- Create: `unity/Assets/Lab/ProbeInlineArrays.cs`, `unity/Assets/Lab/ProbeRefFields.cs`, `unity/Assets/Lab/ProbeStaticAbstract.cs`

**Interfaces:**
- Produces: three guarded `MonoBehaviour` probes. Each is guarded by its own define (`SHARPLINGS_INLINE_ARRAYS`, `SHARPLINGS_REF_FIELDS`, `SHARPLINGS_STATIC_ABSTRACT`) so the default build (none defined) stays green with all three logging `SKIP:`.

- [ ] **Step 1: Write `ProbeInlineArrays.cs`**

```csharp
using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: inline arrays (C# 12). Runtime-dependent — needs
    // System.Runtime.CompilerServices.InlineArrayAttribute and runtime layout
    // support Mono's .NET Standard 2.1 lacks. Guarded by its own define so it
    // never breaks the baseline build; enable SHARPLINGS_INLINE_ARRAYS in
    // Assets/Lab/csc.rsp to run the experiment (Task 6).
    public class ProbeInlineArrays : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_INLINE_ARRAYS
            Buffer4 buffer = default;
            buffer[0] = 3; buffer[1] = 4; buffer[2] = 5; buffer[3] = 6;
            int sum = 0;
            foreach (int value in buffer) sum += value;
            Debug.Log($"PASS: inline arrays (C# 12) ran. sum = {sum}");
#else
            Debug.Log("SKIP: inline arrays probe needs SHARPLINGS_INLINE_ARRAYS.");
#endif
        }
    }

#if SHARPLINGS_INLINE_ARRAYS
    [System.Runtime.CompilerServices.InlineArray(4)]
    internal struct Buffer4 { private int _element; }
#endif
}
```

- [ ] **Step 2: Write `ProbeRefFields.cs`**

```csharp
using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: ref fields (C# 11). Runtime-dependent — a ref field in a
    // ref struct needs runtime byref-field support Mono lacks. Guarded by its
    // own define; enable SHARPLINGS_REF_FIELDS in csc.rsp to run (Task 6).
    public class ProbeRefFields : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_REF_FIELDS
            int backing = 40;
            var holder = new RefHolder(ref backing);
            holder.Value += 2;
            Debug.Log($"PASS: ref fields (C# 11) ran. backing = {backing}");
#else
            Debug.Log("SKIP: ref fields probe needs SHARPLINGS_REF_FIELDS.");
#endif
        }
    }

#if SHARPLINGS_REF_FIELDS
    internal ref struct RefHolder
    {
        public ref int Value;
        public RefHolder(ref int value) { Value = ref value; }
    }
#endif
}
```

- [ ] **Step 3: Write `ProbeStaticAbstract.cs`**

```csharp
using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: static abstract interface members (C# 11). Uses a custom
    // interface (no BCL generic-math dependency) so the signal isolates the
    // runtime feature — static virtual/abstract interface dispatch — that Mono
    // lacks. Guarded by its own define; enable SHARPLINGS_STATIC_ABSTRACT to
    // run (Task 6).
    public class ProbeStaticAbstract : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STATIC_ABSTRACT
            Debug.Log($"PASS: static abstract members (C# 11) ran. Zero = {ZeroOf<MyInt>()}");
#else
            Debug.Log("SKIP: static abstract probe needs SHARPLINGS_STATIC_ABSTRACT.");
#endif
        }

#if SHARPLINGS_STATIC_ABSTRACT
        private static T ZeroOf<T>() where T : IZero<T> => T.Zero;
#endif
    }

#if SHARPLINGS_STATIC_ABSTRACT
    internal interface IZero<T> where T : IZero<T> { static abstract T Zero { get; } }

    internal readonly struct MyInt : IZero<MyInt>
    {
        public static MyInt Zero => new MyInt(0);
        private readonly int _value;
        public MyInt(int value) { _value = value; }
        public override string ToString() => _value.ToString();
    }
#endif
}
```

- [ ] **Step 4: Compile-check the baseline (no frontier define set)**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-frontier-baseline.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll -logFile "$LOG"
grep -cE "error CS" "$LOG"; grep -E "SKIP: (inline arrays|ref fields|static abstract)" "$LOG"
```

Expected: `0` errors; three `SKIP:` lines for the new probes (baseline stays green).

- [ ] **Step 5: Commit**

```bash
git add unity/Assets/Lab/ProbeInlineArrays.cs unity/Assets/Lab/ProbeInlineArrays.cs.meta \
        unity/Assets/Lab/ProbeRefFields.cs unity/Assets/Lab/ProbeRefFields.cs.meta \
        unity/Assets/Lab/ProbeStaticAbstract.cs unity/Assets/Lab/ProbeStaticAbstract.cs.meta
git commit -m "feat: add runtime-dependent frontier probes"
```

---

### Task 6: Run the frontier experiments and record

**Files:**
- Modify: `unity/Assets/Lab/csc.rsp` (temporarily, per feature), `docs/feature-matrix.md`

**Interfaces:**
- Consumes: Task 5's probes; Task 3's `PlayerBuilder`; the editor `ProbeRunner`.

For each of the three features, run this enable → editor-run → coreclr-run → record → disable cycle. Example for inline arrays (repeat with `SHARPLINGS_REF_FIELDS` and `SHARPLINGS_STATIC_ABSTRACT`):

- [ ] **Step 1: Enable the define**

Add a line to `unity/Assets/Lab/csc.rsp` (keep the existing two lines):

```
-define:SHARPLINGS_INLINE_ARRAYS
```

- [ ] **Step 2: Run on the Mono editor backend and capture the outcome**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-frontier-editor.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll -logFile "$LOG"
grep -E "error CS|PASS: inline arrays|FAIL: ProbeInlineArrays" "$LOG"
```

Record the exact outcome: compiles + PASS, compiles + FAIL (runtime), or `error CSxxxx` (does not compile). Any of the three is a valid recorded result.

- [ ] **Step 3: Build and run the CoreCLR player (only if Step 2 compiled)**

```bash
OUT="$TMPDIR/sharplings-coreclr-player/Lab.app"; PLOG="$TMPDIR/sharplings-frontier-player.log"
rm -rf "$TMPDIR/sharplings-coreclr-player"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.PlayerBuilder.BuildCoreCLR -buildOutput "$OUT" -logFile "$TMPDIR/b.log"
BIN="$(ls "$OUT/Contents/MacOS/"* | head -1)"; "$BIN" -batchmode -nographics -logFile "$PLOG"
grep -E "PASS: inline arrays|FAIL: ProbeInlineArrays" "$PLOG"
```

Record whether the same feature that failed/skipped on Mono runs on CoreCLR — this differential is the point of the experiment. If Step 2 did not compile, note "does not compile under Stage 2 Roslyn against the .NET Standard 2.1 BCL" and skip the player build for this feature.

- [ ] **Step 4: Disable the define**

Remove the `-define:SHARPLINGS_INLINE_ARRAYS` line from `unity/Assets/Lab/csc.rsp` so the baseline returns to green.

- [ ] **Step 5: After all three features, record and commit**

Add a dated EN/JA "Frontier probe results (2026-07-12)" block to `docs/feature-matrix.md` giving, per feature, the Mono-editor outcome and the CoreCLR-player outcome. Update the `Mono editor/player` and `CoreCLR player (6.7 exp)` cells of the "Inline arrays (C# 12)", "Ref fields (C# 11)", and "Static abstract members (C# 11)" rows accordingly. Confirm `csc.rsp` is back to its two original lines.

```bash
git add docs/feature-matrix.md unity/Assets/Lab/csc.rsp
git commit -m "docs: record frontier probe results on coreclr"
```

---

## Track B — PolySharp enablement

### Task 7: Install and configure the PolySharp analyzer

**Files:**
- Create: `unity/Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll` (+ `.dll.meta`)
- Create then delete: `unity/Assets/Lab/Editor/PolySharpConfigurator.cs`

**Interfaces:**
- Produces: a `RoslynAnalyzer`-labelled plugin scoped to `Sharplings.Lab`, consumed by Tasks 8–9.

- [ ] **Step 1: Fetch and place the analyzer DLL**

```bash
NUPKG="$TMPDIR/polysharp.zip"
curl -sL "https://www.nuget.org/api/v2/package/PolySharp/1.16.0" -o "$NUPKG"
mkdir -p unity/Assets/Lab/Plugins/PolySharp
unzip -p "$NUPKG" "analyzers/dotnet/cs/PolySharp.SourceGenerators.dll" \
  > unity/Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll
test -s unity/Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll && echo "DLL OK"
```

Expected: `DLL OK`. If `curl` cannot reach nuget.org, pause Track B and report (Global Constraint: network dependency).

- [ ] **Step 2: Write the one-shot configurator**

`unity/Assets/Lab/Editor/PolySharpConfigurator.cs`:

```csharp
using UnityEditor;
using UnityEngine;

namespace Sharplings.Lab.Editor
{
    // One-shot: import the PolySharp DLL as a Roslyn analyzer scoped to the Lab
    // assembly. Disables all runtime/editor platforms and adds the
    // RoslynAnalyzer label, which is exactly what the manual Plugin Inspector
    // steps do. Deleted at the end of Task 7; the resulting .dll.meta persists.
    public static class PolySharpConfigurator
    {
        public static void Configure()
        {
            const string path =
                "Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (PluginImporter)AssetImporter.GetAtPath(path);
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);

            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            AssetDatabase.SetLabels(asset, new[] { "RoslynAnalyzer" });

            importer.SaveAndReimport();
            Debug.Log("POLYSHARP: configured as RoslynAnalyzer.");
        }
    }
}
```

- [ ] **Step 3: Run the configurator**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-polysharp.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.PolySharpConfigurator.Configure -logFile "$LOG"
grep -E "POLYSHARP:|error CS" "$LOG"
grep -q "RoslynAnalyzer" unity/Assets/Lab/Plugins/PolySharp/PolySharp.SourceGenerators.dll.meta && echo "META OK"
```

Expected: `POLYSHARP: configured as RoslynAnalyzer.` and `META OK`.

- [ ] **Step 4: Delete the configurator and commit**

```bash
rm unity/Assets/Lab/Editor/PolySharpConfigurator.cs unity/Assets/Lab/Editor/PolySharpConfigurator.cs.meta
git add unity/Assets/Lab/Plugins
git commit -m "build: add polysharp analyzer to lab"
```

---

### Task 8: Required-members smoke test

**Files:**
- Create: `unity/Assets/Lab/ProbeRequiredMembers.cs`
- Modify: `docs/feature-matrix.md`

**Interfaces:**
- Consumes: the PolySharp analyzer (Task 7). This probe is unguarded — its compiling is the proof PolySharp is wired (a `CS0246` for `RequiredMemberAttribute` means it is not).

- [ ] **Step 1: Write the probe**

```csharp
using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp smoke test: required members (C# 11). Unguarded on purpose —
    // if PolySharp's RequiredMember/SetsRequiredMembers/CompilerFeatureRequired
    // shims are not wired, this fails to compile (CS0246). No Stage 2 needed:
    // required members is within -langversion:preview; PolySharp supplies the
    // attributes Mono's BCL lacks.
    public class ProbeRequiredMembers : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            var config = new LabConfig { Name = "Boss" };
            Debug.Log($"PASS: required members (C# 11) compiled. Name = {config.Name}");
        }

        private sealed class LabConfig
        {
            public required string Name { get; init; }
        }
    }
}
```

- [ ] **Step 2: Run and confirm it compiles**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-required.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll -logFile "$LOG"
grep -E "error CS|PASS: required members" "$LOG"
```

Expected: no `error CS` (especially no `CS0246 ... RequiredMemberAttribute`) and `PASS: required members (C# 11) compiled. Name = Boss`.

- [ ] **Step 3: Record and commit**

Update the "Required members (C# 11)" row in `docs/feature-matrix.md`: `Compiles via csc.rsp?` → `yes — with PolySharp, confirmed 2026-07-12`, `Mono editor/player` → `Working — editor PASS 2026-07-12 (PolySharp)`. Add a dated EN/JA note that PolySharp was installed manually and the smoke test passed.

```bash
git add unity/Assets/Lab/ProbeRequiredMembers.cs unity/Assets/Lab/ProbeRequiredMembers.cs.meta docs/feature-matrix.md
git commit -m "feat: add polysharp required members probe"
```

---

### Task 9: Interpolated string handler and CallerArgumentExpression probes

**Files:**
- Create: `unity/Assets/Lab/ProbeInterpolatedHandler.cs`, `unity/Assets/Lab/ProbeCallerArgumentExpression.cs`
- Modify: `docs/feature-matrix.md`

**Interfaces:**
- Consumes: the PolySharp analyzer (Task 7).

- [ ] **Step 1: Write `ProbeCallerArgumentExpression.cs`**

```csharp
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp-backed probe: CallerArgumentExpression (C# 10). PolySharp
    // supplies CallerArgumentExpressionAttribute, which Mono's BCL lacks.
    public class ProbeCallerArgumentExpression : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            int a = 2, b = 3;
            Describe(a + b);
        }

        private static void Describe(int value,
            [CallerArgumentExpression("value")] string expression = "")
        {
            Debug.Log($"PASS: CallerArgumentExpression (C# 10) ran. expr = '{expression}'");
        }
    }
}
```

- [ ] **Step 2: Write `ProbeInterpolatedHandler.cs`**

```csharp
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp-backed probe: interpolated string handlers (C# 10). PolySharp
    // supplies InterpolatedStringHandler/InterpolatedStringHandlerArgument
    // attributes. This minimal handler just concatenates.
    public class ProbeInterpolatedHandler : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            int count = 3;
            LogTo($"count is {count}");
        }

        private static void LogTo(LabHandler handler) =>
            Debug.Log($"PASS: interpolated string handler (C# 10) ran. text = '{handler.Result}'");

        [InterpolatedStringHandler]
        private ref struct LabHandler
        {
            private string _result;
            public string Result => _result;

            public LabHandler(int literalLength, int formattedCount) => _result = "";
            public void AppendLiteral(string s) => _result += s;
            public void AppendFormatted<T>(T value) => _result += value?.ToString();
        }
    }
}
```

- [ ] **Step 3: Run both probes**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/MacOS/Unity"
LOG="$TMPDIR/sharplings-polysharp-probes.log"
"$UNITY" -batchmode -nographics -quit -projectPath unity \
  -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll -logFile "$LOG"
grep -E "error CS|PASS: CallerArgumentExpression|PASS: interpolated string handler" "$LOG"
```

Expected: no `error CS`; both `PASS:` lines present (`expr = 'a + b'`, `text = 'count is 3'`).

- [ ] **Step 4: Record and commit**

Update the "Interpolated string handlers (C# 10)" and "`CallerArgumentExpression` (C# 10)" rows in `docs/feature-matrix.md` to `Working — editor PASS 2026-07-12 (PolySharp)`, and extend the PolySharp result note to cover all three attribute features.

```bash
git add unity/Assets/Lab/ProbeInterpolatedHandler.cs unity/Assets/Lab/ProbeInterpolatedHandler.cs.meta \
        unity/Assets/Lab/ProbeCallerArgumentExpression.cs unity/Assets/Lab/ProbeCallerArgumentExpression.cs.meta \
        docs/feature-matrix.md
git commit -m "feat: add polysharp handler and caller-expr probes"
```

---

## Self-Review

**Spec coverage:** A0 spike → Task 1; A1 harness → Task 2; A2 build → Task 3; A2 run+record → Task 4; A3 frontier probes → Tasks 5–6; B1 install → Task 7; B2 smoke → Task 8; B2 remaining features → Task 9. Acceptance criteria for both tracks map to Tasks 4/6 (A) and 8/9 (B). Cross-cutting isolation/recording constraints are in Global Constraints.

**Placeholder scan:** No TBD/TODO. The one conditional is Task 3's `SetScriptingBackend` line, explicitly resolved by Task 1's Interfaces and flagged inline.

**Type consistency:** `ProbeRuntimeRunner` (Task 2) is consumed by name in Task 3. `PlayerBuilder.BuildCoreCLR` (Task 3) is invoked verbatim in Tasks 4/6. Frontier defines `SHARPLINGS_INLINE_ARRAYS` / `SHARPLINGS_REF_FIELDS` / `SHARPLINGS_STATIC_ABSTRACT` match between Tasks 5 and 6. `PolySharpConfigurator.Configure` (Task 7) matches its run command. Probe log prefixes (`PASS:`/`SKIP:`/`FAIL:`) match every grep.
