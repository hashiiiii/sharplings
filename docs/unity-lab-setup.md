# Unity lab setup

**EN:** Setup notes for `unity/Assets/Lab/` — the experimental zone where unofficial tricks push Unity 6000.7.0a2 past its official C# 9 ceiling, today, ahead of Unity 6.8's official C# 14 support.

**JA:** `unity/Assets/Lab/` —— Unity 6.8 の公式な C# 14 対応を待たずに、非公式の裏技で Unity 6000.7.0a2 の公式な C# 9 の壁を今のうちに超えるための実験ゾーン —— の setup 手順です。

## 1. Facts and risk disclaimer

**EN:** Unity 6.7 alpha (6000.7.0a2) ships an experimental CoreCLR **Desktop Player**, but the scripting language stays at **C# 9** and the API surface stays **.NET Standard 2.1** — CoreCLR here is a new player runtime backend, not a new compiler. **C# 14** and the **.NET 10** toolchain arrive officially with **Unity 6.8** (where Mono is removed). Sources: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299), [Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279), [Unity missing support of C# version 10 or greater](https://discussions.unity.com/t/unity-missing-support-of-c-version-10-or-greater-version-9-as-default/1617516).

Two unofficial tricks close that gap early, at increasing levels of risk:

- **Stage 1** (non-invasive) — a `csc.rsp` response file raises the language version Unity's own bundled Roslyn accepts, without touching the editor install.
- **Stage 2** (invasive) — [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) replaces the editor's bundled Roslyn compiler and .NET runtime outright, unlocking C# syntax all the way to 14.

Both are unofficial: Unity doesn't support, test, or endorse either. Keep the risk in proportion:

- Nothing in this document is required to complete any `exercises/` exercise. Those are plain .NET 10 console apps; the Lab is purely optional exploration for connecting the language features to a real Unity project ahead of schedule.
- Stage 1 is fully reversible — it adds exactly one project-local file (`Assets/Lab/csc.rsp`) and touches nothing outside the project.
- Stage 2 modifies files **inside the Unity editor's own installation folder**, which is why its own README states plainly that administrative privileges are required. Section 4 below covers backing that up before you patch anything, and restoring it afterward.
- 6000.7.0a2 is an alpha build. Its internals (bundled Roslyn version, directory layout, package versions) can change without notice between alpha patches — everything this document says about "what's there" should be treated as a snapshot to re-verify, not a permanent fact.
- `Assets/Lab/` lives in its own assembly definition (`Sharplings.Lab.asmdef`, `autoReferenced: false`), isolated from `Assets/Contrasts/` and the rest of the project. A compiler crash or a bad experiment in the Lab cannot break anything else — that isolation is the actual safety net here, more than any individual backup step.

**JA:** Unity 6.7 alpha（6000.7.0a2）は実験的な CoreCLR **Desktop Player** を搭載しますが、スクリプト言語は **C# 9** のまま、API は **.NET Standard 2.1** のままです —— ここでの CoreCLR は新しい player 実行基盤であり、新しいコンパイラではありません。**C# 14** と **.NET 10** の toolchain が公式に来るのは **Unity 6.8**（Mono 廃止）からです。情報源: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299)、[Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279)、[Unity missing support of C# version 10 or greater](https://discussions.unity.com/t/unity-missing-support-of-c-version-10-or-greater-version-9-as-default/1617516)。

非公式の裏技が 2 つあり、この gap を先取りで埋めます。リスクは段階的に上がります。

- **Stage 1**（非侵襲）—— `csc.rsp` response file で、Unity 同梱の Roslyn が受け付ける言語バージョンを上げます。エディタ install には触れません。
- **Stage 2**（侵襲）—— [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) がエディタ同梱の Roslyn コンパイラと .NET runtime をまるごと差し替え、C# 14 構文まで解禁します。

どちらも非公式です。Unity はどちらもサポート・テスト・公認していません。リスクは以下の通り、身の丈に合わせてください。

- この文書の内容は `exercises/` のどの exercise を終えるためにも必要ありません。exercises は素の .NET 10 console app であり、Lab はあくまで、実際の Unity project へ前倒しで言語機能をつなげるための任意の探求です。
- Stage 1 は完全に revert 可能です —— 追加するのは project 内のファイル 1 つ（`Assets/Lab/csc.rsp`）だけで、project の外には一切触れません。
- Stage 2 は **Unity エディタ自身の install フォルダの内部** を書き換えます。だからこそ、その README は管理者権限が必要だとはっきり書いています。何かをパッチする前にそれを backup し、あとで restore する手順は下の 4 節にまとめています。
- 6000.7.0a2 は alpha build です。内部構造（同梱 Roslyn の version、ディレクトリ構成、package の version）は alpha の patch 間で予告なく変わり得ます。この文書が「そこに何があるか」について書いていることは、恒久的な事実ではなく、都度再確認すべき スナップショット として扱ってください。
- `Assets/Lab/` は独立した assembly definition（`Sharplings.Lab.asmdef`、`autoReferenced: false`）に置かれ、`Assets/Contrasts/` や project の他部分から隔離されています。Lab 内でコンパイラがクラッシュしても、実験が失敗しても、他は壊れません —— 個々の backup 手順よりも、この隔離こそが実質的な安全網です。

## 2. Stage 0 — probe the bundled compiler

**EN:** Before changing anything, find out what Unity 6000.7.0a2 actually ships and what language versions its own bundled Roslyn already accepts. Stage 0 is entirely read-only — it creates or modifies nothing — and its results are exactly what turns the `unverified (probe: Stage 0)` cells in `docs/feature-matrix.md` into real answers. (This is also literally the probe Task 19 of the project plan runs, once the Unity project exists — this section is the instructions for that step.)

