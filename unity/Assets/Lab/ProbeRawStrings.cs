using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: raw string literals (C# 11).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 1 -- `Assets/Lab/csc.rsp` (`-langversion:preview`)
    //     raising the bundled Roslyn's per-assembly language version. Raw
    //     string literals shipped in C# 11, well below Stage 0's measured
    //     ceiling (bundled Roslyn 4.10.0 accepts up to 12.0 plus
    //     `preview`), so this file is expected to compile today, without
    //     any Stage 2 Roslyn swap. Record the outcome (compiled or not,
    //     and the exact error if not) in the `Compiles via csc.rsp?`
    //     column of `docs/feature-matrix.md`, "Raw string literals (C#
    //     11)" row.
    // JA: 必要なのは Stage 1 -- `Assets/Lab/csc.rsp`（`-langversion:preview`）
    //     が、同梱 Roslyn の言語バージョンを assembly 単位で引き上げます。
    //     raw string literal は C# 11 で導入されており、Stage 0 で確認した
    //     上限（同梱 Roslyn 4.10.0 が `-langversion` を 12.0 まで、加えて
    //     `preview` を受け付ける）を十分に下回るため、Stage 2 の Roslyn
    //     差し替えなしに、今日の時点でコンパイルが通ると見込まれます。
    //     結果（コンパイルできたか、できなかった場合は正確なエラー内容）は
    //     `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「Raw string literals (C# 11)」行に記録してください。
    public class ProbeRawStrings : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            // A raw string literal (C# 11): no escaping needed for the
            // embedded quotes below, unlike a regular string literal.
            const string message = """
                PASS: raw string literals (C# 11) compiled.
                Embedded "quotes" need no escaping here.
                """;

            Debug.Log(message);
        }
    }
}
