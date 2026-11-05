# System: Input, player motor, camera, level objects

Phase: cp1 (input, motor, camera), cp3–cp4 (level objects).

## PlayerInputReader (the only input reader)
Uses the **new Input System** with direct device polling (no `.inputactions` asset: simpler for students and for chat-generated code). Project Settings → Player → Active Input Handling = Input System Package (New).

```csharp
using UnityEngine.InputSystem;
public class PlayerInputReader : MonoBehaviour
{
    public bool InputEnabled { get; set; } = true;
    public float Move { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public int  AbilitySelect { get; private set; } = -1;     // 0..3 or -1
    // In the real file make each of these a `{ get; private set; }` property (shown compactly here):
    public bool TargetHeld, TargetPressed, TargetReleased, CastPressed, VariantPressed, RespawnPressed, DebugTogglePressed;
    public Vector2 PointerScreenPosition { get; private set; }

    void ClearAll()   // zero every value so a disabled reader reports "no input"
    {
        Move = 0f; JumpPressed = JumpHeld = false; AbilitySelect = -1;
        TargetHeld = TargetPressed = TargetReleased = CastPressed = VariantPressed = RespawnPressed = DebugTogglePressed = false;
    }

    void Update()
    {
        var kb = Keyboard.current; var mouse = Mouse.current;
        if (kb == null || mouse == null || !InputEnabled) { ClearAll(); return; }
        Move = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        JumpPressed = kb.spaceKey.wasPressedThisFrame; JumpHeld = kb.spaceKey.isPressed;
        AbilitySelect = kb.digit1Key.wasPressedThisFrame ? 0 : kb.digit2Key.wasPressedThisFrame ? 1 :
                        kb.digit3Key.wasPressedThisFrame ? 2 : kb.digit4Key.wasPressedThisFrame ? 3 : -1;
        TargetPressed = mouse.rightButton.wasPressedThisFrame; TargetReleased = mouse.rightButton.wasReleasedThisFrame;
        TargetHeld = mouse.rightButton.isPressed; CastPressed = mouse.leftButton.wasPressedThisFrame;
        VariantPressed = kb.rKey.wasPressedThisFrame; RespawnPressed = kb.tKey.wasPressedThisFrame;
        DebugTogglePressed = kb.f1Key.wasPressedThisFrame;
        PointerScreenPosition = mouse.position.ReadValue();
    }
}
```
Ordering: `PlayerInputReader.Update` must run **before** `AbilityController.Update` so the controller never reads last frame's input. Set a negative Script Execution Order on the reader (for example -100) or add `[DefaultExecutionOrder(-100)]` to the class.

## PlayerMotor (cp1)
- Rigidbody on `Player` layer; constraints: freeze Position Z, freeze Rotation X/Y/Z; capsule collider with a zero-friction `PhysicsMaterial` on the player so it does not stick to walls.
- Horizontal: set `linearVelocity.x = Move * moveSpeed` (6 m/s) with a small acceleration smoothing.
- Jump: `jumpSpeed = Mathf.Sqrt(2f * 9.81f * 2.2f)`; **coyote time 0.1 s** and **jump buffer 0.1 s**; shorter hop on release (cut upward velocity by 50% when `JumpHeld` ends while rising).
- Ground check: `Physics.SphereCast` or `CheckSphere` from the feet against `Ground | Targetable` layers.
- Mass 1; the player's push strength is intentionally limited by a high-mass block (see Room 2 playtest risk).
- Expose `Grounded` and `VelocityX/Y` for the Animator.

## PlayerRespawn
Stores the last checkpoint position; on `RespawnPressed` or a `KillZone` hit: reset velocity, teleport, raise `OnPlayerRespawn`. The `AbilityController` listens and calls `ForceReset()`; `LevelManager` resets props.

**As built in cp1:** `PlayerRespawn` implements `IRespawnable` (`Scripts/Core`). `Checkpoint` and `KillZone` find the player with `other.GetComponentInParent<IRespawnable>()`, so Level code never references the Player assembly folder. `Checkpoint` always updates the respawn point on entry but raises `OnCheckpointReached` only the first time.

**PlayerMotor as built in cp1:** the jump press is buffered in `Update` (so a press between two physics steps is not lost); every timer (coyote, buffer) counts down in `FixedUpdate`; "grounded" is ignored while `isJumping && velocity.y > 0` so the coyote timer cannot refill during takeoff. Measured at 50 Hz: apex 2.135 m, flat distance 7.92 m (see `BUILD_LOG.md`, cp1).

## Camera (Cinemachine 3)
- One `CinemachineCamera` follows the player with a `CinemachinePositionComposer` (camera distance ~14, small dead zone, slight look-ahead); main camera has a `CinemachineBrain`.
- Side view: rotation (0, 0, 0), perspective FOV ~40.
- Optional confiner per room. Verify component names against the installed Cinemachine package (3.x renamed many 2.x components).

## Level objects (small, event-driven)
| Component | Behaviour |
|---|---|
| `PressurePlate` | Trigger collider. Tracks Rigidbodies inside (`OnTriggerEnter/Exit`), sums mass, compares to `threshold`, raises a `BoolEventChannel` only on **change**. Re-evaluates when mass changes (call from `FixedUpdate` with a cheap sum while any body is inside) so the Room 2 revert trick works. |
| `Door` | Listens to a `BoolEventChannel`; sets Animator `Open`; disables its collider when open. |
| `GoalTrigger` | Fires once when the player enters, raising a `VoidEventChannel`. |
| `Checkpoint` | Trigger; sets the respawn point; raises `OnCheckpointReached` once. |
| `KillZone` | Trigger below the level; requests respawn. |
| `BrittleBarrier` | `OnCollisionEnter`: if `collision.rigidbody.mass >= minMass` and relative speed ≥ `minSpeed`, deactivate with a break burst. |
| `LevelManager` | Stores initial state of every `ForceTarget` on `Start`; on `OnPlayerRespawn` calls `ResetToInitial()`; raises `OnObjectiveChanged` as rooms are completed. |
