using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Everything an ability needs to know about one cast, bundled into a small immutable value.
    /// Built by the AbilityController; abilities only read it.
    /// </summary>
    public readonly struct AbilityContext
    {
        /// <summary>The player / gauntlet that casts.</summary>
        public readonly Transform Source;
        /// <summary>The caster's magnetic polarity.</summary>
        public readonly MagneticPolarity SourcePolarity;
        /// <summary>Index into the ability's VariantNames, chosen with R.</summary>
        public readonly int Variant;
        /// <summary>Direction from source to target, flattened onto the XY play plane.</summary>
        public readonly Vector3 AimDirection;

        public AbilityContext(Transform source, MagneticPolarity polarity, int variant, Vector3 aimDirection)
        {
            Source = source;
            SourcePolarity = polarity;
            Variant = variant;
            AimDirection = aimDirection;
        }
    }
}
