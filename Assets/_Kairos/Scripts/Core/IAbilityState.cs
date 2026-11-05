namespace Kairos
{
    /// <summary>
    /// One state of the ability state machine (Ready, Targeting, Active, Cooldown).
    /// Named Tick() instead of Update() so it never collides with MonoBehaviour.Update.
    /// </summary>
    public interface IAbilityState
    {
        /// <summary>Called once when the machine switches to this state.</summary>
        void Enter();

        /// <summary>Called every frame while this state is current.</summary>
        void Tick();

        /// <summary>Called once when the machine leaves this state (guaranteed on every transition).</summary>
        void Exit();
    }
}
