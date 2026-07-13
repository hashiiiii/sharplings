using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: inline arrays (C# 12), guarded by SHARPLINGS_INLINE_ARRAYS.
    //
    // Runtime-dependent feature. Mono editor result (Stage 2 Roslyn 5.6.0,
    // 2026-07-13): does NOT compile -- CS0234, because
    // System.Runtime.CompilerServices.InlineArrayAttribute is absent from Mono's
    // .NET Standard 2.1 BCL, and PolySharp (active on this assembly) does not
    // polyfill it. It is expected to compile and run on the CoreCLR Desktop Player
    // and Unity 6.8 (a real .NET runtime with the attribute and the inline-array
    // layout) -- the open case this guarded probe exists to exercise.
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
