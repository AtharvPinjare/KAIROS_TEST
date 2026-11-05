using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Base class for every ability. An ability is a shared, STATELESS asset:
    /// anything it must remember per target goes into <see cref="ForceTarget.Remember{T}"/>.
    /// New ability = one subclass of this + one asset. Nothing else may change.
    /// </summary>
    public abstract class AbilityDefinition : ScriptableObject
    {
        [Header("Presentation")]
        public string DisplayName;
        public Sprite Icon;
        public Color Colour = Color.white;

        [Header("Behaviour")]
        [Tooltip("Seconds the effect lasts. 0 = instant (nothing to revert).")]
        public float Duration;
        [Tooltip("Names for the R-key variants, for example Down / Up / Left / Right.")]
        public string[] VariantNames = new string[0];

        /// <summary>Validation only: may this ability be applied to this target right now?</summary>
        public abstract CastRejectReason CanApply(AbilityContext ctx, ForceTarget target);

        /// <summary>Change the target. Store whatever Revert needs with target.Remember(this, ...).</summary>
        public abstract void Apply(AbilityContext ctx, ForceTarget target);

        /// <summary>Undo Apply exactly. A no-op for instant abilities.</summary>
        public abstract void Revert(AbilityContext ctx, ForceTarget target);
    }
}
