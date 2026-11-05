namespace Kairos
{
    /// <summary>Idle: the player can pick an ability and a variant, then hold the target button to start aiming.</summary>
    public class ReadyState : IAbilityState
    {
        readonly AbilityController controller;

        public ReadyState(AbilityController controller) => this.controller = controller;

        public void Enter() => controller.ClearTarget();

        public void Tick()
        {
            controller.SelectFromInput();
            if (controller.Input.VariantPressed) controller.CycleVariant();
            if (controller.Input.TargetPressed) controller.GoTo(controller.Targeting);
        }

        public void Exit() { }
    }
}
