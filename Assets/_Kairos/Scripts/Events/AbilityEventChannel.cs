using UnityEngine;

namespace Kairos
{
    /// <summary>Channel carrying an AbilityDefinition (OnAbilitySelected, OnCastStarted).</summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Ability", fileName = "OnSomething")]
    public class AbilityEventChannel : EventChannel<AbilityDefinition> { }
}
