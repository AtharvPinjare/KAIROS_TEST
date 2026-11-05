# 04 — Build Plan (phases, acceptance criteria, tags)

Work through the phases **in order**. A phase is done only when every acceptance criterion passes, the console is clean, the commit is made and the tag exists. **HUMAN STEP** = ask the person to do it in the Editor with exact click-by-click instructions (see `CLAUDE.md`).

Each phase maps to a workshop block, so the tags double as student checkpoints. After each tag also export `Kairos_cpN.unitypackage`.

After finishing a phase do a **student-mode rehearsal**: in a fresh Claude chat with only the student Project Brief, try to redo the phase's main script with copy-paste. Record failures in `BUILD_LOG.md`. A phase is not "teachable" until that rehearsal succeeds.

---

## cp0 — Project setup
Goal: a clean, version-pinned project that compiles and talks to MCP.
- Create URP project on **6000.3.15f1**; set Active Input Handling to **Input System Package (New)**; install packages: Input System, Cinemachine, Animation Rigging, Timeline, TextMeshPro (uGUI), Test Framework, ProBuilder (optional).
- Folder layout from `01_ARCHITECTURE.md`; `.gitignore`; first commit; GitHub remote.
- Install and connect Unity MCP (`05_MCP_WORKFLOW.md`).
- Layers: Player, Ground, Targetable, Interactable.
- Physics: gravity (0, -9.81, 0); note fixed timestep.
Acceptance: project opens with no console errors; MCP can read the console and create a test object; git pushed; tag `cp0`.

## cp1 — Player, camera, Room 1 greybox
Goal: a character that moves and jumps in the 2.5D plane through a greybox Room 1.
Files: `PlayerInputReader`, `PlayerMotor`, `PlayerRespawn`, `KillZone`, `Checkpoint`, `Spring` (utility), `Bootstrap`.
- `PlayerMotor`: Rigidbody, velocity-driven horizontal movement, jump with **coyote time (0.1 s)** and **jump buffer (0.1 s)**, constraints per conventions.
- Cinemachine 3 side-view camera following the player with a small deadzone.
- Greybox Room 1 per `03_LEVEL_SPEC.md`; Room 2 and 3 can stay empty.
Acceptance: jump apex measures 2.2 ± 0.1 m and distance ≈ 8 m (log them once with a test); player cannot reach the ledge directly; respawn with `T` works; tag `cp1`.

## cp2 — Ability state machine + Gravity Shift
Goal: Room 1 is solvable.
Files: `IAbilityState`, `AbilityStateMachine`, `AbilityController`, `ReadyState`, `TargetingState`, `ActiveState`, `CooldownState`, `AbilityContext`, `AbilityDefinition`, `GravityShiftAbility`, `AbilityLoadout`, `TargetFinder`, `ForceTarget`, `ObjectAttributes`, `CustomGravity`, `DebugOverlay`, event channels (at least `AbilityEventChannel`, `ForceTargetEventChannel`, `FloatEventChannel`, `RejectEventChannel`).
Acceptance:
- State label on `DebugOverlay` cycles Ready → Targeting → Active → Cooldown → Ready.
- Gravity on the pod flips for 12 s then **reverts** (pod returns to the ceiling).
- Rejected cast (no target / locked) leaves state at Targeting and raises `OnCastRejected` with the right reason.
- `ForceReset()` on respawn restores the pod.
- Room 1 solvable by the documented solution; tag `cp2`.

## cp3 — Mass Control + force receiver contract + Room 2
Files: `MassControlAbility`, `PressurePlate`, `Door`, `GoalTrigger`, `BrittleBarrier`, `LevelManager` (prop reset), doors wired to channels.
Acceptance: block mass snaps to 0.5 on cast and restores to 10 on revert; plate triggers only at mass ≥ 8; door opens on plate; both Room 2 solutions work; mass clamps enforced; tag `cp3`.

