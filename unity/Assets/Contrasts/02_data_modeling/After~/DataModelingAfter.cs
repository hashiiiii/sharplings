using UnityEngine;

namespace Contrasts.DataModeling
{
    // Contrasts: 02_data_modeling -- After (needs Unity 6.8, or PolySharp
    // shims + a Stage 1/2 langversion bump today -- see README.md and
    // docs/unity-lab-setup.md).
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today.
    //
    // Mirrors Before/DataModelingBefore.cs behavior 1:1 -- same
    // Debug.Log lines for "potions"/"potions after pickup" -- plus the
    // extra value-equality/`required` demonstration this modern shape
    // enables. See README.md's honest note about the Unity serializer
    // before reusing this shape for Inspector-exposed data.

    // A record: value equality and immutability by default, positional
    // deconstruction. Compare with ItemStackBefore above.
    public record ItemStackAfter(string Id, int Count);

    // A record struct with `required`/`init`: for small, frequently-copied
    // runtime domain values, where struct semantics (no separate heap
    // allocation for the instance itself) are wanted alongside
    // required-at-construction fields.
    public record struct ItemSlotAfter
    {
        public required string Id { get; init; }
        public required int Count { get; init; }
    }

    public class DataModelingAfter : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            var potions = new ItemStackAfter("potion", 3);
            var potionsAgain = new ItemStackAfter("potion", 3);

            Debug.Log($"potions: {potions.Id} x{potions.Count}");

            // Records are immutable by default: "picking up one more"
            // produces a new value via `with`, instead of mutating the
            // original in place.
            potions = potions with { Count = potions.Count + 1 };
            Debug.Log($"potions after pickup: {potions.Id} x{potions.Count}");

            // Value equality out of the box -- no hand-written Equals/==.
            Debug.Log($"potions == potionsAgain: {potions == potionsAgain}");
            Debug.Log($"potions.Equals(potionsAgain): {potions.Equals(potionsAgain)}");

            // `required` + `init`: the compiler enforces that every
            // required member is set at construction (via object
            // initializer syntax), and forbids changing it afterward.
            var slot = new ItemSlotAfter { Id = "potion", Count = 3 };
            Debug.Log($"slot: {slot.Id} x{slot.Count}");
        }
    }
}
