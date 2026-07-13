using UnityEngine;

namespace Sharplings.Lab
{
    // Frontier probe: static abstract interface members (C# 11), guarded by
    // SHARPLINGS_STATIC_ABSTRACT.
    //
    // Uses a custom interface (no BCL generic-math dependency) so the signal
    // isolates the runtime feature -- static virtual/abstract interface dispatch.
    // This is a runtime capability, not an attribute, so PolySharp does not affect
    // it. Whether the Stage 2 Roslyn 5.6.0 compiles it and whether the Mono editor
    // runtime dispatches it correctly is what this probe measures. Record the
    // empirical outcome.
    public class ProbeStaticAbstract : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
#if SHARPLINGS_STATIC_ABSTRACT
            Debug.Log($"PASS: static abstract members (C# 11) ran. Zero = {ZeroOf<MyInt>()}");
#else
            Debug.Log("SKIP: static abstract probe needs SHARPLINGS_STATIC_ABSTRACT.");
#endif
        }

#if SHARPLINGS_STATIC_ABSTRACT
        private static T ZeroOf<T>() where T : IZero<T> => T.Zero;
#endif
    }

#if SHARPLINGS_STATIC_ABSTRACT
    internal interface IZero<T> where T : IZero<T>
    {
        static abstract T Zero { get; }
    }

    internal readonly struct MyInt : IZero<MyInt>
    {
        private readonly int _value;

        public MyInt(int value)
        {
            _value = value;
        }

        public static MyInt Zero => new MyInt(0);

        public override string ToString() => _value.ToString();
    }
#endif
}
