using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Recharge after an effect. Casting is blocked, but the selection keys still work
    /// so the ability bar stays responsive. Raises the normalized cooldown (1 to 0) every frame.
    /// </summary>
    public class CooldownState : IAbilityState
    {
        readonly AbilityController controller;

        public CooldownState(AbilityController controller) => this.controller = controller;

        public void Enter()
        {
            controller.CooldownRemaining = controller.CooldownDuration;
            controller.Channels.CooldownChanged(1f);
        }

        public void Tick()
        {
            controller.SelectFromInput();
            controller.CooldownRemaining -= Time.deltaTime;
            float normalized = controller.CooldownDuration > 0f
                ? Mathf.Max(0f, controller.CooldownRemaining / controller.CooldownDuration) : 0f;
            controller.Channels.CooldownChanged(normalized);
            if (controller.CooldownRemaining <= 0f) controller.GoTo(controller.Ready);
        }

        public void Exit()
        {
            controller.CooldownRemaining = 0f;
            controller.Channels.CooldownChanged(0f);   // make sure the UI ring ends empty, even on ForceReset
        }
    }
}
