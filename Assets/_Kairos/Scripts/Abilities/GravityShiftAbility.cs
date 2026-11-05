using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Points the target's gravity down / up / left / right for a while, then puts it back
    /// exactly as it was (for the cargo pod that means back to "up", against the ceiling).
    /// </summary>
    [CreateAssetMenu(menuName = "Kairos/Abilities/Gravity Shift", fileName = "GravityShift")]
    public class GravityShiftAbility : AbilityDefinition
    {
        // Order must match VariantNames: Down, Up, Left, Right.
        static readonly Vector3[] Directions =
        {
            Vector3.down, Vector3.up, Vector3.left, Vector3.right
        };

        void Reset()
        {
            DisplayName = "Gravity Shift";
            Duration = 12f;
            VariantNames = new[] { "Down", "Up", "Left", "Right" };
        }

        /// <inheritdoc />
        public override CastRejectReason CanApply(AbilityContext ctx, ForceTarget target) =>
            target.Attributes.canManipulateGravity ? CastRejectReason.None : CastRejectReason.CapabilityLocked;

        /// <inheritdoc />
        public override void Apply(AbilityContext ctx, ForceTarget target)
        {
            // Remember the old gravity so Revert can restore it exactly, whatever it was.
            target.Remember(this, (target.Gravity.Enabled, target.Gravity.Direction));
            target.Gravity.Set(true, DirectionFor(ctx.Variant));
            target.Body.WakeUp();   // a sleeping body would ignore the new gravity
        }

        /// <inheritdoc />
        public override void Revert(AbilityContext ctx, ForceTarget target)
        {
            if (!target.Has(this)) return;   // never applied (or already reset): nothing to undo
            var (enabled, direction) = target.Recall<(bool, Vector3)>(this);
            target.Gravity.Set(enabled, direction);
            target.Body.WakeUp();
        }

        static Vector3 DirectionFor(int variant) =>
            Directions[Mathf.Clamp(variant, 0, Directions.Length - 1)];
    }
}
