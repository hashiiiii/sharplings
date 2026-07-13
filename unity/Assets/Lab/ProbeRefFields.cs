using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: ref fields (C# 11), guarded by SHARPLINGS_REF_FIELDS.
    //
    // A ref field in a ref struct (`public ref int Value;`) is a pure
    // language + runtime feature -- no attribute PolySharp can supply makes it
    // work. Mono editor result (Stage 2 Roslyn 5.6.0, 2026-07-13): does NOT
    // compile -- CS9064, "Target runtime doesn't support ref fields"; the compiler
    // itself refuses. Expected to compile and run on the CoreCLR Desktop Player
    // and Unity 6.8 -- the open case this guarded probe exists to exercise.
    public class ProbeRefFields : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_REF_FIELDS
            int backing = 40;
            var holder = new RefHolder(ref backing);
            holder.Value += 2;
            Debug.Log($"PASS: ref fields (C# 11) ran. backing = {backing}");
#else
            Debug.Log("SKIP: ref fields probe needs SHARPLINGS_REF_FIELDS.");
#endif
        }
    }

#if SHARPLINGS_REF_FIELDS
    internal ref struct RefHolder
    {
        public ref int Value;

        public RefHolder(ref int value)
        {
            Value = ref value;
        }
    }
#endif
}
