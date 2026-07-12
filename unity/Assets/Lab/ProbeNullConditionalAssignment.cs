using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: null-conditional assignment (C# 14).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 2 -- C# 14 syntax, so the code below is wrapped in
    //     the same `#if SHARPLINGS_STAGE2` guard as
    //     `ProbeExtensionMembers.cs` (that file's header explains the
    //     guard in full). Record the outcome (compiled or not, and the
    //     exact error if not) in the `Compiles via csc.rsp?` column of
    //     `docs/feature-matrix.md`, "Null-conditional assignment (C# 14)"
    //     row.
    // JA: 必要なのは Stage 2 -- C# 14 構文のため、下のコードは
    //     `ProbeExtensionMembers.cs` と同じ `#if SHARPLINGS_STAGE2` guard
    //     で包んでいます（guard の詳しい理屈はそのファイルのヘッダ参照）。
    //     結果（コンパイルできたか、できなかった場合は正確なエラー内容）は
    //     `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「Null-conditional assignment (C# 14)」行に記録してください。
    public class ProbeNullConditionalAssignment : MonoBehaviour
    {
#if SHARPLINGS_STAGE2
        private sealed class Enemy
        {
            public string Name = "grunt";
        }
#endif

        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STAGE2
            Enemy present = new Enemy();
            Enemy absent = null;

            // C# 14: `?.` on the LEFT side of an assignment -- writes only
            // when the receiver is non-null, so the second line is a no-op
            // instead of a NullReferenceException.
            present?.Name = "Boss";
            absent?.Name = "Boss";

            Debug.Log($"PASS: null-conditional assignment (C# 14) compiled. present.Name = '{present.Name}', absent stayed null: {absent == null}.");
#else
            Debug.Log("SKIP: null-conditional assignment (C# 14) probe needs Stage 2 (SHARPLINGS_STAGE2 is not defined).");
#endif
        }
    }
}
