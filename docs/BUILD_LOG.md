# BUILD_LOG

Write entries as you work. This log is the raw material for the student prompt cards and the Debug Lab, so be specific: paste the exact prompt, the exact error, the exact fix.

## Environment (fill at cp0)
- Unity: 6000.3.15f1
- Packages and versions (from `Packages/manifest.json`): URP 17.3.0, Input System 1.19.0, Cinemachine 3.1.7, Animation Rigging 1.4.1, Timeline 1.8.12, uGUI 2.0.0 (carries TextMeshPro in Unity 6), Test Framework 1.6.0, AI Navigation 2.0.12. ProBuilder not installed (optional). Shader Graph is present in the package cache as a URP dependency.
- Unity MCP version / pinned tag: `com.coplaydev.unity-mcp` pinned to `#v10.0.0` (git URL in manifest). Note: `docs/05_MCP_WORKFLOW.md` quotes the v10.3.0 README, so tool names may differ slightly from that doc.
- MCP tools visible to Claude Code: see "MCP tools" below.
- Tool groups enabled: `core` (default), `testing` (activated at cp0 via `manage_tools activate`; `run_tests` and `get_test_job` are now callable) and `docs` (activated at cp1: `unity_reflect`, `unity_docs`). `execute_code` also answered at cp1 although its group is listed as disabled below. Disabled: animation, asset_gen, probuilder, profiling, ui, vfx.
- Active Input Handling: `activeInputHandler: 1` (Input System Package, New) in `ProjectSettings/ProjectSettings.asset`. Left untouched.
- Physics: `Physics.gravity` = (0, -9.81, 0) in `ProjectSettings/DynamicsManager.asset`. Fixed timestep = 0.02 s (50 Hz), the Unity default. Jump-apex and coyote-time measurements in cp1 assume this; if the timestep changes, re-measure.
- Layers added at cp0 (TagManager slots): Player 8, Ground 9, Targetable 10, Interactable 11. `Ignore Raycast` is built in (slot 2).

## MCP tools
Smoke test (cp0): `manage_gameobject create MCP_Test` → seen in `manage_scene get_hierarchy` → `manage_gameobject delete` → `find_gameobjects` returned 0. `read_console` (errors and warnings) returned 0 entries.

Tools announced to the session: 48 on first connect, 46 after one reconnect (the two missing names were not identified). Only the `core` group is reported enabled by `manage_tools`; I exercised only tools marked **tested**.

| Group | Tools | Status |
|---|---|---|
| core | batch_execute, execute_menu_item, find_gameobjects, find_in_file, manage_asset (**tested**: create_folder), manage_build, manage_camera, manage_components, manage_editor (**tested**: add_layer), manage_gameobject (**tested**), manage_graphics, manage_material, manage_packages, manage_physics, manage_prefabs, manage_scene (**tested**: get_hierarchy), refresh_unity, apply_text_edits, create_script, delete_script, validate_script, manage_script, get_sha, read_console (**tested**), script_apply_edits | enabled |
| animation | manage_animation (Animator control, AnimationClip creation) | disabled |
| docs | unity_docs, unity_reflect | disabled |
| scripting_ext | execute_code, manage_scriptable_object | disabled |
| testing | run_tests, get_test_job | disabled |
| ui | manage_ui (UI Toolkit only; KAIROS uses uGUI) | disabled |
| vfx | manage_shader, manage_texture, manage_vfx | disabled |
| profiling | manage_profiler | disabled |
| probuilder | manage_probuilder (needs package) | disabled |
| asset_gen | generate_image, generate_model, generate_audio, import_model, import_model_file | disabled |
| meta / other | manage_tools, set_active_instance, manage_script_capabilities, debug_request_context, blender_bridge | n/a |

### Timeline, Animation Rigging, Shader Graph
| Feature | Dedicated MCP tool? | Decision |
|---|---|---|
| Timeline | No. | **HUMAN STEP** (cp8), per CLAUDE.md. `execute_code` could script the Timeline API but is untested and disabled; revisit only if the human asks. |
| Animation Rigging | No. | **HUMAN STEP** (cp6) for Rig Builder and Multi-Aim bindings. |
| Shader Graph | No. `manage_shader` writes shader code, not node graphs. | **HUMAN STEP** (cp5), spec in `systems/feedback-layer.md`. |
| Animator Controller / clips (cp6) | `manage_animation`, after enabling the `animation` group. | Drivable in principle; untested. |

Groups to enable later: `scripting_ext` (ScriptableObject assets, cp2), `testing` (Play Mode tests, cp1+), `docs` (API checks, from cp1), `animation` (cp6), `profiling` (cp9).

## Entry template (copy for each phase / notable event)