1. Locate the bundled compiler and any Roslyn directory under the editor install:

   ```
   find /Applications/Unity/Hub/Editor/6000.7.0a2 -maxdepth 5 -iname 'csc*' -o -iname '*Roslyn*' | head
   ```

   Adjust the path if your Unity Hub install location differs (check Unity Hub's Installs tab, or Unity Hub > Preferences > Locations). The design spec's own prior research names one likely candidate directory, `Unity.app/Contents/DotNetSdkRoslyn` — this command confirms (or corrects) that.

2. Once you have a `csc` path, ask it what language versions it accepts:

   ```
   <path-to-csc> -langversion:?
   ```

   If what `find` turned up is a managed `.dll` (a Roslyn assembly, not a native launcher), invoke it through whichever `dotnet` runtime Unity bundles alongside it, e.g.:

   ```
   dotnet exec <path-to-csc.dll> -langversion:?
   ```

   (find the bundled `dotnet` the same way, with `-iname 'dotnet*'` in place of `-iname 'csc*'`).

   A standard Roslyn `csc -langversion:?` prints something shaped like:

   ```
   Allowed values are default, latest, latestmajor, preview, 1, 2, 3, 4, 5, 6, 7, 7.1, 7.2, 7.3, 8.0, 9.0, 10.0, ...
   ```

   The exact top version listed is precisely what's unverified here — that's the whole point of running this.

3. Record the result: update every `unverified (probe: Stage 0)` cell in the `Compiles via csc.rsp?` column of `docs/feature-matrix.md` with what you actually found, feature by feature (a feature's introducing C# version against the highest version `csc` reported accepting).

**JA:** 何かを変える前に、Unity 6000.7.0a2 が実際に何を同梱しているか、そして同梱の Roslyn が実際にどの言語バージョンまで受け付けるかを確認します。Stage 0 は完全に読み取り専用です —— 何も作成・変更しません —— そして、その結果こそが `docs/feature-matrix.md` の `unverified (probe: Stage 0)` セルを実際の答えに変えるものです（これは project plan の Task 19 が、Unity project が存在するようになった段階で実際に走らせる probe そのものでもあります —— この節はその手順の instructions です）。

1. エディタ install の中から、同梱コンパイラと Roslyn 関連ディレクトリを探します。

   ```
   find /Applications/Unity/Hub/Editor/6000.7.0a2 -maxdepth 5 -iname 'csc*' -o -iname '*Roslyn*' | head
   ```

   手元の Unity Hub の install 先が違う場合はパスを調整してください（Unity Hub の Installs タブ、または Unity Hub > Preferences > Locations で確認できます）。設計書の事前調査は `Unity.app/Contents/DotNetSdkRoslyn` を有力な候補ディレクトリとして挙げています —— このコマンドがそれを確認（または訂正）します。

2. `csc` のパスが手に入ったら、それがどの言語バージョンを受け付けるか尋ねます。

   ```
   <path-to-csc> -langversion:?
   ```

   `find` が見つけたのが managed な `.dll`（native launcher ではなく Roslyn の assembly）である場合は、同じ場所に同梱されている `dotnet` runtime 経由で起動してください。例:

   ```
   dotnet exec <path-to-csc.dll> -langversion:?
   ```

   （同梱の `dotnet` も、`-iname 'csc*'` の代わりに `-iname 'dotnet*'` を使って同じ方法で探せます）。

   標準的な Roslyn の `csc -langversion:?` は、次のような形の出力をします。

   ```
   Allowed values are default, latest, latestmajor, preview, 1, 2, 3, 4, 5, 6, 7, 7.1, 7.2, 7.3, 8.0, 9.0, 10.0, ...
   ```

   ここで一覧の一番上に来る version が、まさに今回未確認の部分です —— それを確かめることがこの手順の目的です。

3. 結果を記録します: `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列にある `unverified (probe: Stage 0)` の各セルを、機能ごと（その機能の導入 C# バージョンと、`csc` が受け付けると報告した最高バージョンの比較）に、実際に判明した内容で更新してください。

> **EN — Result (2026-07-12):** Stage 0 has been run against the installed 6000.7.0a2 editor. The bundled toolchain turned out to be a full .NET SDK 8.0.318 at `Unity.app/Contents/Resources/Scripting/DotNetSdk` (not the `DotNetSdkRoslyn` directory guessed above), with Roslyn 4.10.0 accepting `-langversion` up to 12.0 plus `preview`. Full evidence and the per-feature cell updates live in `docs/feature-matrix.md` under "Stage 0 probe results".
>
> **JA — 実施結果 (2026-07-12):** install 済みの 6000.7.0a2 editor に対して Stage 0 を実行済みです。同梱 toolchain は上で推測した `DotNetSdkRoslyn` ディレクトリではなく、`Unity.app/Contents/Resources/Scripting/DotNetSdk` にある丸ごとの .NET SDK 8.0.318 で、Roslyn 4.10.0 が `-langversion` を 12.0 まで（加えて `preview`）受け付けます。証跡の全文と機能別セルの更新は `docs/feature-matrix.md` の「Stage 0 probe results」を参照してください。

## 3. Stage 1 (non-invasive) — `Assets/Lab/csc.rsp`

**EN:** Stage 1 is non-invasive: it adds exactly one file to the project and does not touch the Unity editor install itself.

1. Create `unity/Assets/Lab/csc.rsp` (the same folder as the Lab's `Sharplings.Lab.asmdef`) containing:

   ```
   -langversion:preview
   ```

2. Since Unity 2020.2.0a19, a `csc.rsp` file placed in the same directory as an `.asmdef` scopes its arguments to that one assembly, rather than the whole project (see the [Unity Discussions thread on per-assembly response files](https://discussions.unity.com/t/rsp-file-per-assembly-or-folder/719041)). That's why this file lives inside `Assets/Lab/` next to `Sharplings.Lab.asmdef` — the elevated language version stays scoped to the Lab and never touches `Assets/Contrasts/` or the rest of the project.

3. Reopen the project (or `Assets > Refresh`) so Unity recompiles with the new `csc.rsp` in effect.

4. Check the Console. Features whose introducing C# version is at or below whatever `-langversion:preview` resolves to on this Roslyn build should now compile inside `Assets/Lab/`. Anything in the `Not supported` or `Crash` rows of `docs/feature-matrix.md` should still fail or misbehave even now — those need runtime/BCL support that a language-version bump alone cannot provide.

5. Record what actually compiled, feature by feature, in the `Compiles via csc.rsp?` column of `docs/feature-matrix.md`.

Note: an IDE (Rider, Visual Studio, VS Code) can still show false errors here even once this compiles cleanly inside Unity, because the generated `.csproj` doesn't automatically inherit a `csc.rsp`'s language version — see section 5, IDE alignment.

**JA:** Stage 1 は非侵襲です。project に追加するのはファイル 1 つだけで、Unity エディタの install 自体には触れません。

1. `unity/Assets/Lab/csc.rsp`（Lab の `Sharplings.Lab.asmdef` と同じフォルダ）を、次の内容で作成します。

   ```
   -langversion:preview
   ```

2. Unity 2020.2.0a19 以降、`.asmdef` と同じディレクトリに置かれた `csc.rsp` ファイルは、project 全体ではなくその 1 つの assembly だけに引数を適用します（[per-assembly の response file に関する Unity Discussions のスレッド](https://discussions.unity.com/t/rsp-file-per-assembly-or-folder/719041)を参照）。だからこそ、このファイルは `Sharplings.Lab.asmdef` の隣、`Assets/Lab/` の中に置きます —— 上げた言語バージョンは Lab の範囲内にとどまり、`Assets/Contrasts/` や project の他部分には一切影響しません。

3. project を開き直す（または `Assets > Refresh`）と、新しい `csc.rsp` が反映された状態で Unity が再コンパイルします。

4. Console を確認してください。導入 C# バージョンが、この Roslyn build 上で `-langversion:preview` が解決する範囲以下の機能は、`Assets/Lab/` の中でコンパイルが通るはずです。`docs/feature-matrix.md` の `Not supported` や `Crash` の行にある機能は、この段階でも失敗するか、誤動作するはずです —— それらが必要とするのは runtime / BCL の対応であり、言語バージョンを上げるだけでは得られません。

5. 実際に何がコンパイルできたかを、機能ごとに `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列へ記録してください。

補足: Unity 内でコンパイルが通るようになっても、IDE（Rider、Visual Studio、VS Code）はまだ誤ったエラーを表示することがあります。生成される `.csproj` は `csc.rsp` の言語バージョンを自動的には継承しないためです —— 5 節（IDE 整合）を参照してください。

> **EN — Result (2026-07-12):** Stage 1 has been run. `unity/Assets/Lab/Sharplings.Lab.asmdef` and `unity/Assets/Lab/csc.rsp` (`-langversion:preview`) were created together with three probe scripts (`ProbeRawStrings.cs`, `ProbeCollectionExpressions.cs`, `ProbeExtensionMembers.cs`), and a headless batchmode compile (`Unity -batchmode -nographics -quit -projectPath unity`) exited 0 with zero `error CS` lines in the log — `Sharplings.Lab.dll` compiled cleanly even with a C# 11 raw string literal and a C# 12 collection expression in its source, proving Unity's build pipeline honors this folder's `csc.rsp` language-version override. Full evidence and the per-feature cell updates live in `docs/feature-matrix.md` under "Stage 1 results".
>
> **JA — 実施結果 (2026-07-12):** Stage 1 を実行済みです。`unity/Assets/Lab/Sharplings.Lab.asmdef` と `unity/Assets/Lab/csc.rsp`（`-langversion:preview`）を、3 つの probe script（`ProbeRawStrings.cs`、`ProbeCollectionExpressions.cs`、`ProbeExtensionMembers.cs`）とともに作成し、headless の batchmode compile（`Unity -batchmode -nographics -quit -projectPath unity`）を実行したところ、exit code 0、log 内の `error CS` はゼロ行でした —— C# 11 の raw string literal と C# 12 の collection expression を source に含んだままでも `Sharplings.Lab.dll` は問題なくコンパイルでき、Unity の build pipeline がこのフォルダの `csc.rsp` の言語バージョン上書きを実際に尊重することが確認できました。証跡の全文と機能別セルの更新は `docs/feature-matrix.md` の「Stage 1 results」を参照してください。

## 4. Stage 2 (invasive) — UnityRoslynUpdater

**EN:** Stage 2 is invasive: [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) replaces the editor's own bundled Roslyn compiler and .NET runtime with symbolic links into a local .NET SDK install, unlocking language versions past whatever Stage 1's `-langversion:preview` tops out at (up to C# 14, per its README's own feature table — the same table `docs/feature-matrix.md` is seeded from). This modifies files **inside the Unity editor's own installation folder**, not the project, and its README states plainly that administrative privileges are required.

### Back up first

Before running anything, make your own independent copy of the directories UnityRoslynUpdater touches — a restore path that doesn't depend on the tool succeeding cleanly. Reading its public source (`UpdateSdkOperation.cs`, checked 2026-07-11) shows it replaces `Data/DotNetSdkRoslyn`, `Data/NetCoreRuntime`, and (if present) `Data/DotNetSdk`, relative to the editor install path, with symlinks. On macOS that is expected to resolve under:

```
/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/DotNetSdkRoslyn
```

(the exact `Data`-equivalent path segment on macOS is one of the things Stage 0's `find` output confirms). Copy that directory — and its `NetCoreRuntime` / `DotNetSdk` siblings, if present — to a location outside the Unity install (e.g. `~/Backups/unity-6000.7.0a2-dotnet-sdk-roslyn/`) before doing anything else.

Worth knowing: the tool also keeps its own internal backup — it moves the original directories into a new `Data/BuiltInDotNetSdk/` folder before creating the symlinks, rather than deleting them. That's a second safety net, not a substitute for your own external copy: an alpha editor, an administrative-privilege operation, and a tool whose macOS behavior is unverified (below) are exactly the conditions where a belt-and-suspenders backup earns its cost.

### Clone, build, run

UnityRoslynUpdater ships prebuilt Windows binaries only (`UnityRoslynUpdater.exe`, via its [GitHub Releases](https://github.com/DaZombieKiller/UnityRoslynUpdater/releases)) — there is no macOS build. To use it on macOS at all, clone and build it yourself:

```
git clone https://github.com/DaZombieKiller/UnityRoslynUpdater.git
cd UnityRoslynUpdater
dotnet build -c Release
```

Then run it in CLI mode, pointing it at the editor's `Editor`-equivalent directory directly (this bypasses the interactive picker, which doesn't work on macOS — see below):

```
dotnet run -c Release --project sources/UnityRoslynUpdater -- "/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents"
```

Whether `dotnet build` even succeeds as-is on macOS, given the project's `net10.0-windows` target framework, is exactly what the next subsection is unverified about — read it before assuming these two commands just work.

### macOS caveat — unverified; here's what reading the source found

Nobody has run Stage 2 on macOS yet as part of this project. Treat everything below as a documented risk assessment, not a confirmed result — and please replace this paragraph with what actually happened once someone has tried it. Reading UnityRoslynUpdater's public source as of 2026-07-11 (`UnityRoslynUpdater.csproj`, `EditorFinder.cs`, `DotNetRoot.cs`, `Program.cs`, `app.manifest`) turned up concrete, Windows-specific dependencies:

- Its `TargetFramework` is `net10.0-windows`, and its own `publish.cmd` publishes only for `-r win-x64`. The officially released binary cannot run on macOS at all; using it there means building from source — territory the maintainer hasn't shipped or, as far as this research found, documented.
- Interactive editor discovery (`EditorFinder.GetRegistryEditorPaths`) reads `SOFTWARE\Unity Technologies\Installer` from the Windows Registry via `Microsoft.Win32.Registry`. There's no macOS equivalent in the source, which is exactly why the CLI invocation above passes the editor path explicitly rather than relying on the interactive picker.
- .NET SDK auto-detection (`DotNetRoot.GetLocation`) checks the `DOTNET_ROOT` environment variable first, which is cross-platform — set it explicitly (to wherever `mise`, or whatever installed this repo's pinned .NET 10 SDK, put `dotnet`) to avoid its Windows-Registry and `Environment.SpecialFolder.ProgramFiles` fallbacks, neither of which resolves correctly off Windows.
- `app.manifest` requests Windows' `requireAdministrator` execution level — a Windows UAC concept with no macOS equivalent. It shouldn't block building for macOS, but it hasn't been confirmed either way.
- Path validation in `Program.cs` requires a `Data` subdirectory under whatever editor path you provide (`UpdateContext.EditorDataPath => Path.Combine(EditorPath, "Data")`), which is how the design spec arrives at `Unity.app/Contents/DotNetSdkRoslyn` as the macOS equivalent — but whether `Unity.app/Contents/Data` actually exists in 6000.7.0a2's bundle layout is exactly what Stage 0's `find` output confirms or refutes. If it doesn't exist, the tool refuses to proceed, with the same error message it shows for any invalid path.

None of this makes Stage 2 impossible on macOS — the actual patching logic (`PatchSourceGeneratorOperation`, `PatchUnityAssembliesOperation`) is built on the cross-platform `AsmResolver.DotNet` library, not on anything Windows-specific. It means the realistic path is: retarget `TargetFramework` to `net10.0` in a local clone, set `DOTNET_ROOT`, and always supply the editor path explicitly — all unverified until someone actually does it.

### Editor updates revert this

Updating the Unity editor — including alpha-to-alpha patch updates — reinstalls its `Data` directory from scratch, overwriting the symlinks Stage 2 created. Any editor update silently reverts you to stock Mono/Roslyn. This is expected, not a bug: treat Stage 2 as something to redo after every editor update, not a one-time setup step.

### Manual restore

To undo Stage 2 (before an editor update does it for you naturally, or because something went wrong):

1. Delete the symlinks the tool created — `Data/DotNetSdkRoslyn`, `Data/NetCoreRuntime`, and `Data/DotNetSdk` (if present) — under the editor install path.
2. If the tool's own internal backup succeeded, move the original directories back out of `Data/BuiltInDotNetSdk/` into their original locations (undoing exactly what `UpdateSdkOperation` did).
3. If that backup is missing or looks wrong, fall back to the independent copy you made before patching (above) and copy it back into place.
4. Reopen the Unity editor and confirm `Assets/Lab` still compiles under Stage 1 (`-langversion:preview` against the restored, stock Roslyn) before assuming everything is back to normal.

### Enabling a language version after patching

Once Stage 2 has linked in a newer .NET SDK's Roslyn, raise the language version per-assembly the same way as Stage 1 — a `csc.rsp` in the same folder as the target `.asmdef`, just with a higher version:

```
-langversion:14
```

This needs `com.unity.ide.visualstudio` 2.0.24 or later (or CsprojLangVersionProcessor) for the version to show up correctly in your IDE — see section 5.

**JA:** Stage 2 は侵襲的です。[UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) は、エディタ同梱の Roslyn コンパイラと .NET runtime を、手元の .NET SDK install へのシンボリックリンクに差し替え、Stage 1 の `-langversion:preview` が頭打ちになる先まで（その README 自身の機能表によれば C# 14 まで —— `docs/feature-matrix.md` の種データもこの表から取っています）言語バージョンを解禁します。これは project ではなく **Unity エディタ自身の install フォルダの内部** を書き換えるものであり、その README は管理者権限が必要だとはっきり書いています。

### まず backup する

何かを実行する前に、UnityRoslynUpdater が触れるディレクトリの独立したコピーを自分で作っておいてください —— ツールがきれいに成功するかどうかに依存しない restore 手段です。公開されている source（`UpdateSdkOperation.cs`、2026-07-11 確認）を読むと、エディタ install パスを基準に `Data/DotNetSdkRoslyn`・`Data/NetCoreRuntime`・（存在すれば）`Data/DotNetSdk` をシンボリックリンクに置き換えることが分かります。macOS では、これは次のパス以下に解決されると見込まれます。

```
/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents/DotNetSdkRoslyn
```

（macOS における `Data` 相当の正確なパス segment は、まさに Stage 0 の `find` の出力が確認するものの 1 つです）。何かを始める前に、そのディレクトリ ——存在すれば `NetCoreRuntime` / `DotNetSdk` の姉妹ディレクトリも —— を、Unity install の外（例: `~/Backups/unity-6000.7.0a2-dotnet-sdk-roslyn/`）へコピーしてください。

知っておく価値があること: このツール自身も内部的な backup を取ります —— シンボリックリンクを作る前に、元のディレクトリを削除するのではなく、新しい `Data/BuiltInDotNetSdk/` フォルダへ移動します。これは第二の安全網であって、自分自身の外部コピーの代わりにはなりません。alpha 版エディタ、管理者権限を要する操作、そして macOS での挙動が未検証のツール（下記）—— この組み合わせこそ、二重の backup が割に合う状況です。

### Clone、build、run

UnityRoslynUpdater が配布しているのは Windows 向けの prebuilt binary のみです（`UnityRoslynUpdater.exe`、[GitHub Releases](https://github.com/DaZombieKiller/UnityRoslynUpdater/releases) から）—— macOS 向け build はありません。macOS でこれを使うには、自分で clone して build する必要があります。

```
git clone https://github.com/DaZombieKiller/UnityRoslynUpdater.git
cd UnityRoslynUpdater
dotnet build -c Release
```

そのうえで、CLI モードでエディタの `Editor` 相当のディレクトリを直接指定して実行します（macOS では動かない対話式の選択肢を経由しません —— 詳細は下記）。

```
dotnet run -c Release --project sources/UnityRoslynUpdater -- "/Applications/Unity/Hub/Editor/6000.7.0a2/Unity.app/Contents"
```

そもそも `dotnet build` が、この project の `net10.0-windows` という target framework のまま macOS で通るかどうかは、次の項でまさに未検証としている点です —— この 2 つのコマンドが素直に動くと決めつける前に、そちらを読んでください。

### macOS の注意点 —— 未検証。source を読んで分かったこと

このプロジェクトの一環として、まだ誰も macOS で Stage 2 を実行していません。以下はすべて、確認済みの結果ではなく文書化されたリスク評価として扱ってください —— 実際に試した人がいれば、この段落を実際の結果に置き換えてください。UnityRoslynUpdater の公開 source を 2026-07-11 時点で読んだところ（`UnityRoslynUpdater.csproj`、`EditorFinder.cs`、`DotNetRoot.cs`、`Program.cs`、`app.manifest`）、具体的な Windows 依存がいくつも見つかりました。

- `TargetFramework` は `net10.0-windows` であり、`publish.cmd` も `-r win-x64` 向けにしか publish しません。公式に配布される binary は macOS ではそもそも動きません。macOS で使うには source から build するしかなく、それは maintainer が出荷しておらず、今回の調査で見つかった範囲では文書化もされていない領域です。
- 対話式のエディタ検出（`EditorFinder.GetRegistryEditorPaths`）は、`Microsoft.Win32.Registry` 経由で Windows レジストリの `SOFTWARE\Unity Technologies\Installer` を読みます。source の中に macOS 相当の経路はありません —— だからこそ、上記の CLI 実行では対話式の選択肢に頼らず、エディタのパスを明示的に渡しています。
- .NET SDK の自動検出（`DotNetRoot.GetLocation`）は、まずクロスプラットフォームな `DOTNET_ROOT` 環境変数を確認します —— これを明示的に設定しておく（`mise`、あるいはこの repo が pin する .NET 10 SDK を install した経路に合わせる）ことで、Windows レジストリと `Environment.SpecialFolder.ProgramFiles` へのフォールバック —— どちらも Windows 以外では正しく解決しません —— を回避できます。
- `app.manifest` は Windows の `requireAdministrator` 実行レベルを要求します —— これは Windows の UAC の概念であり、macOS に相当するものはありません。macOS 向け build を妨げるものではないはずですが、どちらの向きにも確認は取れていません。
- `Program.cs` のパス検証は、指定したエディタパスの下に `Data` サブディレクトリが存在することを要求します（`UpdateContext.EditorDataPath => Path.Combine(EditorPath, "Data")`）。設計書が macOS 相当として `Unity.app/Contents/DotNetSdkRoslyn` に行き着いたのはこのためですが、6000.7.0a2 のバンドル構成に実際に `Unity.app/Contents/Data` が存在するかどうかは、まさに Stage 0 の `find` の出力が確認・反証するところです。存在しなければ、このツールは無効なパスの場合と同じエラーメッセージを出して処理を拒否します。

だからといって Stage 2 が macOS で不可能というわけではありません —— 実際のパッチ処理（`PatchSourceGeneratorOperation`、`PatchUnityAssembliesOperation`）は Windows 固有のものではなく、クロスプラットフォームな `AsmResolver.DotNet` ライブラリの上に構築されています。現実的な道筋は、手元の clone で `TargetFramework` を `net10.0` に retarget し、`DOTNET_ROOT` を設定し、エディタパスを常に明示的に渡すことです —— どれも、実際に誰かが試すまでは未検証です。

### エディタの更新でこれは元に戻る

Unity エディタの更新 —— alpha から alpha への patch 更新も含む —— は `Data` ディレクトリをまるごと再 install し、Stage 2 が作ったシンボリックリンクを上書きします。エディタの更新は、黙って stock の Mono / Roslyn へ戻します。これはバグではなく想定通りの挙動です。Stage 2 は一度きりの setup ではなく、エディタを更新するたびにやり直すものとして扱ってください。

### 手動での restore

Stage 2 を元に戻すには（エディタの更新が自然にそうする前に、あるいは何かが失敗した場合）:

1. ツールが作ったシンボリックリンク —— エディタ install パス配下の `Data/DotNetSdkRoslyn`、`Data/NetCoreRuntime`、（存在すれば）`Data/DotNetSdk` —— を削除します。
2. ツール自身の内部 backup が成功していれば、元のディレクトリを `Data/BuiltInDotNetSdk/` から元の場所へ戻します（`UpdateSdkOperation` が行ったことを正確に逆にする形です）。
3. その backup が無い、あるいはおかしく見える場合は、パッチ前に自分で作った独立したコピー（上記）を使って元の場所へ戻します。
4. Unity エディタを開き直し、`Assets/Lab` が Stage 1 の状態（restore された stock の Roslyn に対する `-langversion:preview`）でまだコンパイルできることを確認してから、元通りだと判断してください。

### パッチ後に言語バージョンを有効化する

Stage 2 で新しい .NET SDK の Roslyn がリンクされたら、Stage 1 と同じ方法で assembly ごとに言語バージョンを上げます —— 対象の `.asmdef` と同じフォルダに置く `csc.rsp` の中身を、より高い version に変えるだけです。

```
-langversion:14
```

これが IDE 上で正しく表示されるには、`com.unity.ide.visualstudio` 2.0.24 以降（または CsprojLangVersionProcessor）が必要です —— 5 節を参照してください。

### Concrete macOS plan for this machine (2026-07-12, planned — not yet executed)

**EN:** The generic UnityRoslynUpdater path above is Windows-only, and Stage 0 showed this 6.7 alpha's layout differs from the tool's expectations anyway. The following manual-swap plan was derived from read-only inspection on 2026-07-12 and is everything a fresh session needs to execute Stage 2 here:

1. Facts established: Unity's toolchain is a full .NET SDK 8.0.318 at `Unity.app/Contents/Resources/Scripting/DotNetSdk` with the compiler at `sdk/8.0.318/Roslyn/bincore/csc.dll` (Roslyn 4.10.0). The local mise SDK at `~/.local/share/mise/installs/dotnet/10.0.301/` ships **Roslyn 5.6.0** (C# 14 stable) at `sdk/10.0.301/Roslyn`. Its `csc.runtimeconfig.json` requires `Microsoft.NETCore.App` **10.0.9** with `rollForward: Major` — it can roll forward, never back, so Unity's bundled 8.0.21 runtime cannot host it.
2. Backup (55 MB): copy `DotNetSdk/sdk/8.0.318/Roslyn` to a safe location outside `Unity.app` before touching anything.
3. Swap: replace the contents of `DotNetSdk/sdk/8.0.318/Roslyn` with the local SDK's `sdk/10.0.301/Roslyn`.
4. Runtime, side-by-side (83 MB, additive — overwrites nothing): copy `~/.local/share/mise/installs/dotnet/10.0.301/shared/Microsoft.NETCore.App/10.0.9` into `DotNetSdk/shared/Microsoft.NETCore.App/`. The dotnet host supports co-installed runtimes and picks per `runtimeconfig`.
5. Offline verification before involving Unity: `DotNetSdk/dotnet DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll -version` should print 5.6.x, and `'-langversion:?'` (quoted — zsh globs `?`) should list 13.0 and 14.0.
6. Activate the Lab's C# 14 probe: add `-define:SHARPLINGS_STAGE2` to `unity/Assets/Lab/csc.rsp`, then run the headless compile (`Unity -batchmode -nographics -quit -projectPath unity -logFile <file>`). Whether Unity's Bee pipeline tolerates the newer compiler is the open experimental question — record whatever happens (success, new diagnostics, or pipeline failure) in `docs/feature-matrix.md`.
7. Restore: delete the swapped `Roslyn` directory and put the backup back; optionally remove the added `10.0.9` runtime directory. An editor update or reinstall also reverts everything silently — re-run step 5 to detect that.
8. Known residual risk: modifying `Unity.app` invalidates the bundle's code signature. Managed DLLs are not subject to macOS library validation and the app is already installed and trusted, so this is expected to be harmless — but if the editor misbehaves, restore first and suspect this second.

**JA:** 上の UnityRoslynUpdater の手順は Windows 専用で、しかも Stage 0 の結果、この 6.7 alpha のレイアウトはツールの想定と異なることが分かっています。以下は 2026-07-12 の read-only 調査から導いた、このマシン向けの手動 swap プランです。新しいセッションが Stage 2 を実行するのに必要な情報はすべてここにあります。

1. 確定済みの事実: Unity の toolchain は `Unity.app/Contents/Resources/Scripting/DotNetSdk` にある丸ごとの .NET SDK 8.0.318 で、コンパイラは `sdk/8.0.318/Roslyn/bincore/csc.dll`（Roslyn 4.10.0）。ローカルの mise SDK（`~/.local/share/mise/installs/dotnet/10.0.301/`）は `sdk/10.0.301/Roslyn` に **Roslyn 5.6.0**（C# 14 stable 対応）を同梱。その `csc.runtimeconfig.json` は `Microsoft.NETCore.App` **10.0.9** を要求し、`rollForward: Major`（上方向のみ、下方向不可）なので、Unity 同梱の 8.0.21 runtime では動かせません。
2. Backup（55 MB）: 何かに触る前に `DotNetSdk/sdk/8.0.318/Roslyn` を `Unity.app` の外の安全な場所へコピーします。
3. Swap: `DotNetSdk/sdk/8.0.318/Roslyn` の中身をローカル SDK の `sdk/10.0.301/Roslyn` で置き換えます。
4. Runtime の side-by-side 追加（83 MB、追加のみで何も上書きしない）: `~/.local/share/mise/installs/dotnet/10.0.301/shared/Microsoft.NETCore.App/10.0.9` を `DotNetSdk/shared/Microsoft.NETCore.App/` へコピーします。dotnet host は runtime の共存を正規サポートし、`runtimeconfig` ごとに選択します。
5. Unity を巻き込む前のオフライン検証: `DotNetSdk/dotnet DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll -version` が 5.6.x を、`'-langversion:?'`（zsh は `?` を glob するので quote 必須）が 13.0 と 14.0 を出力すること。
6. Lab の C# 14 probe を起こす: `unity/Assets/Lab/csc.rsp` に `-define:SHARPLINGS_STAGE2` を追加し、headless compile（`Unity -batchmode -nographics -quit -projectPath unity -logFile <file>`）を実行します。Unity の Bee pipeline が新しいコンパイラを許容するかどうかが、まさに未解決の実験課題です——成功・新しい diagnostics・pipeline の失敗、何が起きてもそのまま `docs/feature-matrix.md` に記録してください。
7. Restore: swap した `Roslyn` ディレクトリを削除して backup を戻します。追加した `10.0.9` runtime ディレクトリの削除は任意。エディタの update / 再インストールでもすべて静かに元へ戻るため、それを検知するには手順 5 を再実行します。
8. 既知の残存リスク: `Unity.app` の変更は bundle の code signature を無効化します。managed DLL は macOS の library validation の対象外で、アプリは install・信頼済みなので実害はない見込みですが、エディタの挙動がおかしくなったらまず restore し、次にこれを疑ってください。

## 5. IDE alignment

**EN:** Even after Stage 1 or Stage 2 makes Unity itself compile newer C# syntax, your IDE can still disagree. Unity generates a `.csproj` per assembly for IDE intellisense, and that generated file hardcodes a `<LangVersion>` independently of whatever `csc.rsp` says — so Rider, Visual Studio, or VS Code can show red squiggles on syntax Unity itself compiles cleanly. Two ways to fix the generated `.csproj`:

**Option A — `com.unity.ide.visualstudio` 2.0.24+**

Starting at version 2.0.24, Unity's own Visual Studio Editor package reflects a `csc.rsp`'s `-langversion` into the generated `.csproj` automatically — this is also the exact version UnityRoslynUpdater's own README calls out as required. Check your installed version under `Window > Package Manager > Visual Studio Editor`. The current published version (checked 2026-07-11 against Unity's package registry) is 2.0.27, well past the floor, and a 6000.7.0a2 default project should already carry a recent-enough version out of the box — confirm this locally rather than assuming it, since alpha project templates can pin unusual versions.

**Option B — CsprojLangVersionProcessor**

[CsprojLangVersionProcessor](https://github.com/annulusgames/CsprojLangVersionProcessor) is a small editor extension purpose-built for this exact mismatch, for projects that would rather not depend on the built-in package's version. Per its README (checked 2026-07-11):

1. Install via Package Manager: `Window > Package Manager > + > Add package from git URL`, then:

   ```
   https://github.com/AnnulusGames/CsprojLangVersionProcessor.git?path=Assets/CsprojLangVersionProcessor
   ```

   or add it directly to `Packages/manifest.json`:

   ```json
   {
     "dependencies": {
       "com.annulusgames.csproj-langversion-processor": "https://github.com/AnnulusGames/CsprojLangVersionProcessor.git?path=Assets/CsprojLangVersionProcessor"
     }
   }
   ```

   (Requires Unity 2022.2 or later — 6000.7.0a2 clears this easily.)

2. Set `-langVersion:preview` (or your target version) under `Project Settings > Player > Other Settings > Script Compilation > Additional Compiler Arguments` — the project-wide equivalent of what `Assets/Lab/csc.rsp` does locally.
3. Set the desired `LangVersion` for the IDE under `Project Settings > Editor > Csproj LangVersion Processor`.
4. Regenerate project files if needed: `Edit > Preferences > External Tools > Regenerate project files`.

Either option resolves the same problem; which one you use doesn't affect anything recorded in `docs/feature-matrix.md` — pick whichever fits your workflow.

**JA:** Stage 1 や Stage 2 で Unity 自身が新しい C# 構文をコンパイルできるようになっても、IDE 側の認識が食い違うことがあります。Unity は IDE の intellisense 用に assembly ごとに `.csproj` を生成しますが、その生成ファイルは `csc.rsp` の内容とは無関係に `<LangVersion>` を固定値で書き込みます。そのため、Unity 自身は問題なくコンパイルできる構文に対して、Rider・Visual Studio・VS Code が赤い波線を表示することがあります。生成された `.csproj` を直す方法は 2 つあります。

**選択肢 A —— `com.unity.ide.visualstudio` 2.0.24 以降**

version 2.0.24 以降、Unity 自身の Visual Studio Editor package は `csc.rsp` の `-langversion` を生成される `.csproj` へ自動的に反映します —— これは UnityRoslynUpdater 自身の README が要求 version として名指ししているものと同じです。install 済みの version は `Window > Package Manager > Visual Studio Editor` で確認できます。現在公開されている version（2026-07-11 に Unity の package registry で確認）は 2.0.27 で、下限を十分に上回っています。6000.7.0a2 の既定 project にも、最初から十分新しい version が入っているはずですが、alpha の project テンプレートは変わった version を pin することがあるため、思い込まずローカルで確認してください。

**選択肢 B —— CsprojLangVersionProcessor**

[CsprojLangVersionProcessor](https://github.com/annulusgames/CsprojLangVersionProcessor) は、まさにこのずれのために作られた小さな editor 拡張です。組み込み package の version に依存したくない project 向けです。その README（2026-07-11 確認）によれば:

1. Package Manager から install します: `Window > Package Manager > + > Add package from git URL` に続けて:

   ```
   https://github.com/AnnulusGames/CsprojLangVersionProcessor.git?path=Assets/CsprojLangVersionProcessor
   ```

   または `Packages/manifest.json` に直接追加します:

   ```json
   {
     "dependencies": {
       "com.annulusgames.csproj-langversion-processor": "https://github.com/AnnulusGames/CsprojLangVersionProcessor.git?path=Assets/CsprojLangVersionProcessor"
     }
   }
   ```

   （Unity 2022.2 以降が必要です —— 6000.7.0a2 は楽にクリアします。）

2. `Project Settings > Player > Other Settings > Script Compilation > Additional Compiler Arguments` の下に `-langVersion:preview`（または目的の version）を設定します —— `Assets/Lab/csc.rsp` がローカルで行うことの project 全体版です。
3. `Project Settings > Editor > Csproj LangVersion Processor` で、IDE 向けに使いたい `LangVersion` を設定します。
4. 必要なら project ファイルを再生成します: `Edit > Preferences > External Tools > Regenerate project files`。

どちらの選択肢も同じ問題を解決します。どちらを使うかは `docs/feature-matrix.md` に記録する内容には影響しません —— 自分のワークフローに合う方を選んでください。

## 6. PolySharp installation

**EN:** [PolySharp](https://github.com/Sergio0694/PolySharp) is a source generator that provides compile-time-only polyfills — it generates the "magic" attribute and BCL types the C# compiler needs to see in order to allow certain newer-C# syntax on an older target, without needing any actual runtime support. That is exactly the `Needs PolySharp` rows in `docs/feature-matrix.md` (required members, interpolated string handlers, `CallerArgumentExpression`): all three are attribute-driven rather than runtime-driven, which is why PolySharp can make them work on Mono. It cannot help the `Not supported` or `Crash` rows — those need actual runtime/BCL capability, which no source generator can fake.

PolySharp's own README (checked 2026-07-11) doesn't document a Unity-specific install path — it ships purely as a NuGet package (a plain `<PackageReference Include="PolySharp" />` in a normal .NET project). Two ways to get it into a Unity project instead:

**Option A — NuGetForUnity**

Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) itself first (via OpenUPM, a git-URL package dependency, or its `.unitypackage` release — see its README for current instructions for each), then use its Package Manager-style window to search for and install the `PolySharp` package. Verify locally that this actually wires up source generation correctly — NuGetForUnity's own docs don't specifically document automatic `RoslynAnalyzer` labeling for every package shape, so treat this as convenient but unconfirmed until the smoke test below passes.

**Option B — manual DLL install (matches Unity's own documented procedure)**

This is the same procedure Unity's manual describes for installing any third-party analyzer or source generator ([Install and use an existing analyzer or source generator](https://docs.unity3d.com/Manual/install-existing-analyzer.html)):

1. Download the PolySharp package from [nuget.org](https://www.nuget.org/packages/PolySharp) (the "Download package" button saves a `.nupkg` file, which is a `.zip` in disguise).
2. Extract it and locate the analyzer DLL at `analyzers/dotnet/cs/PolySharp.SourceGenerators.dll` (verified by inspecting the 1.16.0 package's contents on 2026-07-11 — PolySharp ships exactly this one analyzer DLL, unlike some analyzer packages that ship several).
3. Copy that DLL into `Assets/Lab/` (or a nested `Assets/Lab/Plugins/PolySharp/` folder).
4. Select it in Unity's Project window to open the Plugin Inspector:
   - Under **Select platforms for plugin**, disable **Any Platform**.
   - Under **Include Platforms**, disable **Editor** and **Standalone**.
   - Under **Asset Labels**, add a new label named exactly `RoslynAnalyzer` (case-sensitive). Unity recognizes this label and treats the DLL as a Roslyn analyzer/source generator scoped to whichever assembly definition contains it — here, `Sharplings.Lab`.
5. Raise the effective `LangVersion` high enough to reach the polyfilled features (Stage 1/2, plus section 5's IDE alignment).

**Smoke test**

Add a small `required`-member type inside `Assets/Lab` and confirm it compiles without a missing-attribute error (a `CS0246`-class error mentioning `RequiredMemberAttribute` or similar means PolySharp isn't wired up yet). Record the result in the `Required members` row's `Compiles via csc.rsp?` / `Mono editor/player` cells in `docs/feature-matrix.md`.

**JA:** [PolySharp](https://github.com/Sergio0694/PolySharp) は、コンパイル時だけの polyfill を提供する source generator です —— 実際の runtime サポートを必要とせず、より新しい C# 構文を古い target 上で許可するために C# コンパイラが必要とする「魔法の」attribute や BCL の型を生成します。これはまさに `docs/feature-matrix.md` の `Needs PolySharp` の行（required members、interpolated string handler、`CallerArgumentExpression`）に当たります。この 3 つはいずれも runtime 依存ではなく attribute 依存であり、だからこそ PolySharp が Mono 上でも動かせます。`Not supported` や `Crash` の行には効果がありません —— それらが必要とするのは実際の runtime / BCL の能力であり、source generator では偽装できません。

PolySharp 自身の README（2026-07-11 確認）は、Unity 固有の install 経路を文書化していません —— 純粋な NuGet package として配布されているだけです（通常の .NET project であれば、ただの `<PackageReference Include="PolySharp" />` です）。代わりに、Unity project へ組み込む方法は 2 つあります。

**選択肢 A —— NuGetForUnity**

まず [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) 自体を install します（OpenUPM、git URL package 依存、または `.unitypackage` release 経由 —— それぞれの現在の手順は README を参照してください）。そのうえで、Package Manager 風の window から `PolySharp` package を検索して install します。これが source generation を正しく組み込むかどうかは、ローカルで確認してください —— NuGetForUnity 自身の docs は、すべての package 形状に対して `RoslynAnalyzer` label を自動付与するとは明記していないため、便利ではあっても、下記の smoke test が通るまでは未確認として扱ってください。

**選択肢 B —— 手動での DLL install（Unity 自身が文書化している手順と同じ）**

これは、Unity の manual がサードパーティの analyzer や source generator を install する方法として説明している手順と同じです（[Install and use an existing analyzer or source generator](https://docs.unity3d.com/Manual/install-existing-analyzer.html)）。

1. [nuget.org](https://www.nuget.org/packages/PolySharp) から PolySharp package を download します（「Download package」ボタンで `.nupkg` ファイルが保存されます。これは実体が `.zip` です）。
2. 展開して、analyzer の DLL を `analyzers/dotnet/cs/PolySharp.SourceGenerators.dll` に見つけます（2026-07-11 に 1.16.0 package の中身を確認済み —— PolySharp はこの analyzer DLL を 1 つだけ同梱しており、複数同梱する package もある中でシンプルです）。
3. その DLL を `Assets/Lab/`（または入れ子の `Assets/Lab/Plugins/PolySharp/` フォルダ）へコピーします。
4. Unity の Project window でそれを選択し、Plugin Inspector を開きます。
   - **Select platforms for plugin** の下で **Any Platform** を無効化します。
   - **Include Platforms** の下で **Editor** と **Standalone** を無効化します。
   - **Asset Labels** の下で、`RoslynAnalyzer`（大文字小文字を区別）という名前の label を新規作成して追加します。Unity はこの label を認識し、その DLL を、それが含まれる assembly definition —— ここでは `Sharplings.Lab` —— に対する Roslyn analyzer / source generator として扱います。
5. polyfill された機能に届くところまで、実効的な `LangVersion` を上げます（Stage 1/2、および 5 節の IDE 整合）。

**Smoke test**

`Assets/Lab` の中に小さな `required` member 付きの型を追加し、attribute 不足のエラーなしにコンパイルが通ることを確認してください（`RequiredMemberAttribute` などに言及する `CS0246` 系のエラーが出る場合、PolySharp がまだ正しく組み込まれていません）。結果は `docs/feature-matrix.md` の `Required members` 行の `Compiles via csc.rsp?` / `Mono editor/player` セルへ記録してください。
