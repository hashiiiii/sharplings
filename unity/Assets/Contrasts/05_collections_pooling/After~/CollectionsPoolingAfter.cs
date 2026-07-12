using System;
using System.Buffers;
using UnityEngine;

namespace Contrasts.CollectionsPooling
{
    // Contrasts: 05_collections_pooling -- After (needs Unity 6.8, or a
    // Stage 1 langversion bump for the collection-expression/spread
    // syntax specifically -- see README.md).
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today.
    //
    // Mirrors Before/CollectionsPoolingBefore.cs behavior 1:1 -- same
    // Debug.Log lines for "combined"/"over 30" -- so the two files diff
    // cleanly.
    public class CollectionsPoolingAfter : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            int[] baseHits = ProbeHits();
            int[] extraHits = [40, 50];

            // Collection expression + spread: no manual Array.Copy
            // juggling to build the combined array.
            int[] combined = [.. baseHits, .. extraHits];

            Debug.Log($"combined: [{string.Join(", ", combined)}]");

            Debug.Log($"over 30: [{string.Join(", ", FilterOver30(combined))}]");
        }

        private static int[] ProbeHits() => [10, 20, 30];

        private static int[] FilterOver30(ReadOnlySpan<int> source)
        {
            // Rent a scratch buffer from the shared pool instead of
            // allocating a fresh List<T> every call, then slice the
            // pooled buffer with Span<T> down to just the matches.
            int[] buffer = ArrayPool<int>.Shared.Rent(source.Length);
            try
            {
                Span<int> scratch = buffer;
                int count = 0;

                foreach (int value in source)
                {
                    if (value > 30)
                    {
                        scratch[count++] = value;
                    }
                }

                return scratch[..count].ToArray();
            }
            finally
            {
                ArrayPool<int>.Shared.Return(buffer);
            }
        }
    }
}