### cpN — <phase name> — <date>
**Prompt that worked (paste verbatim):**
```
```
**First attempt result:** (compiled? behaved? what was wrong)
**Bug or failure:** (exact console error / observed behaviour)
**Root cause:**
**Fix (and the prompt that produced it):**
**API drift found:** (old API → new API, so we can extend CLAUDE.md's table)
**Teaching value (1-5) and why:**
**Student-mode rehearsal:** did the chat-only version succeed? What context was missing from the Project Brief?
**Acceptance criteria evidence:** (test names, screenshots, measurements)

### cp1 — Player, camera, Room 1 greybox — 2026-11-05
**Prompt that worked (paste verbatim):**
```
Read CLAUDE.md, docs/04_BUILD_PLAN.md, docs/systems/input-camera.md and docs/03_LEVEL_SPEC.md. Implement phase cp1 (player, camera, Room 1 greybox). Follow the working loop: after each script, wait for compile and read the console. Do not stack several script changes before checking for errors. Stop at the acceptance criteria and show me the evidence, including the measured jump apex and distance from a Play Mode test. Update docs/BUILD_LOG.md and commit with cp1: ... messages, but do not tag until I review. If you hit a HUMAN STEP, stop and give me click-by-click instructions.
```
**First attempt result:** All 7 runtime scripts plus KLog, IRespawnable, VoidEventChannel compiled with 0 errors, one at a time. The Editor builder (`Tools/Kairos/Build Room 1`) built the scene on the first run. No HUMAN STEP was needed in cp1.
**Bug or failure (4 things, all mine):**
1. `CreateAsset() should not be used to create a file of type 'physicsMaterial'` when the builder saved the player's friction material as `PlayerNoFriction.physicsMaterial`.
2. First Play Mode run reported `status: succeeded, total: 0`. A run that discovers nothing reports **Passed**. The test list (`mcpforunity://tests/PlayMode`) showed only the root node `Kairos_Test`.
3. `T_Key_Returns_Player_To_Checkpoint` failed with `OnPlayerRespawn should be raised exactly once. Expected: 1 But was: 2`.
4. `mcpforunity://editor/state` and `unity_reflect` briefly answered "No Unity Editor instances found" (one call, recovered on the next call; no retry loop needed).
**Root cause:**
1. Unity wants the `.asset` extension for programmatic `CreateAsset` of a PhysicsMaterial.
2. The test asmdef had `includePlatforms: ["Editor"]`, which makes Unity treat the assembly as an Edit Mode assembly, so the Play Mode runner skipped it.
3. NUnit uses ONE fixture instance for every test in the class. The `raised` counter was an instance field, so the KillZone test's raise (1) leaked into the T-key test (2). The game code was correct. My first theory (InputTestFixture's immediate input update shows "pressed this frame" twice) was wrong; trying `queueEventOnly` did not change the result, a per-frame log showed exactly one raise in isolation, and that is how the shared field was found. That experiment was reverted.
4. The MCP bridge reconnects after a domain reload; the console also prints "Port 6400 not released yet after reload; retrying same port" after reloads (harmless, it is an MCP notice, not a project warning).
**Fix (and the prompt that produced it):** `.asset` extension; asmdef with no platform restriction and the one editor-only call (`EditorSceneManager.LoadSceneAsyncInPlayMode`) behind `#if UNITY_EDITOR`; `raised = 0` at the start of each test. No new prompt was needed; each fix was one edit followed by a compile.
**API drift found:** none against the CLAUDE.md table. Verified with `unity_reflect` before use: `CinemachineCamera.Lens` / `.Follow`, `CinemachinePositionComposer` (`CameraDistance`, `TargetOffset`, `Composition.DeadZone.Enabled/Size`, `Lookahead`), `PhysicsMaterial`, `Rigidbody.linearVelocity`, `InputTestFixture` lives in assembly `Unity.InputSystem.TestFramework`. New fact for CLAUDE.md: a Play Mode test asmdef must NOT list `Editor` in `includePlatforms`.
**Teaching value (1-5) and why:** 4. Velocity-driven motor, coyote time and jump buffer are the classic "game feel" lesson, and the Play Mode measurement test (apex and distance from the real physics step) shows students how to turn a design number into a pass/fail.
**Student-mode rehearsal:** NOT DONE. It needs a fresh Claude chat with only the student Project Brief and cannot be run from this session. `PlayerMotor` (125 lines) is the script to rehearse. cp1 is not "teachable" until this is done.
**Acceptance criteria evidence:**

| Criterion | Evidence (Play Mode test, 50 Hz fixed step, 10/10 tests pass: 9 acceptance tests plus 1 wall-overlap pin, re-run after the wall investigation) |
|---|---|
| Jump apex 2.2 ± 0.1 m | `Jump_Apex_Is_2_2m`: **2.135 m** (takeoff speed 6.570 m/s). In range but at the low end, see Open questions |
| Distance ≈ 8 m | `Jump_Distance_Is_About_8m`: **7.920 m**, air time 1.320 s, running at 6 m/s |
| Cannot reach the ledge directly | `Player_Cannot_Reach_Exit_Ledge_Directly`: 8 run-and-jump attempts, best feet height **2.13 m** (ledge top 3.6), never got onto the ledge |
| Respawn with `T` | `T_Key_Returns_Player_To_Checkpoint`: from x = 7.96 back to x = 1.00; `OnPlayerRespawn` raised once |
| KillZone respawn | `KillZone_Returns_Player_To_Checkpoint`: from y = -7.5 back to (1.00, 0.01); raised once |
| Coyote time 0.1 s | `Coyote_Jump_Works_...` (0.04 s after the edge: jumped, 6.37 m/s) and `Coyote_Jump_Fails_...` (0.16 s: no jump, -2.16 m/s) |
| Jump buffer 0.1 s | `Buffered_Jump_Fires_On_Landing` (≈0.07 s early: jumped) and `Early_Press_Is_Forgotten_...` (≈0.19 s early: no jump) |
| Console clean | 0 errors, 0 warnings from the project after the final compile and a live Play Mode run (only MCP bridge info lines) |
| Camera | `docs/evidence/cp1_room1_start.png`: Cinemachine 3 side view, distance 14, FOV 40 |

