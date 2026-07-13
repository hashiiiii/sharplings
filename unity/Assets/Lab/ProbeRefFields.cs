using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: ref fields (C# 11), guarded by SHARPLINGS_REF_FIELDS.
    //
    // A ref field in a ref struct (`public ref int Value;`) is a pure
    // language + runtime feature -- there is no attribute PolySharp can supply to
    // make it work, so PolySharp does not change this probe's outcome. It needs
    // the runtime's byref-field support, which Mono is not expected to have.
    // Whether the Stage 2 Roslyn 5.6.0 compiles it against Unity's .NET Standard
    // 2.1 reference assemblies, and whether the Mono editor runtime then runs it,
    // is what this probe measures. Record the empirical outcome.
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
