# System: Ability state machine

Phases: cp2 (core), extended in cp3–cp4. Read `01_ARCHITECTURE.md` first.

## Files
`IAbilityState`, `AbilityStateMachine`, `AbilityController`, `ReadyState`, `TargetingState`, `ActiveState`, `CooldownState`, `AbilityContext`, `AbilityLoadout`, `TargetFinder`.

## AbilityStateMachine
```csharp
public class AbilityStateMachine
{
    public IAbilityState Current { get; private set; }
    public void Initialize(IAbilityState start) { Current = start; Current.Enter(); }
    public void ChangeState(IAbilityState next)
    {
        if (next == null || next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }
    public void Tick() => Current?.Tick();
}
```

## AbilityController (MonoBehaviour)
Responsibilities, and only these: build the four states, expose them, own the selected ability / variant / current target, run `TargetFinder`, build the `AbilityContext`, raise events, expose read-only state for UI/debug.

Serialized: `AbilityLoadout loadout` (array of 4 `AbilityDefinition`), `TargetFinder finder`, `Transform source` (gauntlet or player), `MagneticPolarity polarity = North`, `float cooldownDuration = 2`, event channels (see `events-data.md`).

Public surface:
```csharp
public AbilityDefinition Selected { get; }
public int Variant { get; }
public ForceTarget Target { get; }
public ReadyState Ready; public TargetingState Targeting; public ActiveState Active; public CooldownState Cooldown;
public void GoTo(IAbilityState s);
public void Select(int index);          // raises OnAbilitySelected
public void CycleVariant();             // wraps at Selected.VariantNames.Length
public void RefreshTarget();            // raises OnTargetChanged only when it changes
public CastRejectReason TryCast();      // validation only; no side effects
public void BeginEffect();              // calls Selected.Apply, raises OnEffectApplied
public void EndEffect();                // calls Selected.Revert, raises OnEffectReverted
public void ForceReset();               // used by respawn: revert if active, go to Ready
```
`TryCast()` order of checks: no target → `NoTarget`; on cooldown → `OnCooldown`; then `Selected.CanApply(ctx, target)`.

## States (the exact behaviour)
**ReadyState**
- Tick: if `PlayerInputReader.AbilitySelect >= 0` → `controller.Select(index)`; if `VariantPressed` → `CycleVariant()`; if `TargetPressed` → `GoTo(Targeting)`.

**TargetingState**
- Enter: raise `OnTargetingStarted`.
- Tick: `RefreshTarget()`; if `TargetReleased` → `GoTo(Ready)`; if `CastPressed` → `reason = TryCast()`; if `None` → `GoTo(Active)` else raise `OnCastRejected(reason)` and **stay** in Targeting.
- Selection keys and `VariantPressed` also work while targeting.
- Exit: raise `OnTargetingEnded`, clear highlight (`OnTargetChanged(null)`).

**ActiveState**
- Enter: `controller.BeginEffect()`; start timer = `Selected.Duration`; raise `OnCastStarted`.
- Tick: if `Selected.Duration <= 0` → immediately `GoTo(Cooldown)`; else decrement timer (using `Time.deltaTime`, **scaled**, so hit-stop pauses it) and `GoTo(Cooldown)` at ≤ 0. If the target was destroyed (`Target == null`) → `GoTo(Cooldown)` immediately.
- Exit: `controller.EndEffect()` (revert). **Exit must be safe to call from `ForceReset`.**

**CooldownState**
- Enter: `remaining = cooldownDuration`, raise `OnCooldownChanged(1)`.
- Tick: `remaining -= dt`; raise `OnCooldownChanged(remaining / cooldownDuration)` (normalized 1→0); at ≤ 0 → `GoTo(Ready)`.
- While in Cooldown, selection keys still change the selected ability (so the UI is responsive) but casting is blocked.

## Edge cases to test (Play Mode tests)
1. Cast, then respawn during Active → target restored, state Ready.
2. Cast on a target that is destroyed mid-effect → no exception, goes to Cooldown.
3. Rapid selection changes during Targeting → no stale highlight.
4. `ChangeState(current)` does nothing.
5. Reject does not consume cooldown.

## DebugOverlay (build in cp2)
`OnGUI` box (toggle F1): current state name, selected ability + variant, target name, cooldown remaining, active timer. This is the single most useful teaching tool for debugging state machines.

## TargetFinder
```csharp
[SerializeField] LayerMask targetableMask; [SerializeField] float maxRange = 15f; Camera cam;
public ForceTarget Find(Vector2 pointerScreenPos, Transform source)
{
    // ScreenPointToRay → Physics.Raycast(…, 100f, targetableMask)
    // GetComponentInParent<ForceTarget>(); reject if distance(source, target) on XY > maxRange
}
```
Cache `Camera.main` once. Do not read the mouse here: the pointer position arrives as a parameter from `PlayerInputReader`.