Files added: `Scripts/{Kairos.Runtime.asmdef, Core/IRespawnable, Events/VoidEventChannel, Player/{PlayerInputReader,PlayerMotor,PlayerRespawn}, Level/{Checkpoint,KillZone}, Util/{KLog,Spring,Bootstrap}}`, `Editor/{BuildUtil,PlayerRigBuilder,Room1Builder}`, `Tests/{Kairos.Tests.asmdef,PlayLevelFixture,PlayerMotorTests,PlayerRespawnTests,PlayerJumpFeelTests,WallContactDiagnosticTests}`, scene `Scenes/Level_ContainmentWing.unity`, two `VoidEventChannel` assets. Re-create the scene any time with menu `Tools/Kairos/Build Room 1`.
Not verified in cp1: `Spring` has no test yet (it is unused until cp6/cp7); no manual keyboard playtest by a human (all input in tests is injected through the Input System test fixture).

### cp2 — Ability state machine + Gravity Shift — 2026-11-05
**Prompt that worked (paste verbatim):**
```
Read CLAUDE.md, then the latest entries in docs/BUILD_LOG.md (open questions
and architecture proposals), then docs/04_BUILD_PLAN.md phase cp2, then
01_ARCHITECTURE.md , systems/abilityfsm.md , systems/forcetargets.md , systems/eventsdata.md , Room 1 in
03_LEVEL_SPEC.md .
Implement phase cp2 following the working loop in CLAUDE.md:
- After every script, wait for compile and read the console. Never stack
script changes.
- A test run that finds zero tests is a failure, not a pass. Show me the test
count.
- Do not change ProjectSettings without telling me first and reading the
current value.
- Prefer an Editor menu script (Tools/Kairos/...) for building scenes so they
can be rebuilt.
- One class per file, each file under about 150 lines.
- Commit in single-purpose commits named `cp2: ...`. Do not tag.
- If a step needs a HUMAN STEP, stop and give me click-by-click instructions.
Stop at the acceptance criteria. Report each criterion with a measured value,
list anything you changed that I did not ask for, and list your open
questions.
```
(The file names in the prompt lost their hyphens; the real files are `ability-fsm.md`, `force-targets.md`, `events-data.md`.)
**First attempt result:** Nothing was built in the first session: the `UnityMCP` server showed `CONNECTION_CLOSED` while a fresh `claude mcp list` said "Connected". The human reconnected it with `/mcp`, then the phase ran. About 35 scripts compiled one at a time, 0 errors left at the end; `Tools/Kairos/Build Room 1` rebuilt the scene with the pod, controller and overlay. No HUMAN STEP was needed.
**Bug or failure (all mine unless noted):**
1. `error CS1729: 'RequireComponent' does not contain a constructor that takes 4 arguments` (`ForceTarget`). The spec lists four types.
2. `error CS1061: 'AbilityController' does not contain a definition for 'SelectFromInput'` once: `create_script` started a compile before my controller edit (sent in the same batch) had landed. A second compile passed.
3. `AbilityController` reached 175 lines (limit ~150).
4. First test run: after the click the state was Ready, not Active. `rightPressed=False` right after pressing the left button.
5. After switching to whole mouse-state events: `held=True pressed=False` (the cast never fired).
6. `Gravity Shift lasts 12 s. Expected 12.0 +/- 0.3, was 12.35` when the Unity window was in the background.
7. `Room1_Is_Solvable...: could not climb onto the pod` (player ended on the floor at x = 8.65, pushing the pod).
8. Found while writing the reject tests: `TryCast` had no `OnCooldown` check.
9. MCP test runs stalled ("tests did not start within timeout", job stays "running", blocks `run_tests` with `tests_running`), leaving `Assets/InitTestScene<guid>.unity` behind.
**Root cause:**
1. `RequireComponent` takes at most 3 types.
2. Race between `create_script` auto-compile and a later edit in the same batch.
3. States, events and effect memory all lived in the controller.
4. `InputTestFixture.Press(button)` writes the byte that holds all mouse buttons, so pressing left released right (a harness quirk; a real mouse sends full state).
5. I called `InputSystem.Update()` myself; in `ProcessEventsInDynamicUpdate` mode that advances the input frame, so `wasPressedThisFrame` was already false when `PlayerInputReader.Update` ran.
6. The test measured real time; the timer sums `Time.deltaTime`, which a throttled background editor clamps. Measured with `Time.time` inside the event callbacks it is 12.002 s.
7. The motor has a variable-height jump. The test released Space after 2 steps and the rise was cut from 6.37 m/s to 2.79 m/s.
8. I wrote the check list from memory instead of from the spec order.
9. The MCP test runner's "No Throttling" step writes `runInBackground` (ProjectSettings.asset) and `m_EnterPlayModeOptions` (EditorSettings.asset). Unity treats ProjectSettings.asset as "script compilation related", so a recompile started right after "Entering Playmode" and interrupted the run (Editor.log: `Requested script compilation because: Assetdatabase observed changes in script compilation related files`). The interrupted run never restores the values.
**Fix (and the prompt that produced it):**
1. `[RequireComponent(typeof(Collider), typeof(ObjectAttributes), typeof(CustomGravity))]`; the Rigidbody comes from `CustomGravity`. Spec updated.
2. Order the dependency edit before the script that uses it, then force a compile.
3. Extracted `AppliedEffect` (what was applied to what) and moved the "raise only when the target changes" logic into `AbilityChannels`: 175 to 150 lines.
4+5. `AbilityTestFixture` sends whole `MouseState` events (`WithButton(MouseButton.Right, ..).WithButton(MouseButton.Left, ..)`) with `QueueStateEvent` only, no manual `InputSystem.Update()`.
6. Timestamps taken inside `OnCastStarted` / `OnEffectReverted` callbacks; keep the Unity window in the foreground while tests run.
7. Hold Space for 30 steps in the test.
8. Added `if (machine.Current == Cooldown) return OnCooldown;` between the two checks.
9. Recovery: `manage_editor stop`, load `Level_ContainmentWing`, delete the stray `InitTestScene*.unity`, then `refresh_unity` with `compile=request, mode=force` (log line "Job orphaned after domain reload" clears the stuck job), wait for the editor to settle, run again. Once `runInBackground` is already 1 the flip no longer happens and runs start normally. Not changed by me; see Open questions.
**API drift found:** `RequireComponent` has at most 3 type arguments (spec fixed). Confirmed compile in 6000.3: `PhysicsMaterial`, `PhysicsMaterialCombine`, `Rigidbody.linearVelocity`, `MouseState.WithButton(MouseButton, bool)`. New fact for CLAUDE.md: Play Mode input helpers: queue events, never call `InputSystem.Update()` yourself; `Press` on one mouse button can release another.
**Teaching value (1-5) and why:** 5. The Ready/Targeting/Active/Cooldown machine with `Exit` that always reverts is the clearest "why a state machine" lesson, and the `DebugOverlay` makes it visible. The reject-stays-in-Targeting rule and the shared-asset-plus-memento pattern (`Remember`/`Recall`) are both good prompt cards.
**Student-mode rehearsal:** NOT DONE (needs a fresh Claude chat with only the student Project Brief). Scripts to rehearse: `AbilityStateMachine` (31 lines), `ActiveState` (41 lines), `GravityShiftAbility` (51 lines).
**Acceptance criteria evidence:** 28 Play Mode tests, 28 passed, 0 failed, 0 skipped (10 from cp1 + 18 new), run with assembly filter `Kairos.Tests`.

