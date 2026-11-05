using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// The effect is running. Enter applies it, Exit ALWAYS reverts it, so a respawn,
    /// a destroyed target or a normal timeout can never leave a prop stuck in a changed state.
    /// </summary>
    public class ActiveState : IAbilityState
    {
        readonly AbilityController controller;

        public ActiveState(AbilityController controller) => this.controller = controller;

        public void Enter()
        {
            controller.BeginEffect();
            controller.ActiveRemaining = controller.Selected.Duration;
            controller.Channels.CastStarted(controller.Selected);
        }

        public void Tick()
        {
            // Instant abilities (Duration 0) and destroyed targets end immediately.
            if (controller.Selected.Duration <= 0f || controller.Target == null)
            {
                controller.GoTo(controller.Cooldown);
                return;
            }
            // Scaled time on purpose: hit-stop (timeScale ~ 0) pauses the effect timer too.
            controller.ActiveRemaining -= Time.deltaTime;
            if (controller.ActiveRemaining <= 0f) controller.GoTo(controller.Cooldown);
        }

        public void Exit()
        {
            controller.EndEffect();   // safe to call from ForceReset
            controller.ActiveRemaining = 0f;
        }
    }
}
