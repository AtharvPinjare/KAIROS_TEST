using UnityEngine;

namespace Kairos
{
    /// <summary>Channel carrying a ForceTarget; null means "no target" (OnTargetChanged, OnEffectApplied/Reverted).</summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Force Target", fileName = "OnSomething")]
    public class ForceTargetEventChannel : EventChannel<ForceTarget> { }
}
