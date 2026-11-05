using System;
using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// The event channels the ability system raises, bundled so the controller and states stay short.
    /// Every channel is optional: a missing one is simply skipped.
    /// </summary>
    [Serializable]
    public class AbilityChannels
    {
        [Header("Raised by the ability system")]
        [SerializeField] private AbilityEventChannel onAbilitySelected;
        [SerializeField] private IntEventChannel onVariantChanged;
        [SerializeField] private ForceTargetEventChannel onTargetChanged;
        [SerializeField] private VoidEventChannel onTargetingStarted;
        [SerializeField] private VoidEventChannel onTargetingEnded;
        [SerializeField] private AbilityEventChannel onCastStarted;
        [SerializeField] private ForceTargetEventChannel onEffectApplied;
        [SerializeField] private ForceTargetEventChannel onEffectReverted;
        [SerializeField] private RejectEventChannel onCastRejected;
        [SerializeField] private FloatEventChannel onCooldownChanged;

        [Header("Listened to")]
        [SerializeField, Tooltip("Raised by PlayerRespawn. The controller calls ForceReset() when it fires.")]
        private VoidEventChannel onPlayerRespawn;

        public VoidEventChannel PlayerRespawn => onPlayerRespawn;

        public void AbilitySelected(AbilityDefinition a) => onAbilitySelected?.Raise(a);
        public void VariantChanged(int v) => onVariantChanged?.Raise(v);
        ForceTarget lastTarget;   // the target listeners were last told about (not serialized)

        /// <summary>Raises OnTargetChanged only when the target really changed (null = no target).</summary>
        public void TargetChanged(ForceTarget t)
        {
            if (t == lastTarget) return;
            lastTarget = t;
            onTargetChanged?.Raise(t);
        }
        public void TargetingStarted() => onTargetingStarted?.Raise();
        public void TargetingEnded() => onTargetingEnded?.Raise();
        public void CastStarted(AbilityDefinition a) => onCastStarted?.Raise(a);
        public void EffectApplied(ForceTarget t) => onEffectApplied?.Raise(t);
        public void EffectReverted(ForceTarget t) => onEffectReverted?.Raise(t);
        public void CastRejected(CastRejectReason r) => onCastRejected?.Raise(r);
        public void CooldownChanged(float normalized) => onCooldownChanged?.Raise(normalized);
    }
}
