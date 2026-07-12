using System.Collections.Generic;
using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: `nameof` with unbound generic types (C# 14).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 2 -- C# 14 syntax, so the code below is wrapped in
    //     the same `#if SHARPLINGS_STAGE2` guard as
    //     `ProbeExtensionMembers.cs` (that file's header explains the
    //     guard in full). Record the outcome (compiled or not, and the
    //     exact error if not) in the `Compiles via csc.rsp?` column of
    //     `docs/feature-matrix.md`, "`nameof` improvements -- unbound
    //     generic types (C# 14)" row.
    // JA: 必要なのは Stage 2 -- C# 14 構文のため、下のコードは
    //     `ProbeExtensionMembers.cs` と同じ `#if SHARPLINGS_STAGE2` guard
    //     で包んでいます（guard の詳しい理屈はそのファイルのヘッダ参照）。
    //     結果（コンパイルできたか、できなかった場合は正確なエラー内容）は
    //     `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「`nameof` improvements -- unbound generic types (C# 14)」行に
    //     記録してください。
    public class ProbeNameofUnboundGeneric : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STAGE2
            // C# 14: `nameof` accepts an unbound generic type -- no type
            // argument needed just to name the type.
            Debug.Log($"PASS: nameof unbound generics (C# 14) compiled. nameof(List<>) = '{nameof(List<>)}', nameof(Dictionary<,>) = '{nameof(Dictionary<,>)}'.");
#else
            Debug.Log("SKIP: nameof unbound generics (C# 14) probe needs Stage 2 (SHARPLINGS_STAGE2 is not defined).");
#endif
        }
    }
}
