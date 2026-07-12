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
    // Mirrors Before/DataModelingBefore.cs -- same Debug.Log lines for
    // "potions"/"potions after pickup" -- with one intentional ordering
    // divergence: the equality comparison is logged BEFORE the pickup
    // (where record value equality observably prints True), plus once
    // more after it (False). See Run() and README.md. Also see
    // README.md's honest note about the Unity serializer before reusing
    // this shape for Inspector-exposed data.

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

            // Value equality out of the box -- no hand-written Equals/==.
            // Compared HERE, while the data still matches, both lines
            // print True; Before's reference-equality version prints
            // False for the same data no matter where the check sits.
            // (Intentional divergence from the strict 1:1 mirror: Before
            // logs its comparison only after the pickup -- see README.md.)
            Debug.Log($"potions == potionsAgain: {potions == potionsAgain}");
            Debug.Log($"potions.Equals(potionsAgain): {potions.Equals(potionsAgain)}");

            // Records are immutable by default: "picking up one more"
            // produces a new value via `with`, instead of mutating the
            // original in place.
            potions = potions with { Count = potions.Count + 1 };
            Debug.Log($"potions after pickup: {potions.Id} x{potions.Count}");

            // Now the data no longer matches, so the same comparison is
            // False -- the `with` expression changed this copy's data,
            // not the equality semantics.
            Debug.Log($"potions == potionsAgain (after pickup): {potions == potionsAgain}");

            // `required` + `init`: the compiler enforces that every
            // required member is set at construction (via object
            // initializer syntax), and forbids changing it afterward.
            var slot = new ItemSlotAfter { Id = "potion", Count = 3 };
            Debug.Log($"slot: {slot.Id} x{slot.Count}");
        }
    }
}
