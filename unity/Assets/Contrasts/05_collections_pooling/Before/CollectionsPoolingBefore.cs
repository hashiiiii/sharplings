using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contrasts.CollectionsPooling
{
    // Contrasts: 05_collections_pooling -- Before (C# 9, today's Unity
    // default). See README.md in this folder for the full EN/JA
    // explanation.
    public class CollectionsPoolingBefore : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            int[] baseHits = ProbeHits();
            int[] extraHits = { 40, 50 };

            // Manual array juggling: alloc the combined backing array by
            // hand and copy both halves into it.
            var combined = new int[baseHits.Length + extraHits.Length];
            Array.Copy(baseHits, combined, baseHits.Length);
            Array.Copy(extraHits, 0, combined, baseHits.Length, extraHits.Length);

            Debug.Log($"combined: [{string.Join(", ", combined)}]");

            // A brand-new List<T> allocated every call -- fine once, but
            // this is exactly the kind of per-call allocation that adds
            // up if it runs every frame.
            Debug.Log($"over 30: [{string.Join(", ", FilterOver30(combined))}]");
        }

        private static int[] ProbeHits() => new[] { 10, 20, 30 };

        private static List<int> FilterOver30(int[] source)
        {
            var result = new List<int>();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] > 30)
                {
                    result.Add(source[i]);
                }
            }

            return result;
        }
    }
}