| Criterion | Evidence |
|---|---|
| State label cycles Ready, Targeting, Active, Cooldown, Ready | `State_Label_Cycles_Ready_Targeting_Active_Cooldown_Ready`: sequence `ReadyState > TargetingState > ActiveState > CooldownState > ReadyState`, `DebugOverlay.BuildText()` checked at each stage. Active **12.002 s**, cooldown **2.009 s**, 121 ring events from 1.00 down to 0.00 (monotonic) |
| Gravity flips for 12 s then reverts | `Gravity_Flips_For_12s_Then_Pod_Returns_To_Ceiling`: 2 s after cast pod at **(9.00, 1.00), top 2.00**, gravity Down; at expiry gravity Up and enabled; 2 s later pod **y = 7.00**; `OnEffectReverted` raised exactly **1** time |
| Reject leaves state at Targeting, right reason, no cooldown | `Cast_With_No_Target...`: reasons `[NoTarget]`, state TargetingState, cooldown 0. `Cast_On_Locked_Target...`: `[CapabilityLocked]`, TargetingState, cooldown 0, then unlock + click goes to Active with no further reject. `Cast_Attempt_During_Cooldown...`: `TryCast()` = `OnCooldown` |
| `ForceReset()` on respawn restores the pod | `ForceReset_On_Respawn_During_Active_Restores_Pod`: pod on floor, `T` pressed: state ReadyState, gravity (0, 1, 0) enabled, target null, active timer 0, reverts raised **1**, pod back at y = 7.00 |
| Room 1 solvable by the documented solution | `Room1_Is_Solvable_By_The_Documented_Solution` with injected input only: pod settles at (9.00, 1.00); player climbs onto it (x = 8.56, feet y = 2.00); hop from x = 8.46; on the ledge at **x = 14.13, feet y = 3.60**, grounded, effect still Active with **7.2 s left**. The R1-R2 door is still a solid placeholder (door and `GoalTrigger` are cp3) |
| Console clean | 0 errors and 0 warnings from the project after the final compile and the full run (only Test Framework info lines) |
| Files under ~150 lines | largest: `AbilityController` 150, `PlayerMotor` 125 (cp1), `ForceTarget` 112 |

Other edge cases covered: target destroyed mid-effect goes to Cooldown with no exception and no revert event; rapid selection keys during Targeting leave exactly two highlight events (on, off); `ChangeState(current)`, `ChangeState(null)` and Exit-before-Enter on the bare state machine; every event channel delivers its payload.
Files added: `Scripts/{Core/{CastRejectReason,IAbilityState,AbilityContext,AbilityStateMachine,TargetFinder,AbilityChannels,AppliedEffect,AbilityController,ReadyState,TargetingState,ActiveState,CooldownState}, Abilities/{AbilityDefinition,GravityShiftAbility,AbilityLoadout}, Targets/{MagneticPolarity,ObjectAttributes,CustomGravity,ForceTarget}, Events/{EventChannel,Float,Int,Reject,Ability,ForceTarget}EventChannel, Util/DebugOverlay}`, `Editor/{AbilityAssetBuilder,AbilityRigBuilder}` (Room1Builder edited), `Tests/{AbilityTestFixture,AbilityStateCycleTests,AbilityGravityTests,AbilityRejectTests,AbilityEdgeCaseTests,AbilityStateMachineTests,EventChannelTests,Room1SolutionTests}`, 10 channel assets, `GravityShift.asset`, `AbilityLoadout.asset`. Rebuild everything with `Tools/Kairos/Build Room 1`.
Not verified in cp2: no hand play with a real mouse (all input was injected); `Spring` still unused; no screenshot of the targeting view.

