using System;
using UnityEngine;

namespace Contrasts.DataModeling
{
    // Contrasts: 02_data_modeling -- Before (C# 9, today's Unity default).
    // See README.md in this folder for the full EN/JA explanation.
    //
    // The C# 9 Unity idiom: a mutable [Serializable] class DTO with
    // hand-written constructor boilerplate. No value equality, no
    // immutability -- anything holding a reference can mutate it.
    [Serializable]
    public class ItemStackBefore
    {
        public string Id;
        public int Count;

        public ItemStackBefore(string id, int count)
        {
            Id = id;
            Count = count;
        }
    }

    public class DataModelingBefore : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            var potions = new ItemStackBefore("potion", 3);
            var potionsAgain = new ItemStackBefore("potion", 3);

            Debug.Log($"potions: {potions.Id} x{potions.Count}");

            // Mutable DTO: "picking up one more" means mutating the
            // instance in place.
            potions.Count += 1;
            Debug.Log($"potions after pickup: {potions.Id} x{potions.Count}");

            // Reference equality only. Two DTOs with identical data are
            // not "equal" unless Equals/== is hand-written.
            Debug.Log($"potions == potionsAgain: {potions == potionsAgain}");
            Debug.Log($"potions.Equals(potionsAgain): {potions.Equals(potionsAgain)}");
        }
    }
}
