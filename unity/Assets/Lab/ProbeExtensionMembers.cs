using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: extension members (C# 14).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 2 -- a UnityRoslynUpdater swap of the editor's
    //     bundled Roslyn compiler (see `docs/unity-lab-setup.md`, section
    //     4). Stage 0 found the bundled Roslyn 4.10.0 tops out at
    //     `-langversion` 12.0 plus `preview`; extension members are C#
    //     14 syntax, which that compiler cannot parse at all -- not even
    //     inside a disabled `#if` branch, if the branch were active. That
    //     is why the extension block below, and the one line that uses
    //     it, are guarded by `#if SHARPLINGS_STAGE2`: a preprocessor
    //     branch that resolves false is only *lexed*, never *parsed*, so
    //     C# 14 syntax inside it cannot break compilation of this file,
    //     or this assembly, today. `SHARPLINGS_STAGE2` is not defined
    //     anywhere yet -- it is meant to be added to this folder's
    //     `csc.rsp` (an extra `-define:SHARPLINGS_STAGE2` line) only
    //     after Stage 2 actually swaps in a Roslyn build that
    //     understands C# 14. Until then, `Probe()` below takes the
    //     `#else` branch and logs that the probe was skipped. Record the
    //     outcome (compiled or not, and the exact error if not) in the
    //     `Compiles via csc.rsp?` column of `docs/feature-matrix.md`,
    //     "Extension members (C# 14)" row.
    // JA: 必要なのは Stage 2 -- UnityRoslynUpdater によるエディタ同梱
    //     Roslyn コンパイラの差し替えです（`docs/unity-lab-setup.md` の
    //     4 節を参照）。Stage 0 で判明した通り、同梱 Roslyn 4.10.0 は
    //     `-langversion` を 12.0（加えて `preview`）までしか受け付けず、
    //     extension member は C# 14 構文であるため、そのコンパイラは
    //     そもそも構文解析すらできません -- たとえ `#if` の無効な分岐が
    //     有効化されたとしてもです。だからこそ、下の extension block と
    //     それを使う 1 行は `#if SHARPLINGS_STAGE2` で guard されて
    //     います。プリプロセッサの分岐が false に解決される場合、その
    //     中身は *lex（字句解析）* されるだけで *parse（構文解析）*
    //     されないため、内部の C# 14 構文が今日このファイル、この
    //     assembly のコンパイルを壊すことはありません。`SHARPLINGS_STAGE2`
    //     はまだどこにも定義されていません -- これは、Stage 2 が実際に
    //     C# 14 を理解する Roslyn build を差し替えたあとで初めて、この
    //     フォルダの `csc.rsp` に（`-define:SHARPLINGS_STAGE2` という
    //     追加の行として）加えることを想定しています。それまでは、下の
    //     `Probe()` は `#else` 分岐を通り、probe を skip した旨を
    //     ログ出力します。結果（コンパイルできたか、できなかった場合は
    //     正確なエラー内容）は `docs/feature-matrix.md` の
    //     `Compiles via csc.rsp?` 列、「Extension members (C# 14)」行に
    //     記録してください。
    public class ProbeExtensionMembers : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STAGE2
            const int value = 21;
            Debug.Log($"PASS: extension members (C# 14) compiled. value.Doubled = {value.Doubled}");
#else
            Debug.Log("SKIP: extension members (C# 14) probe needs Stage 2 (SHARPLINGS_STAGE2 is not defined).");
#endif
        }
    }

#if SHARPLINGS_STAGE2
    // The extension block itself: C# 14 syntax, guarded for the reason
    // explained in the header comment above.
    internal static class IntExtensions
    {
        extension(int value)
        {
            public int Doubled => value * 2;
        }
    }
#endif
}