## cp4 — Magnetism + Friction + Room 3
Files: `MagneticPulseAbility`, `KairosMagneticSystem`, `FrictionFieldAbility`, runtime `PhysicsMaterial` instancing in `ForceTarget`, hard stop, switch plate, final door (no animation yet).
Acceptance: bridge aligns against the hard stop in one pull from the documented position; crate does **not** slide at normal friction but slides across and presses the switch when slippery; non-magnetic object rejects magnetism with `NotMagnetic`; shared friction asset is not modified; the full level is solvable start to finish; tag `cp4`.
Playtest: three people complete the greybox level; log where they got stuck.

## cp5 — Feedback layer
Files: `ForceVisual` (MaterialPropertyBlock driver), `HitStop`, `CameraImpulse`, `SfxPlayer`, `ParticleBursts`, AudioMixer with a `Pulse` snapshot.
**HUMAN STEP:** build the Force Field Shader Graph per `systems/feedback-layer.md` (Claude Code cannot reliably author it).
- Swap greybox for free art (Kenney/Quaternius) now.
Acceptance: each cast/revert has shader + particle + sound + camera response; hit-stop is ≤ 0.08 s and never freezes UI; shader properties are driven from events, not from ability classes; tag `cp5`.

## cp6 — Animation, events, procedural response
Files: `GauntletEvents`, `AimRigDriver`, Animator Controller (player locomotion + gauntlet cast), door Animator.
**HUMAN STEP (if MCP animation/rigging tools unavailable):** create the Rig, the Multi-Aim constraint and the gauntlet clip with its AnimationEvent per `systems/animation-procedural.md`.
Acceptance: the **feedback pulse** (particles, camera impulse, hit-stop, sound) fires **from the AnimationEvent**, not from the click, while the gameplay effect still applies at click (documented design decision in `systems/animation-procedural.md`); aim constraint turns the gauntlet toward the target with smoothing; camera has a spring-damped recoil on cast; Animator FSM visible in the Animator window; tag `cp6`.

## cp7 — UI
Files: `AbilityBarUI`, `CooldownRing`, `TargetReticle`, `ToastUI`, `ObjectiveUI`.
Acceptance: UI updates only through event channels; cooldown ring fills smoothly; each reject reason shows its own message; reticle follows the target with a spring; UI works at 16:9 and 16:10; tag `cp7`.

## cp8 — Timeline
**HUMAN STEP (if no MCP Timeline tools):** build the two timelines per `systems/ui-timeline.md`.
Files: `CutsceneController` (disables `PlayerInputReader.InputEnabled`), `TimelineSignalBridge`.
Acceptance: intro reveal plays on start and is skippable; final door sequence is triggered by the switch channel; Signal Emitters raise real game events (sound, UI, door); input is restored afterwards; tag `cp8`.

## cp9 — Integration and hardening
- Wire objective text, checkpoints, prop reset, restart, end screen.
- Edge cases: respawn during Active, cast on destroyed target, rapid ability switching, pause/resume, window focus loss.
- Performance pass: no per-frame allocations (Profiler), stable 60 FPS on a mid laptop.
Acceptance: full playthrough 3 times without a console error; tag `cp9`.

## cp10 — Build and showcase
- Standalone build (Windows; macOS if needed) from a **Build Profile**; run the build and complete the level.
- Write `README.md` for the repo, export `Kairos_cp10.unitypackage`.
Acceptance: build runs outside the Editor and the level is completable; tag `cp10`.

---

## Workshop artefacts to produce alongside (not separate phases)
- **`cp-debuglab`**: a copy of cp9 with the seeded bugs from the student pack's `03_Debug_Lab.md` (use a branch `debuglab`).
- **`cp-capstone`**: the fifth ability (Time Dilation) built by adding only `TimeDilationAbility.cs` + one asset, proving the architecture. If it requires editing anything else, record it in `BUILD_LOG.md` under "Architecture proposals".
- **Stretch (only if asked):** player gravity flip, drone, WebGL build.

## Kick-off prompt for each phase
> Read CLAUDE.md and the docs listed for this phase. Implement phase `cpN` from `docs/04_BUILD_PLAN.md`. Follow the working loop. After each script compile and read the console. Stop at the acceptance criteria, show me the evidence, update `docs/BUILD_LOG.md`, commit and tag.
