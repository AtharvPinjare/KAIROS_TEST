# System: Animation, animation events, procedural response

Phase: cp6. Teaches the Animator as a **second state machine**, AnimationEvents as a sync mechanism, and procedural animation (aim constraint + springs).

## Design decision (document and teach it)
The gameplay effect applies **at click**, so gameplay stays deterministic. The cast **animation** and its **AnimationEvent** drive the *feedback pulse* (particles, camera impulse, hit-stop, sound). The event is set early in the clip (~0.1 s) so the desync is imperceptible. In a production game you might gate the effect on the event; we explain why we did not.

## Animator Controller (player)
Parameters: `Speed` (float), `Grounded` (bool), `VerticalSpeed` (float), `Cast` (trigger), `Targeting` (bool).
Base layer states: `Idle`, `Run` (blend tree on `Speed`), `Jump`, `Fall` → transitions by `Grounded`/`VerticalSpeed`. Upper-body layer (avatar mask on the arm/gauntlet, weight 1): `Empty` ↔ `Aim` (while `Targeting`) → `Cast` (on trigger) → back.
Show the Animator window to students as a state machine identical in structure to the ability FSM.

## AnimatorBridge
Listens to `OnTargetingStarted/Ended` and `OnCastStarted` channels and sets `Targeting` / `Cast`. No gameplay code touches the Animator directly.

## Gauntlet clip and event
If no suitable clip exists, **author your own**: select the gauntlet object, Animation window, record a 0.5 s clip (rotate the arm up 30°, scale the glow object 1 → 1.4 → 1) and add an **Animation Event** at 0.1 s calling `GauntletEvents.OnPulse`.
```csharp
public class GauntletEvents : MonoBehaviour   // must sit on the same GameObject as the Animator
{
    [SerializeField] VoidEventChannel onGauntletPulse;
    public void OnPulse() => onGauntletPulse.Raise();   // called by the AnimationEvent
}
```
Pitfall to log in BUILD_LOG: the event method must be public, on a component of the **Animator's** GameObject, and spelled exactly as in the clip.

## Door animation
Animator with `Open` bool; `Door` sets it from the plate/switch channel. A second tiny state machine (Closed ↔ Opening ↔ Open).

## Procedural animation 1 — aim constraint (**HUMAN STEP** if MCP rigging tools unavailable)
Package: Animation Rigging. Steps: add `Rig Builder` to the character, a child `Rig` object with weight 1, a `Multi-Aim Constraint` (Constrained Object = gauntlet/hand bone, Source Object = `AimTarget` transform, Aim Axis = the gauntlet's forward). `AimRigDriver`:
- Listens to `OnTargetChanged`. Moves `AimTarget` to the target's position with a spring (`Spring`, 8–12 Hz).
- Drives `constraint.weight` toward 1 while Targeting, 0 otherwise, also springed.
Acceptance: the gauntlet visibly tracks the target with smoothing; releases to neutral on cancel.

## Procedural animation 2 — springs everywhere (code-only, cheap, high impact)
```csharp
public struct Spring
{
    public float Value, Velocity;
    public float Update(float target, float frequency, float dt)   // critically damped
    {
        float x = Value - target;
        float temp = (Velocity + frequency * x) * dt;
        float e = Mathf.Exp(-frequency * dt);
        Value = target + (x + temp) * e;
        Velocity = (Velocity - frequency * temp) * e;
        return Value;
    }
}
```
Use it for: camera FOV kick on pulse, reticle follow, ability bar slot "pop", player landing squash.

## Acceptance (cp6)
- Pulse feedback fires from the AnimationEvent (verify by temporarily removing the event: no feedback).
- Animator FSM visible and understandable in the Animator window.
- Aim constraint smooth; no popping when targets change.
- Springs implemented once, reused 3+ times.
