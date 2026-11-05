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

| Criterion | Evidence (Play Mode test, 50 Hz fixed step, 9/9 tests pass) |
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

Files added: `Scripts/{Kairos.Runtime.asmdef, Core/IRespawnable, Events/VoidEventChannel, Player/{PlayerInputReader,PlayerMotor,PlayerRespawn}, Level/{Checkpoint,KillZone}, Util/{KLog,Spring,Bootstrap}}`, `Editor/{BuildUtil,PlayerRigBuilder,Room1Builder}`, `Tests/{Kairos.Tests.asmdef,PlayLevelFixture,PlayerMotorTests,PlayerRespawnTests,PlayerJumpFeelTests}`, scene `Scenes/Level_ContainmentWing.unity`, two `VoidEventChannel` assets. Re-create the scene any time with menu `Tools/Kairos/Build Room 1`.
Not verified in cp1: `Spring` has no test yet (it is unused until cp6/cp7); no manual keyboard playtest by a human (all input in tests is injected through the Input System test fixture).

## Seeded-bug candidates (promote the best to the Debug Lab)
| Bug | Where it happens | Symptom | Real or invented? |
|---|---|---|---|
| Test counter kept in an instance field and never reset | `PlayerRespawnTests.raised` | Second test in the class fails with "Expected 1 but was 2" although the game code is right | **Real** (hit during cp1) |
| Play Mode test asmdef lists `Editor` in `includePlatforms` | `Kairos.Tests.asmdef` | Test run says "Passed" with 0 tests | **Real** (hit during cp1) |
| Jump press read in `FixedUpdate` instead of `Update` | `PlayerMotor` | Jump sometimes ignored: a press that starts and ends between two physics steps is never seen | Invented (the real code buffers the press in `Update`) |
| Ground check without the "still rising" guard | `PlayerMotor.UpdateGrounded` | Coyote timer refills on the first frame of a jump, allowing a second jump in the air | Invented (reasoned from the code, not run) |

## Architecture proposals (do not implement without approval)
Additions made in cp1 that go beyond the documents. They are additive and nothing in 01_ARCHITECTURE.md was changed, but please confirm or reject them:
- **`IRespawnable` in `Scripts/Core/`** (`CheckpointPosition`, `SetCheckpoint`, `Respawn`). `Checkpoint` and `KillZone` (Level) reach the player through this interface, so Level has no reference to `PlayerRespawn` (Player). The spec's plain "Checkpoint sets the respawn point" would have needed that direct reference.
- **`Kairos.Runtime` asmdef** (`Scripts/Kairos.Runtime.asmdef`, references only `Unity.InputSystem`) and **`Kairos.Tests` asmdef** (`Tests/`). 01_ARCHITECTURE.md allows these as optional; Play Mode tests need them. Note: a `Kairos.Runtime` asmdef means every future script folder under `Scripts/` is compiled into it, and any package it uses (for example Cinemachine in cp5+) must be added to its `references`.
- **Two folders not in the layout list:** `Assets/_Kairos/Editor/` (menu builders, as 05_MCP_WORKFLOW.md recommends) and `Assets/_Kairos/Tests/`.
- **`Bootstrap` is a component in the level scene** for now, not a separate Bootstrap scene as 02_CONVENTIONS.md describes. It only sets the frame rate and warns if the fixed timestep changes. Splitting into two scenes can wait until there are channels to hold.
- **Only `VoidEventChannel` exists** (no generic `EventChannel<T>` base yet); the generic base arrives with the first payload channel in cp2.
- **Room 1 pod is a static collider in cp1** (no Rigidbody); cp2 converts it. The R1→R2 door is a solid placeholder cube (`Door_R1R2_Placeholder`) until cp3.

## HUMAN STEPS completed
| Step | Who | Done? | Notes |
|---|---|---|---|
| (none needed in cp1) | n/a | n/a | Camera, scene and tests were all done through MCP and Editor scripts |

## Open questions
- **Jump apex is 2.135 m, near the bottom of the 2.2 ± 0.1 window (2.1).** Cause: PhysX integrates velocity then position each 0.02 s step, so the discrete apex is lower than the continuous formula `v²/2g = 2.2` by about `v·dt/2 = 0.066 m`. It passes, but the margin is only 0.035 m and it shrinks as the fixed timestep grows (the loss is `v·dt/2`: about 0.082 m at 40 Hz, 0.055 m at 60 Hz). Options were: (a) leave as is, (b) raise `jumpApexHeight` to ≈ 2.27 so the measured apex is 2.2, (c) widen the tolerance. **Decision (human, 2026-11-05): keep (a), no change.** Re-measure if the fixed timestep ever changes (Bootstrap warns when it does).
- **Wall overlap of about 9 cm.** In `Player_Cannot_Reach_Exit_Ledge_Directly` the player's centre reached x = 10.74 against a ledge face at x = 11, with a capsule radius of 0.35 (expected at most 10.65). It was seen only while pressing into a wall at full speed; not investigated further. Check visually at cp2 and fix if the player visibly sinks into walls.
- **`ProjectSettings/EditorSettings.asset` showed `m_EnterPlayModeOptions: 1` (domain reload disabled) after the first test runs**, which contradicts 05_MCP_WORKFLOW.md. I did not set it. Most likely the test runner toggles it temporarily; I restored the committed values and the file diffed clean. If it reappears in `git status`, do not commit it.
- `Assets/InputSystem_Actions.inputactions` (the URP template's default action asset) is unused, since the project polls devices directly. Left alone.
- `Assets/Settings/Mobile_RPAsset.asset` was reserialized by Unity (asset version 12 → 13); committed separately as format-only, like the cp0 SampleScene.
