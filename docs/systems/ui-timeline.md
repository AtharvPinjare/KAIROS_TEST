# System: UI and Timeline

Phases: cp7 (UI), cp8 (Timeline).

## UI (uGUI + TextMeshPro)
Canvas: Screen Space – Overlay, Canvas Scaler = Scale With Screen Size, reference 1920×1080, match 0.5. Everything updates **only through event channels**.

| Component | Shows | Listens to |
|---|---|---|
| `AbilityBarUI` | 4 slots: icon, key label (1–4), highlight on the selected slot, variant label (e.g. "Down") | `OnAbilitySelected`, `OnVariantChanged` |
| `CooldownRing` | Image with `Image.Type = Filled`, radial, `fillAmount` = remaining fraction | `OnCooldownChanged` |
| `TargetReticle` | Reticle around the targeted object plus its display name | `OnTargetChanged`, `OnTargetingStarted/Ended` |
| `ToastUI` | One-line message that fades after 1.5 s | `OnCastRejected`, `OnCheckpointReached` |
| `ObjectiveUI` | Current objective text | `OnObjectiveChanged` |

Reject-reason messages:
| Reason | Message |
|---|---|
| NoTarget | "Aim at an object" |
| CapabilityLocked | "This object resists the device" |
| NotMagnetic | "Not magnetic" |
| OnCooldown | "Device recharging" |

`TargetReticle` converts the target position with `Camera.WorldToScreenPoint` and follows it with the `Spring` utility; a scale "pop" on target change. Hide the system cursor when targeting if desired; keep the reticle anchored to the target, not the cursor.

Acceptance: UI scripts have no reference to `AbilityController` (events only); cooldown ring is smooth; layout holds at 16:9 and 16:10; no `GetComponent` or `Find` calls per frame.

## Timeline (cp8)
Package: Timeline. Two `PlayableDirector`s.

### Intro reveal (`Timeline_Intro`, ~7 s)
Tracks:
1. **Cinemachine Track** on the main camera's Brain: shot A (final door, 3 s) blending to shot B (player start, 3 s). Verify the CinemachineBrain is on the Main Camera.
2. **Activation Track**: title card object on at 0.5 s, off at 4 s.
3. **Audio Track**: ambient sting.
4. **Signal Track**: at the end a `IntroFinished` Signal Emitter.
`CutsceneController`: on start sets `PlayerInputReader.InputEnabled = false`, plays the director, and on the Signal (via `TimelineSignalBridge` → `OnIntroFinished` channel) re-enables input. **Skip:** any key while playing sets `director.time = duration` and calls `Evaluate()`.

### Final door (`Timeline_FinalDoor`, ~5 s)
Triggered by `OnFinalDoorOpened`. Tracks: Cinemachine (camera cuts to the door), Animation track on the door (opening), Audio track (alarm + hiss), Signal Track with emitters `DoorUnlock` (raises door event), `FlashScreen` (UI flash), then `LevelComplete` (raises `OnLevelComplete`).

### Signals
Create `SignalAsset`s (`IntroFinished`, `DoorUnlock`, `FlashScreen`, `LevelComplete`). A `SignalReceiver` component on the director's object maps each signal to a `UnityEvent` that calls `TimelineSignalBridge.Raise(string)`, which raises the matching `VoidEventChannel`. Teach: **Signals are Timeline's way of calling gameplay at an exact moment without hard-wiring.**

### HUMAN STEP
Timeline tracks, bindings and Signal assets are usually authored in the Editor. If the MCP has no Timeline tool group, write the exact click-by-click steps in `BUILD_LOG.md` and have the human build them once; students receive the finished Timeline in the checkpoint.

Acceptance: intro plays once, skippable; final timeline plays when the switch opens the door; input restored after both; no errors if either director is missing a binding (log a warning instead).
