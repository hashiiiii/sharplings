# Sharplings.Lab

**EN:** `Assets/Lab/` is an isolated experiment zone for pushing Unity 6000.7.0a2 past its official C# 9 ceiling, ahead of Unity 6.8's official C# 14 support. It lives in its own assembly (`Sharplings.Lab.asmdef`, `autoReferenced: false`), so a compiler crash or a bad experiment here cannot break `Assets/Contrasts/` or the rest of the project. Full background, risk disclaimer, and step-by-step procedures: [`../../../docs/unity-lab-setup.md`](../../../docs/unity-lab-setup.md).

**JA:** `Assets/Lab/` は、Unity 6.8 の公式な C# 14 対応を待たずに、Unity 6000.7.0a2 の公式な C# 9 の壁を超えるための、独立した実験ゾーンです。専用の assembly（`Sharplings.Lab.asmdef`、`autoReferenced: false`）に置かれているため、ここでコンパイラがクラッシュしても、実験が失敗しても、`Assets/Contrasts/` や project の他部分は壊れません。背景・リスクの注意書き・手順の詳細は [`../../../docs/unity-lab-setup.md`](../../../docs/unity-lab-setup.md) を参照してください。

## The two-stage workflow

**EN:** Two unofficial tricks unlock newer C# syntax in this same editor install, at increasing levels of risk:

- **Stage 1** (non-invasive, this folder) -- `csc.rsp` (`-langversion:preview`) raises the language version Unity's own bundled Roslyn accepts, scoped to this one assembly, without touching the editor install. This is what `ProbeRawStrings.cs` and `ProbeCollectionExpressions.cs` probe.
- **Stage 2** (invasive, editor-wide) -- [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) replaces the editor's bundled Roslyn compiler and .NET runtime outright, unlocking C# syntax all the way to 14. This is what `ProbeExtensionMembers.cs` needs, and it is guarded accordingly -- see below.

Run Stage 1 first: reopen the project (or `Assets > Refresh`), right-click a probe component in the Inspector, and choose "Probe" from the context menu (or its gear icon), then read the Console for the `PASS`/`SKIP` line. Stage 2 is optional and separate; follow `docs/unity-lab-setup.md` section 4 if you choose to attempt it.

**JA:** 同じエディタ install の中でより新しい C# 構文を解禁する非公式の裏技が 2 つあり、リスクは段階的に上がります。

