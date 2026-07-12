using UnityEngine;

namespace Contrasts.StaticUtilities
{
    // Contrasts: 06_static_utilities -- Before (C# 9, today's Unity default).
    // See README.md in this folder for the full EN/JA explanation.
    //
    // The C# 9 Unity idiom: a static helper class, each call wrapping its
    // subject as the first argument.
    public static class VectorUtilsBefore
    {
        public static Vector3 WithY(Vector3 v, float y) => new Vector3(v.x, y, v.z);

        public static float HorizontalSqrMagnitude(Vector3 v) => v.x * v.x + v.z * v.z;

        public static Vector3 FlattenY(Vector3 v) => new Vector3(v.x, 0f, v.z);

        public static Vector3 FromPolarXZ(float angleDegrees, float radius)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
        }
    }

    public class StaticUtilitiesBefore : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            var position = new Vector3(3f, 5f, 4f);

            // Call-site noise: every helper call wraps its subject as the
            // first argument, reading "outside-in" instead of left-to-right.
            var grounded = VectorUtilsBefore.WithY(position, 0f);
            var horizontalSqrMag = VectorUtilsBefore.HorizontalSqrMagnitude(position);
            var flat = VectorUtilsBefore.FlattenY(position);
            var fromPolar = VectorUtilsBefore.FromPolarXZ(45f, 2f);

            Debug.Log($"grounded: {grounded}");
            Debug.Log($"horizontalSqrMagnitude: {horizontalSqrMag}");
            Debug.Log($"flat: {flat}");
            Debug.Log($"fromPolar: {fromPolar}");
        }
    }
}