## Seeded-bug candidates (promote the best to the Debug Lab)
| Bug | Where it happens | Symptom | Real or invented? |
|---|---|---|---|
| Test counter kept in an instance field and never reset | `PlayerRespawnTests.raised` | Second test in the class fails with "Expected 1 but was 2" although the game code is right | **Real** (hit during cp1) |
| Play Mode test asmdef lists `Editor` in `includePlatforms` | `Kairos.Tests.asmdef` | Test run says "Passed" with 0 tests | **Real** (hit during cp1) |
| Player collision mode left at `Continuous` while running at 6 m/s | `PlayerMotor.Awake` | Player sinks up to 0.1 m into a wall for one to three physics steps when running into it, then pops back out; invisible in standing tests | **Real** (found in cp1, fix = `ContinuousSpeculative`, not applied) |
| Jump press read in `FixedUpdate` instead of `Update` | `PlayerMotor` | Jump sometimes ignored: a press that starts and ends between two physics steps is never seen | Invented (the real code buffers the press in `Update`) |
| Ground check without the "still rising" guard | `PlayerMotor.UpdateGrounded` | Coyote timer refills on the first frame of a jump, allowing a second jump in the air | Invented (reasoned from the code, not run) |
| `TryCast` without the cooldown check | `AbilityController.TryCast` | Reason list skips `OnCooldown`; the cast reaches `CanApply` while recharging (invisible in normal play because Cooldown never enters Targeting) | **Real** (omitted in cp2, found by writing the reject test) |
| `RequireComponent` with four types | `ForceTarget` | `error CS1729: 'RequireComponent' does not contain a constructor that takes 4 arguments` | **Real** (spec had four) |
| `ActiveState.Exit` that does not call `EndEffect` | `ActiveState` | Respawn during Active leaves the pod with flipped gravity; `OnEffectReverted` never raised | Invented (the real code reverts in Exit) |
| `TargetingState.Exit` that clears `Target` as well as the highlight | `AbilityController.ClearHighlight` | `ActiveState.Enter` finds `Target == null` and throws on the first cast | Invented (reasoned while designing; the real code keeps `Target`) |
| Mouse buttons pressed one at a time in a test | `AbilityTestFixture` | Pressing left releases right in the same step, so a "hold right, click left" test never reaches Active | **Real** (harness quirk, cp2) |

## Architecture proposals (do not implement without approval)
Additions made in cp1 that go beyond the documents. They are additive and nothing in 01_ARCHITECTURE.md was changed, but please confirm or reject them:
- **`IRespawnable` in `Scripts/Core/`** (`CheckpointPosition`, `SetCheckpoint`, `Respawn`). `Checkpoint` and `KillZone` (Level) reach the player through this interface, so Level has no reference to `PlayerRespawn` (Player). The spec's plain "Checkpoint sets the respawn point" would have needed that direct reference.
- **`Kairos.Runtime` asmdef** (`Scripts/Kairos.Runtime.asmdef`, references only `Unity.InputSystem`) and **`Kairos.Tests` asmdef** (`Tests/`). 01_ARCHITECTURE.md allows these as optional; Play Mode tests need them. Note: a `Kairos.Runtime` asmdef means every future script folder under `Scripts/` is compiled into it, and any package it uses (for example Cinemachine in cp5+) must be added to its `references`.
- **Two folders not in the layout list:** `Assets/_Kairos/Editor/` (menu builders, as 05_MCP_WORKFLOW.md recommends) and `Assets/_Kairos/Tests/`.
- **`Bootstrap` is a component in the level scene** for now, not a separate Bootstrap scene as 02_CONVENTIONS.md describes. It only sets the frame rate and warns if the fixed timestep changes. Splitting into two scenes can wait until there are channels to hold.
- **Only `VoidEventChannel` exists** (no generic `EventChannel<T>` base yet); the generic base arrives with the first payload channel in cp2.
- **Room 1 pod is a static collider in cp1** (no Rigidbody); cp2 converts it. The R1→R2 door is a solid placeholder cube (`Door_R1R2_Placeholder`) until cp3.

