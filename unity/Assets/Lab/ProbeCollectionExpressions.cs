using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: collection expressions (C# 12).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 1 -- `Assets/Lab/csc.rsp` (`-langversion:preview`)
    //     raising the bundled Roslyn's per-assembly language version.
    //     Collection expressions shipped in C# 12, at Stage 0's measured
    //     ceiling (bundled Roslyn 4.10.0 accepts up to 12.0 plus
    //     `preview`), so this file is expected to compile today, without
    //     any Stage 2 Roslyn swap. Record the outcome (compiled or not,
    //     and the exact error if not) in the `Compiles via csc.rsp?`
    //     column of `docs/feature-matrix.md`, "Collection expressions (C#
    //     12)" row.
    // JA: 必要なのは Stage 1 -- `Assets/Lab/csc.rsp`（`-langversion:preview`）
    //     が、同梱 Roslyn の言語バージョンを assembly 単位で引き上げます。
    //     collection expression は C# 12 で導入されており、Stage 0 で確認
    //     した上限（同梱 Roslyn 4.10.0 が `-langversion` を 12.0 まで、
    //     加えて `preview` を受け付ける）にちょうど収まるため、Stage 2 の
    //     Roslyn 差し替えなしに、今日の時点でコンパイルが通ると見込まれ
    //     ます。結果（コンパイルできたか、できなかった場合は正確なエラー
    //     内容）は `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「Collection expressions (C# 12)」行に記録してください。
    public class ProbeCollectionExpressions : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            // A collection expression (C# 12): `[...]` targets the
            // int[] below without `new int[] { ... }`.
            int[] values = [1, 2, 3];

            Debug.Log($"PASS: collection expressions (C# 12) compiled. values.Length = {values.Length}");
        }
    }
}
