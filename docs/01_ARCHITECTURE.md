# 01 — Architecture (the contract)

## Principle
**Controller coordinates. State machine transitions. States define behaviour. Effects execute. Targets hold state. Attributes are data. Events decouple everything else.**

This is also the **prompt-friendly** architecture: each file has a small interface, so a student can ask Claude for one file without pasting the whole project.

## Layer diagram

```
PlayerInputReader (only input reader)
        │ properties / events
        ▼
AbilityController ──owns──▶ AbilityStateMachine
   │   │                         │ current IAbilityState
   │   │                         ▼
   │   │              Ready / Targeting / Active / Cooldown
   │   │
   │   ├─ TargetFinder (raycast → ForceTarget)
   │   └─ AbilityLoadout (4 × AbilityDefinition SO)
   ▼
AbilityDefinition (abstract SO) ─ Apply(ctx, target) / Revert(ctx, target)
   ├─ GravityShiftAbility
   ├─ MassControlAbility
   ├─ MagneticPulseAbility  → KairosMagneticSystem (static helper)
   └─ FrictionFieldAbility
        ▼
ForceTarget (MonoBehaviour on props) ─ ObjectAttributes (data), CustomGravity, runtime PhysicsMaterial

Everything else listens to EventChannels:
  AbilityController ─raises─▶ [OnAbilitySelected, OnTargetChanged, OnCastStarted, OnCastRejected(reason),
                               OnEffectApplied, OnEffectReverted, OnCooldownChanged(float)]
  UI, Feedback, Audio, Camera, Level objects subscribe.
```

## Contracts

```csharp
namespace Kairos
{
    public interface IAbilityState { void Enter(); void Tick(); void Exit(); }

    public enum CastRejectReason { None, NoTarget, CapabilityLocked, NotMagnetic, OnCooldown }

    public readonly struct AbilityContext
    {
        public readonly Transform Source;        // the player/gauntlet
        public readonly MagneticPolarity SourcePolarity;
        public readonly int Variant;             // chosen with R
        public readonly Vector3 AimDirection;    // source → target, projected on XY plane
    }

    public abstract class AbilityDefinition : ScriptableObject
    {
        public string DisplayName;  public Sprite Icon;  public Color Colour;
        public float Duration;      // 0 = instant (nothing to revert)
        public string[] VariantNames;           // e.g. { "Down", "Up", "Left", "Right" }
        public abstract CastRejectReason CanApply(AbilityContext ctx, ForceTarget target);
        public abstract void Apply(AbilityContext ctx, ForceTarget target);
        public abstract void Revert(AbilityContext ctx, ForceTarget target);   // no-op for instant abilities
    }
}
```

Notes on the interface naming: the original workshop code used `Update()` on states. Use `Tick()` here so states never collide with the Unity message name and students do not confuse a state's per-frame method with `MonoBehaviour.Update`.

**Per-target memory (what to restore on Revert):** effects are shared assets and must hold **no per-target data**. `ForceTarget` stores a small memento per ability: `ForceTarget.Remember<T>(AbilityDefinition, T value)` / `ForceTarget.Recall<T>(AbilityDefinition)`.

## State machine behaviour

- `AbilityStateMachine.ChangeState(next)` does nothing if `next` is null or equals the current state; otherwise `Exit()` → swap → `Enter()`.
- States receive the `AbilityController` in their constructor and never reference each other directly: they ask the controller (`controller.GoTo(controller.Active)`).
- Input is read via `PlayerInputReader` properties only.
- **Active.Enter** applies the effect; **Active.Exit** reverts it. Because `Exit` is guaranteed on every transition, respawn or target destruction cannot leave a target stuck in a changed state.
- Edge cases to handle explicitly: target destroyed during Active; player dies during Active (`ForceReset`); cast rejected (stay in Targeting, raise `OnCastRejected(reason)`, do not consume cooldown); ability selection changed during Targeting.

## Event channels
Concrete ScriptableObject channels (generic base + one concrete subclass per payload type because Unity cannot serialize open generics): `VoidEventChannel`, `FloatEventChannel`, `IntEventChannel`, `BoolEventChannel`, `AbilityEventChannel` (payload `AbilityDefinition`), `ForceTargetEventChannel`, `RejectEventChannel` (payload `CastRejectReason`). Full spec: `systems/events-data.md`.

## Folder layout

```
Assets/_Kairos/
  Scripts/Core/        IAbilityState, AbilityStateMachine, AbilityController, TargetFinder, states…
  Scripts/Abilities/   AbilityDefinition + the four ability classes
  Scripts/Targets/     ForceTarget, ObjectAttributes, CustomGravity, KairosMagneticSystem
  Scripts/Player/      PlayerInputReader, PlayerMotor, PlayerRespawn, GauntletEvents, AimRigDriver
  Scripts/Level/       PressurePlate, Door, Checkpoint, KillZone, GoalTrigger, LevelManager
  Scripts/Feedback/    ForceVisual, HitStop, CameraImpulse, SfxPlayer, ParticleBursts
  Scripts/UI/          AbilityBarUI, CooldownRing, TargetReticle, ToastUI, ObjectiveUI
  Scripts/Events/      EventChannel<T> + concrete channels
  Scripts/Util/        Spring, Bootstrap, DebugOverlay
  ScriptableObjects/   Abilities/, Events/
  Prefabs/  Scenes/  Art/  Audio/  Shaders/  Animation/  Timeline/  Input/
```

## Assembly definitions
Optional. If used: one `Kairos.Runtime` asmdef and one `Kairos.Tests` (Play Mode tests). Do not split further.

## Dependency rules (enforced by review)
- `Core` and `Abilities` never reference `UI`, `Feedback` or `Level`.
- `UI`, `Feedback`, `Level` reference only `Events` and data types (`AbilityDefinition`, `ForceTarget` read-only).
- `Player` may reference `Core` through events and the controller's public read-only properties.
