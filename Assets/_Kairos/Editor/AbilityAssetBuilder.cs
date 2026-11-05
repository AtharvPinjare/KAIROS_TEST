using UnityEditor;
using UnityEngine;

namespace Kairos.EditorTools
{
    /// <summary>
    /// Creates (or updates) the ScriptableObject assets the ability system needs:
    /// the Gravity Shift ability, the loadout and the event channels. Re-runnable.
    /// </summary>
    public static class AbilityAssetBuilder
    {
        const string EventsFolder = "Assets/_Kairos/ScriptableObjects/Events";
        const string AbilitiesFolder = "Assets/_Kairos/ScriptableObjects/Abilities";

        /// <summary>Load or create one event channel asset by name (for example "OnCastRejected").</summary>
        public static T Channel<T>(string assetName) where T : ScriptableObject =>
            BuildUtil.LoadOrCreate<T>($"{EventsFolder}/{assetName}.asset");

        /// <summary>The Gravity Shift asset: 12 s, four variants.</summary>
        public static GravityShiftAbility GravityShift()
        {
            var ability = BuildUtil.LoadOrCreate<GravityShiftAbility>($"{AbilitiesFolder}/GravityShift.asset");
            ability.DisplayName = "Gravity Shift";
            ability.Duration = 12f;
            ability.VariantNames = new[] { "Down", "Up", "Left", "Right" };
            ability.Colour = new Color(0.4f, 0.8f, 1f);
            EditorUtility.SetDirty(ability);
            return ability;
        }

        /// <summary>The loadout: slot 0 = Gravity Shift, the other three stay empty until cp3 and cp4.</summary>
        public static AbilityLoadout Loadout(AbilityDefinition slot0)
        {
            var loadout = BuildUtil.LoadOrCreate<AbilityLoadout>($"{AbilitiesFolder}/AbilityLoadout.asset");
            var so = new SerializedObject(loadout);
            var slots = so.FindProperty("slots");
            slots.arraySize = AbilityLoadout.SlotCount;
            slots.GetArrayElementAtIndex(0).objectReferenceValue = slot0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loadout);
            return loadout;
        }
    }
}
