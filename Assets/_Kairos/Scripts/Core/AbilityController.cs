using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Coordinates the ability system and nothing else: owns the state machine, the selected
    /// ability / variant / target, builds the AbilityContext and raises events.
    /// Behaviour lives in the states; the physics change lives in the AbilityDefinition.
    /// </summary>
    public class AbilityController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("The only object allowed to read devices.")]
        private PlayerInputReader input;
        [SerializeField] private AbilityLoadout loadout;
        [SerializeField] private TargetFinder finder;
        [SerializeField, Tooltip("Where casts start from (player or gauntlet).")]
        private Transform source;
        [SerializeField] private AbilityChannels channels;

        [Header("Tuning")]
        [SerializeField, Tooltip("The caster's magnetic polarity.")]
        private MagneticPolarity polarity = MagneticPolarity.North;
        [SerializeField, Tooltip("Seconds between the end of an effect and the next cast.")]
        private float cooldownDuration = 2f;

        readonly AbilityStateMachine machine = new AbilityStateMachine();
        readonly AppliedEffect effect = new AppliedEffect();   // remembers what to revert

        public AbilityDefinition Selected { get; private set; }
        public int Variant { get; private set; }
        public ForceTarget Target { get; private set; }

        // Read-only views for states, UI and the debug overlay.
        public PlayerInputReader Input => input;
        public AbilityChannels Channels => channels;
        public float CooldownDuration => cooldownDuration;
        public string StateName => machine.Current == null ? "None" : machine.Current.GetType().Name;
        public float CooldownRemaining { get; internal set; }
        public float ActiveRemaining { get; internal set; }

        // The four states. Created once; they never reference each other, only this controller.
        public ReadyState Ready { get; private set; }
        public TargetingState Targeting { get; private set; }
        public ActiveState Active { get; private set; }
        public CooldownState Cooldown { get; private set; }

        void Awake()
        {
            finder.Initialize();
            Ready = new ReadyState(this);
            Targeting = new TargetingState(this);
            Active = new ActiveState(this);
            Cooldown = new CooldownState(this);
        }

        void Start()
        {
            machine.Initialize(Ready);
            Select(0);   // first slot is selected at start so the UI has something to show
        }

        /// <summary>Number keys 1 to 4 choose an ability (used by every state that allows selection).</summary>
        public void SelectFromInput()
        {
            if (input.AbilitySelect >= 0) Select(input.AbilitySelect);
        }

        void OnEnable() { if (channels.PlayerRespawn != null) channels.PlayerRespawn.Raised += ForceReset; }
        void OnDisable() { if (channels.PlayerRespawn != null) channels.PlayerRespawn.Raised -= ForceReset; }

        void Update() => machine.Tick();

        /// <summary>Ask the state machine to switch (states never reference each other).</summary>
        public void GoTo(IAbilityState state) => machine.ChangeState(state);

        /// <summary>Choose the ability in a loadout slot. Empty slots are ignored.</summary>
        public void Select(int index)
        {
            var ability = loadout.Get(index);
            if (ability == null) return;
            Selected = ability;
            Variant = 0;
            channels.AbilitySelected(ability);
            channels.VariantChanged(Variant);
        }

        /// <summary>Next variant of the selected ability, wrapping around.</summary>
        public void CycleVariant()
        {
            if (Selected == null || Selected.VariantNames.Length == 0) return;
            Variant = (Variant + 1) % Selected.VariantNames.Length;
            channels.VariantChanged(Variant);
        }

        /// <summary>Look under the pointer. OnTargetChanged is raised only when the target changes.</summary>
        public void RefreshTarget()
        {
            Target = finder.Find(input.PointerScreenPosition, source);
            channels.TargetChanged(Target);
        }

        /// <summary>Tell listeners to drop the highlight. The target itself stays for the cast that follows.</summary>
        public void ClearHighlight() => channels.TargetChanged(null);

        /// <summary>Forget the target (back to Ready).</summary>
        public void ClearTarget()
        {
            Target = null;
            ClearHighlight();
        }

        /// <summary>Validation only, no side effects: why a cast would be refused (None = allowed).</summary>
        public CastRejectReason TryCast()
        {
            if (Selected == null || Target == null) return CastRejectReason.NoTarget;
            if (machine.Current == Cooldown) return CastRejectReason.OnCooldown;
            return Selected.CanApply(BuildContext(Target), Target);
        }

        /// <summary>Apply the selected ability to the current target.</summary>
        public void BeginEffect()
        {
            effect.Apply(Selected, Target, BuildContext(Target));
            channels.EffectApplied(Target);
        }

        /// <summary>Revert what BeginEffect applied. Safe to call twice and when the target was destroyed.</summary>
        public void EndEffect()
        {
            var reverted = effect.Revert();
            if (reverted != null) channels.EffectReverted(reverted);
        }

        /// <summary>Respawn hook: revert any running effect and return to Ready.</summary>
        public void ForceReset()
        {
            GoTo(Ready);    // leaving Active reverts the effect (ActiveState.Exit)
            EndEffect();    // no-op unless an effect is somehow still applied
            ClearTarget();
        }

        AbilityContext BuildContext(ForceTarget target)
        {
            Vector3 aim = target.transform.position - source.position;
            aim.z = 0f;   // 2.5D: depth is not gameplay
            return new AbilityContext(source, polarity, Variant, aim.normalized);
        }
    }
}
