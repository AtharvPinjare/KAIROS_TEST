namespace Kairos
{
    /// <summary>
    /// Remembers which ability was applied to which target with which context, so Revert
    /// undoes exactly that, even if the selection changed or the target was destroyed meanwhile.
    /// </summary>
    public class AppliedEffect
    {
        AbilityDefinition ability;
        ForceTarget target;
        AbilityContext context;

        /// <summary>True between Apply and Revert.</summary>
        public bool IsApplied => ability != null;

        /// <summary>Apply and remember.</summary>
        public void Apply(AbilityDefinition def, ForceTarget forceTarget, AbilityContext ctx)
        {
            ability = def;
            target = forceTarget;
            context = ctx;
            def.Apply(ctx, forceTarget);
        }

        /// <summary>
        /// Revert what Apply did. Safe to call twice. Returns the target that was reverted,
        /// or null when nothing was applied or the target no longer exists.
        /// </summary>
        public ForceTarget Revert()
        {
            if (ability == null) return null;
            var def = ability;
            var forceTarget = target;
            ability = null;
            target = null;
            if (forceTarget == null) return null;   // destroyed mid-effect: nothing left to revert
            def.Revert(context, forceTarget);
            return forceTarget;
        }
    }
}
