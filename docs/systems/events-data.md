# System: ScriptableObject event channels and data

Purpose: decouple everything. UI, feedback, audio, level logic and Timeline all talk through channel assets, never through direct references to the ability code.

## Base types
```csharp
public abstract class EventChannel<T> : ScriptableObject
{
    public event System.Action<T> Raised;
    public void Raise(T value) => Raised?.Invoke(value);
}

public class VoidEventChannel : ScriptableObject
{
    public event System.Action Raised;
    public void Raise() => Raised?.Invoke();
}

[CreateAssetMenu(menuName = "Kairos/Events/Float")]
public class FloatEventChannel : EventChannel<float> { }
```
Unity cannot serialize open generics, so create one concrete subclass per payload: `FloatEventChannel`, `IntEventChannel`, `BoolEventChannel`, `StringEventChannel`, `AbilityEventChannel` (payload `AbilityDefinition`), `ForceTargetEventChannel`, `RejectEventChannel` (payload `CastRejectReason`).

Rules:
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. A missed unsubscribe leaks listeners (especially if domain reload is ever disabled).
- Channels carry no state; if a listener needs the last value, it asks the producer's read-only property.
- A channel asset lives in `Assets/_Kairos/ScriptableObjects/Events/` and is dragged into serialized fields.

## Channel catalogue
| Channel asset | Type | Raised by | Listened to by |
|---|---|---|---|
| OnAbilitySelected | AbilityEventChannel | AbilityController | AbilityBarUI, SfxPlayer |
| OnVariantChanged | IntEventChannel | AbilityController | AbilityBarUI (variant label / gravity arrow) |
| OnTargetChanged | ForceTargetEventChannel (null = none) | AbilityController | TargetReticle, ForceVisual |
| OnTargetingStarted / OnTargetingEnded | VoidEventChannel | TargetingState | TargetReticle, SfxPlayer |
| OnCastStarted | AbilityEventChannel | ActiveState | AnimatorBridge, SfxPlayer |
| OnGauntletPulse | VoidEventChannel | GauntletEvents (AnimationEvent) | ParticleBursts, CameraImpulse, HitStop, SfxPlayer |
| OnEffectApplied | ForceTargetEventChannel | AbilityController | ForceVisual, ParticleBursts, ObjectiveUI |
| OnEffectReverted | ForceTargetEventChannel | AbilityController | ForceVisual, SfxPlayer |
| OnCastRejected | RejectEventChannel | TargetingState | ToastUI, SfxPlayer |
| OnCooldownChanged | FloatEventChannel (1→0) | CooldownState | CooldownRing |
| OnPlateStateChanged_{id} | BoolEventChannel | PressurePlate | Door, ObjectiveUI |
| OnCheckpointReached | VoidEventChannel | Checkpoint | ToastUI, SfxPlayer |
| OnPlayerRespawn | VoidEventChannel | PlayerRespawn | AbilityController (ForceReset), LevelManager (prop reset), CameraImpulse |
| OnObjectiveChanged | StringEventChannel | LevelManager | ObjectiveUI |
| OnIntroFinished / OnFinalDoorOpened | VoidEventChannel | Timeline Signals / Door | CutsceneController, LevelManager |
| OnLevelComplete | VoidEventChannel | LevelManager | UI end screen |

## Data assets
- `AbilityLoadout` : ScriptableObject — ordered array of 4 `AbilityDefinition`.
- `AbilityDefinition` subclasses: assets under `ScriptableObjects/Abilities/` (Gravity, Mass, Magnetic, Friction).
- Optional: `FeelSettings` ScriptableObject (hit-stop duration, impulse strength, FOV kick) so juice is tuned in one place.

## Testing
A Play Mode test that raises each channel and asserts a listener received it; and a test that `ActiveState.Exit` raises `OnEffectReverted` exactly once.
