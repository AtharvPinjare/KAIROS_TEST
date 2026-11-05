namespace Kairos
{
    /// <summary>
    /// Holds the current ability state and performs transitions.
    /// It knows nothing about abilities, input or events: it only swaps states safely.
    /// </summary>
    public class AbilityStateMachine
    {
        /// <summary>The state that is running right now.</summary>
        public IAbilityState Current { get; private set; }

        /// <summary>Set the first state and enter it.</summary>
        public void Initialize(IAbilityState start)
        {
            Current = start;
            Current.Enter();
        }

        /// <summary>Exit the current state, switch, enter the new one. Ignores null and "already there".</summary>
        public void ChangeState(IAbilityState next)
        {
            if (next == null || next == Current) return;
            Current?.Exit();   // guaranteed on every transition, so effects can always be reverted
            Current = next;
            Current.Enter();
        }

        /// <summary>Run the current state's per-frame logic.</summary>
        public void Tick() => Current?.Tick();
    }
}
