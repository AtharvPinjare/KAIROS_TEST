using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// The ordered list of abilities on keys 1 to 4. Slots may be empty until later phases add them.
    /// Adding an ability = drag its asset into a slot. Nothing else changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Kairos/Ability Loadout", fileName = "AbilityLoadout")]
    public class AbilityLoadout : ScriptableObject
    {
        public const int SlotCount = 4;

        [SerializeField, Tooltip("Slot 0 = key 1, slot 1 = key 2, and so on.")]
        private AbilityDefinition[] slots = new AbilityDefinition[SlotCount];

        /// <summary>Ability in a slot, or null if the slot is empty or out of range.</summary>
        public AbilityDefinition Get(int index) =>
            index >= 0 && index < slots.Length ? slots[index] : null;

        /// <summary>Number of slots (always at least SlotCount).</summary>
        public int Count => slots.Length;
    }
}
