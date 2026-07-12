using UnityEngine;

namespace Contrasts.NullHandling
{
    // Contrasts: 01_null_handling -- Before (C# 9, today's Unity default).
    // See README.md in this folder for the full EN/JA explanation.
    //
    // Self-contained: right-click this component in the Inspector and pick
    // "Run" from the context menu (or the gear icon), then read the
    // Console. No scene wiring required -- the demo builds its own targets.
    public class NullHandlingBefore : MonoBehaviour
    {
        // A minimal target component. MonoBehaviour already exposes
        // `.enabled` (inherited from Behaviour), which is enough to
        // exercise the brief's idiom: `if (target != null && target.enabled)`.
        private sealed class DemoTarget : MonoBehaviour
        {
        }

        // A plain C# class -- NOT a UnityEngine.Object. Used below to
        // contrast the safe, ordinary meaning of "null" against the Unity
        // fake-null case above.
        private sealed class Stats
        {
            public int Score;
        }

        private DemoTarget _target;
        private readonly Stats _liveStats = new Stats { Score = 10 };
        private readonly Stats _missingStats = null; // intentionally null, for contrast

        [ContextMenu("Run")]
        private void Run()
        {
            var go = new GameObject("DemoTarget");
            _target = go.AddComponent<DemoTarget>();

            ReportTarget("before destroy");

            // Simulate a scene teardown / Destroy() call. After this line,
            // `_target` is Unity's "fake null": the underlying native
            // object is gone, but the C# reference field itself still
            // refers to a (dead) managed wrapper -- it is not literally
            // null from the CLR's point of view.
            DestroyImmediate(go);

            ReportTarget("after destroy");

            ReportStats("before assignment");

            // C# 9 has no null-conditional *assignment* -- writing through
            // a possibly-null reference still needs an explicit `if`.
            if (_liveStats != null)
            {
                _liveStats.Score = 99;
            }

            if (_missingStats != null)
            {
                _missingStats.Score = 99;
            }

            ReportStats("after assignment");
        }

        private void ReportTarget(string label)
        {
            // The C# 9 Unity idiom: an explicit `!= null` chain.
            // `UnityEngine.Object` overloads `==`/`!=` so a destroyed
            // object still compares equal to `null` here, even though the
            // underlying C# reference is not actually null. That overload
            // is exactly what makes this chain safe to call after Destroy.
            if (_target != null && _target.enabled)
            {
                Debug.Log($"[{label}] target is alive and enabled");
            }
            else
            {
                Debug.Log($"[{label}] target is null-or-disabled (Unity fake-null check)");
            }
        }

        private void ReportStats(string label)
        {
            Debug.Log($"[{label}] liveStats.Score = {_liveStats.Score}");
            Debug.Log($"[{label}] missingStats == null: {_missingStats == null}");
        }
    }
}
