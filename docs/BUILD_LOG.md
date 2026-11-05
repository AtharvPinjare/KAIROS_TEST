# BUILD_LOG

Write entries as you work. This log is the raw material for the student prompt cards and the Debug Lab, so be specific: paste the exact prompt, the exact error, the exact fix.

## Environment (fill at cp0)
- Unity: 6000.3.15f1
- Packages and versions (from `Packages/manifest.json`): URP 17.3.0, Input System 1.19.0, Cinemachine 3.1.7, Animation Rigging 1.4.1, Timeline 1.8.12, uGUI 2.0.0 (carries TextMeshPro in Unity 6), Test Framework 1.6.0, AI Navigation 2.0.12. ProBuilder not installed (optional). Shader Graph is present in the package cache as a URP dependency.
- Unity MCP version / pinned tag: `com.coplaydev.unity-mcp` pinned to `#v10.0.0` (git URL in manifest). Note: `docs/05_MCP_WORKFLOW.md` quotes the v10.3.0 README, so tool names may differ slightly from that doc.
- MCP tools visible to Claude Code: see "MCP tools" below.
- Tool groups enabled: `core` only (per `manage_tools list_groups`). Disabled: animation, asset_gen, docs, probuilder, profiling, scripting_ext, testing, ui, vfx.
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

## Seeded-bug candidates (promote the best to the Debug Lab)
| Bug | Where it happens | Symptom | Real or invented? |
|---|---|---|---|

## Architecture proposals (do not implement without approval)
-

## HUMAN STEPS completed
| Step | Who | Done? | Notes |
|---|---|---|---|

## Open questions
-
