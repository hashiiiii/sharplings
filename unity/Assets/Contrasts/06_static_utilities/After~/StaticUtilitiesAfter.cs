using UnityEngine;

namespace Contrasts.StaticUtilities
{
    // Contrasts: 06_static_utilities -- After (C# 14, needs Unity 6.8 or a
    // Stage 2 Roslyn swap -- see docs/feature-matrix.md).
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today.
    //
    // Mirrors Before/StaticUtilitiesBefore.cs behavior 1:1 -- same
    // Debug.Log lines for the same Vector3 -- so the two files diff
    // cleanly.
    public static class VectorUtilsAfter
    {
        // Instance extension members: an `extension(Vector3 v)` block adds
        // members that read exactly like real instance members on Vector3
        // -- a property here, a method there -- with no wrapper argument
        // at the call site.
        extension(Vector3 v)
        {
            public float HorizontalSqrMagnitude => v.x * v.x + v.z * v.z;

            public Vector3 WithY(float y) => new(v.x, y, v.z);

            public Vector3 FlattenY() => new(v.x, 0f, v.z);
        }

        // Static extension members: an `extension(Vector3)` block (no
        // receiver name) adds members to Vector3's *static* surface,
        // callable as Vector3.FromPolarXZ(...) -- as if it were a real
        // static factory method on Vector3 itself.
        extension(Vector3)
        {
            public static Vector3 FromPolarXZ(float angleDegrees, float radius)
            {
                float radians = angleDegrees * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
            }
        }
    }

    public class StaticUtilitiesAfter : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            var position = new Vector3(3f, 5f, 4f);

            // Reads left-to-right, exactly like a real instance member --
            // HorizontalSqrMagnitude is even a property, not a method call.
            var grounded = position.WithY(0f);
            var horizontalSqrMag = position.HorizontalSqrMagnitude;
            var flat = position.FlattenY();
            var fromPolar = Vector3.FromPolarXZ(45f, 2f);

            Debug.Log($"grounded: {grounded}");
            Debug.Log($"horizontalSqrMagnitude: {horizontalSqrMag}");
            Debug.Log($"flat: {flat}");
            Debug.Log($"fromPolar: {fromPolar}");
        }
    }
}
