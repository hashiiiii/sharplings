using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: inline arrays (C# 12), guarded by SHARPLINGS_INLINE_ARRAYS.
    //
    // Runtime-dependent feature. Two things must line up for it to work: (1) the
    // compiler must see System.Runtime.CompilerServices.InlineArrayAttribute, and
    // (2) the runtime must honor the inline-array memory layout. PolySharp is now
    // active on this assembly (a RoslynAnalyzer scoped to Sharplings.Lab), so it
    // MAY source-generate the attribute in (1) -- which means, unlike the pre-
    // PolySharp assumption, this may actually COMPILE. Whether it compiles, and if
    // so whether the Mono editor runtime yields correct values (2), is what this
    // probe measures. Record the empirical outcome; do not assume a compile error.
    public class ProbeInlineArrays : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_INLINE_ARRAYS
            Buffer4 buffer = default;
            buffer[0] = 3;
            buffer[1] = 4;
            buffer[2] = 5;
            buffer[3] = 6;
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
    internal struct Buffer4
    {
        private int _element;
    }
#endif
}
