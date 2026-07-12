using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: the `field` keyword (C# 14).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 2 -- C# 14 syntax, so the code below is wrapped in
    //     the same `#if SHARPLINGS_STAGE2` guard as
    //     `ProbeExtensionMembers.cs` (that file's header explains the
    //     guard in full). Record the outcome (compiled or not, and the
    //     exact error if not) in the `Compiles via csc.rsp?` column of
    //     `docs/feature-matrix.md`, "`field` keyword (C# 14)" row.
    // JA: 必要なのは Stage 2 -- C# 14 構文のため、下のコードは
    //     `ProbeExtensionMembers.cs` と同じ `#if SHARPLINGS_STAGE2` guard
    //     で包んでいます（guard の詳しい理屈はそのファイルのヘッダ参照）。
    //     結果（コンパイルできたか、できなかった場合は正確なエラー内容）は
    //     `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「`field` keyword (C# 14)」行に記録してください。
    public class ProbeFieldKeyword : MonoBehaviour
    {
#if SHARPLINGS_STAGE2
        // C# 14: `field` inside the accessors is the compiler-provided
        // backing field -- no separate field declaration needed.
        private string Label
        {
            get => field ?? "unset";
            set => field = value;
        }
#endif

        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STAGE2
            string before = Label;
            Label = "labeled";
            Debug.Log($"PASS: field keyword (C# 14) compiled. Label went '{before}' -> '{Label}'.");
#else
            Debug.Log("SKIP: field keyword (C# 14) probe needs Stage 2 (SHARPLINGS_STAGE2 is not defined).");
#endif
        }
    }
}
