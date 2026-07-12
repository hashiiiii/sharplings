using System;
using UnityEngine;

namespace Sharplings.Lab
{
    // Lab probe: first-class span conversions (C# 14).
    // See README.md in this folder for the two-stage workflow this probe
    // belongs to.
    //
    // EN: Needs Stage 2 -- C# 14 semantics, so the code below is wrapped
    //     in the same `#if SHARPLINGS_STAGE2` guard as
    //     `ProbeExtensionMembers.cs` (that file's header explains the
    //     guard in full). Record the outcome (compiled or not, and the
    //     exact error if not) in the `Compiles via csc.rsp?` column of
    //     `docs/feature-matrix.md`, "First-class span conversions (C#
    //     14)" row.
    // JA: 必要なのは Stage 2 -- C# 14 のセマンティクスのため、下のコードは
    //     `ProbeExtensionMembers.cs` と同じ `#if SHARPLINGS_STAGE2` guard
    //     で包んでいます（guard の詳しい理屈はそのファイルのヘッダ参照）。
    //     結果（コンパイルできたか、できなかった場合は正確なエラー内容）は
    //     `docs/feature-matrix.md` の `Compiles via csc.rsp?` 列、
    //     「First-class span conversions (C# 14)」行に記録してください。
    public class ProbeFirstClassSpans : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STAGE2
            int[] numbers = { 2, 4, 6 };

            // C# 14: the int[] converts to the extension method's
            // ReadOnlySpan<int> receiver directly -- before C# 14 this
            // required an explicit `numbers.AsSpan()` step.
            int total = numbers.SumSpan();

            Debug.Log($"PASS: first-class span conversions (C# 14) compiled. numbers.SumSpan() = {total}.");
#else
            Debug.Log("SKIP: first-class span conversions (C# 14) probe needs Stage 2 (SHARPLINGS_STAGE2 is not defined).");
#endif
        }
    }

#if SHARPLINGS_STAGE2
    // The span-shaped extension method the probe calls on a plain array.
    internal static class SpanExtensions
    {
        public static int SumSpan(this ReadOnlySpan<int> numbers)
        {
            int total = 0;
            foreach (int number in numbers)
            {
                total += number;
            }

            return total;
        }
    }
#endif
}
