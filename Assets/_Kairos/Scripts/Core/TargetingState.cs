namespace Kairos
{
    /// <summary>
    /// The player holds the target button and aims. Every frame it looks for a target;
    /// a cast attempt either starts the effect or is rejected WITHOUT leaving this state.
    /// </summary>
    public class TargetingState : IAbilityState
    {
        readonly AbilityController controller;

        public TargetingState(AbilityController controller) => this.controller = controller;

        public void Enter() => controller.Channels.TargetingStarted();

        public void Tick()
        {
            var input = controller.Input;
            // Not held any more (released, or input was disabled): stop aiming.
            if (input.TargetReleased || !input.TargetHeld)
            {
                controller.GoTo(controller.Ready);
                return;
            }

            controller.SelectFromInput();
            if (input.VariantPressed) controller.CycleVariant();
            controller.RefreshTarget();   // after selection, so the highlight is never stale

            if (!input.CastPressed) return;
            var reason = controller.TryCast();
            if (reason == CastRejectReason.None) controller.GoTo(controller.Active);
            else controller.Channels.CastRejected(reason);   // stay in Targeting, no cooldown used
        }

        public void Exit()
        {
            controller.Channels.TargetingEnded();
            controller.ClearHighlight();   // the target itself is kept for the cast that may follow
        }
    }
}