Additions made in cp2 beyond the documents (all additive; please confirm or reject):
- **`AbilityChannels`** (`[Serializable]` bundle of the channels the controller raises; `TargetChanged` dedupes) and **`AppliedEffect`** (remembers ability, target and context between Apply and Revert). Both exist only to keep `AbilityController` at 150 lines. Alternative: leave them out and accept a ~175-line controller.
- **`TargetFinder` is a `[Serializable]` plain class** with `Initialize()` (called from the controller's `Awake`), not a MonoBehaviour. The spec only said "serialized `TargetFinder finder`".
- **`CustomGravity` start state is serialized** (`startsEnabled`, `startDirection`) plus an idempotent `Initialize()`, instead of an external `Set(true, Up)` call after Awake. This avoids an Awake-order dependency with `ForceTarget`.
- **The pod freezes all rotation** (not only X/Y) so it lands flat and can be stood on; the conventions allow Z rotation for props. Change `MakeForceTarget` if a tumbling prop is wanted later.
- **`AbilityController.CooldownRemaining` / `ActiveRemaining` have public setters** (the states write them). Cleaner would be `internal set`, since the states and the controller are in the same `Kairos.Runtime` assembly. Not changed in cp2 to avoid another full test cycle; proposed as a one-word follow-up.
- **`TargetingState` also leaves when `!TargetHeld`**, not only on `TargetReleased`.
- **`BoolEventChannel` and `StringEventChannel` not created yet**; they arrive with their first user (cp3 plate, cp9 objective).
- **Nothing calls `ForceTarget.ResetToInitial()` yet.** `ForceReset()` reverts the ability, which is enough for the pod; `LevelManager` (cp3) will call `ResetToInitial()` on respawn.

## HUMAN STEPS completed
| Step | Who | Done? | Notes |
|---|---|---|---|
| (none needed in cp1) | n/a | n/a | Camera, scene and tests were all done through MCP and Editor scripts |
| Reconnect `UnityMCP` with `/mcp` (cp2 start) | Human | Done | Server showed `CONNECTION_CLOSED` in the session although `claude mcp list` said Connected |
| (none needed in cp2 otherwise) | n/a | n/a | No Shader Graph, Timeline or Rigging yet |

## Open questions
- **Jump apex is 2.135 m, near the bottom of the 2.2 ± 0.1 window (2.1).** Cause: PhysX integrates velocity then position each 0.02 s step, so the discrete apex is lower than the continuous formula `v²/2g = 2.2` by about `v·dt/2 = 0.066 m`. It passes, but the margin is only 0.035 m and it shrinks as the fixed timestep grows (the loss is `v·dt/2`: about 0.082 m at 40 Hz, 0.055 m at 60 Hz). Options were: (a) leave as is, (b) raise `jumpApexHeight` to ≈ 2.27 so the measured apex is 2.2, (c) widen the tolerance. **Decision (human, 2026-11-05): keep (a), no change.** Re-measure if the fixed timestep ever changes (Bootstrap warns when it does).
- **Wall overlap of about 9 cm: DIAGNOSED, not a `PlayerMotor` bug, not changed.** In `Player_Cannot_Reach_Exit_Ledge_Directly` the player's centre reached x = 10.74 against a ledge face at x = 11 (capsule radius 0.35, so 10.65 expected). Measured with `Physics.ComputePenetration` and a step-by-step log:
  - Cause: **discrete collision detection at running speed.** At 6 m/s the capsule moves 0.12 m per 0.02 s physics step. In the log the right edge was 2.6 cm short of the face (s128, x = 10.624), which is more than the 1 cm contact offset, so PhysX made no contact for that step. The capsule moved the full 0.12 m and ended 0.094 m inside the wall (s129, x = 10.744, overlap 0.094). The next three steps pushed it back out (dx -0.075, -0.015, -0.003) to exactly x = 10.650. So overlap = `0.12 - gap`, at most one step of travel, and it fixes itself within 3 steps (0.06 s).
  - Why it looked random: it depends on the approach phase. Standing pushes (0.000 m), a drop while pushing (0.000 m), and a jump from rest against the wall (0.000 m) never show it, with or without `PlayerMotor` (a motor-off run that sets the same velocity pattern by hand gave the same zeros). Running in from 8 m at 12 slightly different start positions: worst 0.100 m.
  - Collision-detection mode experiment (same 12 phases): `Discrete` 0.100 m, `Continuous` (current) 0.100 m, `ContinuousDynamic` 0.100 m, **`ContinuousSpeculative` 0.000 m**.
  - Possible fix, NOT applied (waiting for your decision after hand-play): set `CollisionDetectionMode.ContinuousSpeculative` on the player (one line in `PlayerMotor.Awake` and the same in `PlayerRigBuilder`). This changes the "Continuous" choice in 02_CONVENTIONS.md only for the player, and would need a re-run of all tests. Alternative: leave it; the overlap is at most 0.12 m for at most 3 physics steps and only when running into a wall.
  - Pinned by `WallContactDiagnosticTests.Running_Into_Wall_Overlap_Is_At_Most_One_Step_And_Recovers` (asserts worst ≤ one step and settled overlap < 5 mm). Measured: worst 0.100 m, settled 0.000 m.
  - Side finding that corrected my own assumption: **`yield return new WaitForFixedUpdate()` resumes AFTER the physics step**, so test samples are post-step positions and resolved velocities (a motor commanding vx = 6 into a wall reads back 0). The apex and distance measurements above are post-step samples at every step and are unaffected.
- **Running Play Mode tests through MCP dirties two tracked files** (seen again after the diagnostic runs, so it is repeatable, not a one-off): (1) `ProjectSettings/EditorSettings.asset` gets `m_EnterPlayModeOptions: 0 → 1` (domain reload disabled), which contradicts 05_MCP_WORKFLOW.md; (2) `Scenes/Level_ContainmentWing.unity` is re-saved with the Cinemachine-driven Main Camera values (FOV 60 → 40, y 3 → 2.55) and a `UniversalAdditionalCameraData` component baked in. Neither is an intended change. I never set them; the test runner is the only thing running when they appear. After each test session restore both with `git checkout -- ProjectSettings/EditorSettings.asset Assets/_Kairos/Scenes/Level_ContainmentWing.unity` (load a different scene first so Unity has nothing open to overwrite) and do not commit them.
- **cp2: ProjectSettings drift from the MCP test runner, NOT committed, waiting for your decision.** Read now: `ProjectSettings/ProjectSettings.asset` `runInBackground: 1` (committed value 0) and `ProjectSettings/EditorSettings.asset` `m_EnterPlayModeOptions: 1` (committed 0). I never edited either; the runner's "No Throttling" step writes them and an interrupted run does not restore them (`EditorSettings` was restored by the runner at times, `ProjectSettings` was not). Consequence: with `runInBackground: 0` the runner flips it at run start, Unity recompiles, and the run is interrupted (4 of about 20 runs this phase stalled). Options: (a) restore both with `git checkout -- ProjectSettings/...` after each session (the cp1 procedure; expect an occasional stalled first run), (b) commit `runInBackground: 1` on purpose so runs start reliably (a behaviour change to the Player settings: the game keeps running when the window loses focus; cp9 has "window focus loss" in its edge cases), (c) leave as is. Not touched until you choose.
- **cp2: the first hop of the Room 1 solution has a thin margin.** The pod's top is at Y = 2.0 and the jump apex is 2.135 m (cp1 decision), so climbing from the floor onto the pod works but with 0.135 m to spare, above the 1.8 m "safe step" in 03_LEVEL_SPEC.md (that table's "1.6 m within the safe step" only covers the second hop, pod to ledge). It passed with scripted input (take-off at x = 3.6 while running); a human may find the timing harder. If hand-play shows it is frustrating, options: raise `jumpApexHeight` (the cp1 option b), or lower the pod's landing height (for example a floor-level recess), or add a visible step. No change made.
- **cp2: not yet checked by a human:** whether the pod (centre y = 7, visible up to about y = 7.6 from the spawn camera) is comfortable to click from the spawn point, and how the targeting feels. All aiming in tests uses `WorldToScreenPoint`.
- **cp2: the revert-carries-you design note** (standing on the pod when the 12 s end) is not tested; the pod simply rises back by gravity.
- **cp2: tests need the Unity window in the foreground** (measured: 12.35 s active time when it was not, 12.002 s when it was).
- `Assets/InputSystem_Actions.inputactions` (the URP template's default action asset) is unused, since the project polls devices directly. Left alone.
- `Assets/Settings/Mobile_RPAsset.asset` was reserialized by Unity (asset version 12 → 13); committed separately as format-only, like the cp0 SampleScene.

## WIP notes — cp2 — 2026-11-05 (state at the moment work was paused)

**Summary:** cp2 is functionally complete and committed (22 `cp2:` commits before this one). Nothing is failing right now. What is left is decisions and human checks, not code. There is no `cp2` tag yet.

### Done (evidence in the cp2 entry above)
- All cp2 files built, compiled one at a time, console clean. Last full Play Mode run (assembly filter `Kairos.Tests`): **28 tests, 28 passed, 0 failed, 0 skipped** (10 from cp1, 18 new).
- Room 1 rebuilds with `Tools/Kairos/Build Room 1`; pod is a `ForceTarget`; Gravity Shift asset, loadout and 10 channel assets exist.
- Measured: active 12.002 s, cooldown 2.009 s, pod lands at (9.00, 1.00) top 2.00, returns to y = 7.00, one `OnEffectReverted` per cast, Room 1 solved with injected input (ledge at x = 14.13, feet 3.60, 7.2 s left).
- Specs updated: `ability-fsm.md`, `force-targets.md`, `events-data.md`.

### Half done / not done
- **Uncommitted, not mine:** `ProjectSettings/ProjectSettings.asset` has `runInBackground: 1` (committed 0) and `ProjectSettings/EditorSettings.asset` has `m_EnterPlayModeOptions: 1` (committed 0). Values read just before writing this note. Written by the MCP test runner's "No Throttling" step. I did not edit or restore them; waiting for the human's choice (restore both to 0, or commit `runInBackground: 1` deliberately). They must not be swept into a commit by accident: always `git add` named files only.
- **No tag.** `git tag cp2` is the human's call after review.
- **Not done:** hand play with a real mouse; student-mode rehearsal (needs a fresh chat with only the Project Brief); screenshot of the targeting view; test for "standing on the pod when the effect ends".
- **Small follow-up not made:** `AbilityController.CooldownRemaining` / `ActiveRemaining` have public setters; should be `internal set`.
- **Unexplained, not investigated:** the console printed `The referenced script (Unknown) on this Behaviour is missing!` five times at the start of each stalled test run. I checked the 15 script GUIDs in `Level_ContainmentWing.unity`: all resolve to real files, so the scene is not the source. My guess is the leftover `Assets/InitTestScene*.unity`, but I never confirmed it. After a successful run the warning did not appear.

### Failing test or error, with full text
Currently failing: **nothing**. These all occurred during the phase; each is fixed unless marked otherwise.

1. Compile (fixed, `ForceTarget.cs`):
   `Assets\_Kairos\Scripts\Targets\ForceTarget.cs(10,6): error CS1729: 'RequireComponent' does not contain a constructor that takes 4 arguments`
2. Compile (self-resolved on the next compile; my copy of the message is cut off in the log reader, so the end of the line is missing):
   `Assets\_Kairos\Scripts\Core\CooldownState.cs(23,24): error CS1061: 'AbilityController' does not contain a definition for 'SelectFromInput' and no accessible extension method 'SelectFromInput' accepting a first ar`
3. Test (fixed, mouse byte-bitfield quirk), `State_Label_Cycles...`:
   `Expected: String containing "State: ActiveState"` / `But was:  "State: ReadyState\nAbility: Gravity Shift / Down\nTarget: none\nCooldown: 0.0s   Active: 0.0s"`
4. Test (fixed, manual `InputSystem.Update()`), same test:
   `Expected: String containing "State: TargetingState"` / `But was:  "State: ReadyState\nAbility: Gravity Shift / Down\nTarget: none\nCooldown: 0.0s   Active: 0.0s"`
5. Test (fixed, measured game time instead of real time), same test:
   `Gravity Shift lasts 12 s` / `Expected: 12.0d +/- 0.30000001192092896d` / `But was:  12.352569580078125d`
6. Test (fixed, my diagnostic read a device control from FixedUpdate context):
   `Unhandled log message: '[Error] Cached unprocessed value unexpectedly became outdated for unknown reason, new value '0' old value '1'.'. Use UnityEngine.TestTools.LogAssert.Expect`
7. Test (fixed, hold Space through the rise), `Room1_Is_Solvable_By_The_Documented_Solution`:
   `could not climb onto the pod` / `Expected: True` / `But was:  False`
8. MCP test runner (workaround, cause not fully proven):
   `Test job failed to initialize (tests did not start within timeout)`; then `error: tests_running` (`retry_after_ms: 5000`) for new runs while the old job stays "running"; cleared by a forced compile, which reports `Job orphaned after domain reload`. Editor.log line at each stall: `[ScriptCompilation] Requested script compilation because: Assetdatabase observed changes in script compilation related files`, directly after `Entering Playmode with Reload Domain disabled.`

### What I tried (for the stalled runs, item 8)
- Waiting longer: no effect. Stopping Play Mode (`manage_editor stop`), loading `Level_ContainmentWing`, deleting the stray `InitTestScene*.unity`: needed, not sufficient on its own.
- Bringing the Unity window to the foreground with a PowerShell `SetForegroundWindow`: the run that followed worked, but a later stall happened with the window focused, so focus is not the cause.
- Forcing a compile with `refresh_unity` (`compile=request`, `mode=force`) to orphan the stuck job, waiting until Editor.log stopped growing, then re-running: worked every time.
- Reading the settings diff: the only changes are `runInBackground` and `m_EnterPlayModeOptions`, both written by the runner. The theory (the runner flips `runInBackground`, Unity recompiles, the run is interrupted) fits the log line and the pattern that runs start fine once the value is already 1. I have not proven it by changing the value back and forth, because the human asked me not to touch ProjectSettings without asking.

### What I would try next
1. Ask the human for the ProjectSettings decision. If they pick "commit `runInBackground: 1`", set it, commit it as its own `cp2:` commit, and confirm that 5 consecutive runs start without a stall. If they pick "restore", `git checkout -- ProjectSettings/EditorSettings.asset ProjectSettings/ProjectSettings.asset` after loading `Level_ContainmentWing`.
2. Human hand-play of Room 1 (controls: A/D, Space held, right mouse hold + left click, R variant, T respawn, F1). Note whether the pod is clickable from the spawn point and whether the first jump onto the pod (0.135 m margin) is acceptable. If it is not, decide between raising `jumpApexHeight` and changing the pod's landing height.
3. Find the source of the five "referenced script (Unknown) … missing" warnings: start a run, and when it stalls, open the leftover `InitTestScene*.unity` and look at its root object's components before deleting it.
4. Make the two timer setters `internal set`, re-run the 28 tests, commit.
5. Run the student-mode rehearsal on `AbilityStateMachine`, `ActiveState` and `GravityShiftAbility`; record the result here.
6. After review: `git tag cp2`, then start cp3 (Mass Control, `BoolEventChannel`, `PressurePlate`, `Door`, `GoalTrigger`, `BrittleBarrier`, `LevelManager` calling `ForceTarget.ResetToInitial()`).

### cp2 close-out — 2026-11-05
- **Hand play done by the human:** "I played the game and it runs as expected from my side." No problems reported with the pod aim, the first jump or the controls. (Nothing more specific was said, so the jump-margin and aim questions above are answered only as "works for the human"; no numbers were collected.)
- **Done after the wip notes:** `CooldownRemaining` and `ActiveRemaining` now have `internal set`. Full suite re-run: **28 tests, 28 passed, 0 failed, 0 skipped**, console clean. This supersedes the "small follow-up not made" item in the wip notes.
- **Correction to my stall theory.** I wrote that the runner flipping `runInBackground` in ProjectSettings.asset causes the recompile that interrupts a run. The 5th stall happened with both settings **already 1** (read just before: `runInBackground: 1`, `m_EnterPlayModeOptions: 1`), with the same Editor.log sequence (`Entering Playmode with Reload Domain disabled.` then `[ScriptCompilation] Requested script compilation because: Assetdatabase observed changes in script compilation related files`). So that theory is not the (whole) cause and is unproven. What is observed: stalls happened on a run started shortly after I changed a `.cs` file; recovery (stop Play Mode, load `Level_ContainmentWing`, delete `Assets/InitTestScene*.unity`, `refresh_unity compile=request scope=all mode=force`, wait, re-run) worked 4 out of 4 times. Cause still unknown.
- **Checked:** the leftover `InitTestScene*.unity` contains one object, "Code-based tests runner", with the Test Framework's own `PlaymodeTestsController`, a valid script. It is not the source of the five "referenced script (Unknown) is missing" warnings; those remain unexplained.
- **ProjectSettings:** `ProjectSettings.asset` (`runInBackground: 1`) and `EditorSettings.asset` (`m_EnterPlayModeOptions: 1`) are still modified in the working tree and are NOT in any cp2 commit or in the cp2 tag. Decision still open (restore to 0, or commit deliberately).
- **Still not done:** the student-mode rehearsal (needs a fresh Claude chat with only the student Project Brief).
