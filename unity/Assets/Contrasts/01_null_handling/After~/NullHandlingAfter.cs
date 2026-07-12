using UnityEngine;

namespace Contrasts.NullHandling
{
    // Contrasts: 01_null_handling -- After (C# 14, needs Unity 6.8 or a
    // Stage 2 Roslyn swap -- see docs/feature-matrix.md).
    // See README.md in this folder for the full EN/JA explanation.
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today. Rename the folder to
    // "After" (dropping the "~") once Unity 6.8 (or a Stage 2 Roslyn swap,
    // see docs/unity-lab-setup.md) makes C# 14 available, to activate it.
    //
    // Mirrors Before/NullHandlingBefore.cs behavior 1:1 -- same Debug.Log
    // lines for the same scenarios -- so the two files diff cleanly.
    public class NullHandlingAfter : MonoBehaviour
    {
        private sealed class DemoTarget : MonoBehaviour
        {
        }

        private sealed class Stats
        {
            public int Score;
        }

        private DemoTarget? _target;
        private readonly Stats _liveStats = new() { Score = 10 };
        private readonly Stats? _missingStats = null; // intentionally null, for contrast

        [ContextMenu("Run")]
        private void Run()
        {
            var go = new GameObject("DemoTarget");
            _target = go.AddComponent<DemoTarget>();

            ReportTarget("before destroy");

            DestroyImmediate(go);

            ReportTarget("after destroy");

            ReportStats("before assignment");

            // C# 14 null-conditional assignment: `?.` (and `?[]`) can now
            // appear on the left of `=`. If the receiver is null, the
            // whole statement is a no-op and the right-hand side is not
            // even evaluated; otherwise the assignment runs normally.
            // This is exactly the `if (x != null) x.Member = value;`
            // shape above, collapsed to one line -- and it is genuinely
            // safe here because `Stats` is a plain C# class, not a
            // UnityEngine.Object. See README.md.
            _liveStats?.Score = 99;
            _missingStats?.Score = 99;

            ReportStats("after assignment");
        }

        private void ReportTarget(string label)
        {
            // IMPORTANT: this check is intentionally IDENTICAL to
            // Before/NullHandlingBefore.cs. `UnityEngine.Object`'s
            // overloaded `==`/`!=` is still the only correct way to test
            // a Unity object for "destroyed or null" -- that does not
            // change in C# 14.
            //
            // PITFALL (do not copy): rewriting this as
            // `if (_target is { enabled: true })` or as
            // `_target?.enabled == true` looks like a modern upgrade, but
            // both silently break here. Pattern-matching's null test
            // (`is null` / `is { ... }`) and `?.`'s null test are raw
            // reference checks -- they bypass any user-defined `==`
            // entirely. A destroyed-but-fake-null `_target` sails
            // straight through either one, so the pattern/`?.` version
            // would report "alive" (or throw down inside `.enabled`)
            // exactly when the correct answer is "destroyed". Keep the
            // explicit `!= null` check for every UnityEngine.Object field.
            // See README.md and docs/feature-matrix.md.
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
            // Safe here: `Stats` is a plain C# reference type, so `?.` and
            // `is null` mean exactly what they look like they mean.
            Debug.Log($"[{label}] liveStats.Score = {_liveStats?.Score}");
            Debug.Log($"[{label}] missingStats == null: {_missingStats is null}");
        }
    }
}