- **Stage 1**（非侵襲、このフォルダ）—— `csc.rsp`（`-langversion:preview`）が、Unity 同梱の Roslyn が受け付ける言語バージョンを、この assembly 1 つに限定して引き上げます。エディタ install には触れません。`ProbeRawStrings.cs` と `ProbeCollectionExpressions.cs` が probe するのはこれです。
- **Stage 2**（侵襲、エディタ全体）—— [UnityRoslynUpdater](https://github.com/DaZombieKiller/UnityRoslynUpdater) が、エディタ同梱の Roslyn コンパイラと .NET runtime をまるごと差し替え、C# 14 構文まで解禁します。`ProbeExtensionMembers.cs` が必要とするのはこれで、そのために下記の通り guard されています。

まず Stage 1 を試してください。project を開き直す（または `Assets > Refresh`）、Inspector で probe component を右クリックし、コンテキストメニュー（または歯車アイコン）から「Probe」を選ぶと、Console に `PASS`/`SKIP` の行が出ます。Stage 2 は任意かつ別個の手順です。試す場合は `docs/unity-lab-setup.md` の 4 節に従ってください。

## The lab-notebook contract

**EN:** `docs/feature-matrix.md` is the lab notebook for this whole exploration. Every probe run in this folder is expected to end with an update there, not just a Console log read once and forgotten: after running a probe, record what actually happened -- compiled or not, and the exact error if not -- in the matrix's `Compiles via csc.rsp?` column, in the row for that feature. This README and the probe scripts point at the matrix; they do not duplicate its contents.

**JA:** `docs/feature-matrix.md` は、この探求全体のための lab notebook です。このフォルダで probe を実行したら、その結果を必ずそこへ記録してください -- Console のログを一度読んで終わりにするのではなく、実際に何が起きたか（コンパイルできたか、できなかった場合は正確なエラー内容）を、matrix の `Compiles via csc.rsp?` 列、対応する機能の行に書き込みます。この README と probe script は matrix を指し示すものであり、その内容を重複して持つものではありません。

## The `SHARPLINGS_STAGE2` guard

**EN:** `ProbeExtensionMembers.cs` targets C# 14 (extension members), which the bundled Roslyn 4.10.0 cannot parse at all -- not C# 12/13-ish-but-rejected, but genuinely unparseable syntax. To keep this file compiling at Stage 1 anyway, its extension block and the code that calls it are wrapped in `#if SHARPLINGS_STAGE2 ... #endif`. A preprocessor branch that resolves false is only *lexed* by the compiler, never *parsed* -- so C# 14 syntax sitting inside a disabled branch is safe, even on a compiler that would reject that same syntax if it were active. `SHARPLINGS_STAGE2` has been live in this folder's `csc.rsp` since 2026-07-12, when the Stage 2 swap landed Roslyn 5.6.0 (see `docs/feature-matrix.md`, "Stage 2 results"). The sequence that got here -- and to repeat after any editor update reverts the swap -- is:

1. Complete a Stage 2 Roslyn swap (`docs/unity-lab-setup.md`, section 4).
2. Add `-define:SHARPLINGS_STAGE2` as a second line to this folder's `csc.rsp`.
3. Reopen the project; `ProbeExtensionMembers.cs`'s guarded branch becomes live, and the probe actually exercises C# 14 syntax instead of logging `SKIP`.

If an editor update silently restores the stock Roslyn (it does -- see `docs/unity-lab-setup.md`, section 4), remove the `-define:SHARPLINGS_STAGE2` line until the swap is redone -- keeping the define active against a compiler that still cannot parse the guarded branch reintroduces the exact failure the guard exists to prevent.

**JA:** `ProbeExtensionMembers.cs` は C# 14（extension member）を対象としており、同梱の Roslyn 4.10.0 はこれをそもそも構文解析できません -- 「C# 12/13 相当だが拒否される」のではなく、本当に parse 不能な構文です。それでもこのファイルを Stage 1 の段階でコンパイルできるようにするため、extension block とそれを呼び出すコードは `#if SHARPLINGS_STAGE2 ... #endif` で包んでいます。プリプロセッサの分岐が false に解決される場合、コンパイラはその中身を *lex（字句解析）* するだけで *parse（構文解析）* しません -- そのため、無効な分岐の中にある C# 14 構文は、それが有効化されたら拒否するはずのコンパイラの上でも安全です。`SHARPLINGS_STAGE2` は 2026-07-12、Stage 2 の swap で Roslyn 5.6.0 が入った時点から、このフォルダの `csc.rsp` で有効になっています（`docs/feature-matrix.md` の「Stage 2 results」を参照）。ここへ至った手順 -- そしてエディタ更新が swap を元に戻したあとにやり直す手順 -- は次の通りです。

1. Stage 2 の Roslyn 差し替えを完了させます（`docs/unity-lab-setup.md` の 4 節）。
2. このフォルダの `csc.rsp` に、2 行目として `-define:SHARPLINGS_STAGE2` を追加します。
3. project を開き直すと、`ProbeExtensionMembers.cs` の guard された分岐が有効になり、probe は `SKIP` をログ出力する代わりに、実際に C# 14 構文を検証します。

エディタの更新は stock の Roslyn を黙って復元します（`docs/unity-lab-setup.md` の 4 節を参照）。その場合は、swap をやり直すまで `-define:SHARPLINGS_STAGE2` の行を削除してください -- guard された分岐を、まだそれを構文解析できないコンパイラに対して有効なままにしておくと、その guard が防ぐはずだった失敗をそのまま再現することになります。
