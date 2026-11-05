# CLAUDE.md — KAIROS (Unity 6.3 LTS, URP, 2.5D)

You are building **KAIROS**, a 2.5D physics puzzle-platformer used as the teaching project for a Unity workshop. The build must be **clean, small-file, interface-first code**, because students will later rebuild it using only Claude chat and copy-paste. Readability and teachability outrank cleverness.

Read these before writing code, in this order:
1. `docs/00_GDD.md` — what the game is
2. `docs/01_ARCHITECTURE.md` — how the code is organised (the contract)
3. `docs/02_CONVENTIONS.md` — naming, folders, Unity 6 API rules
4. `docs/04_BUILD_PLAN.md` — the phases you work through, one at a time
5. `docs/systems/*.md` — one spec per system, read the one for the phase you are in
6. `docs/03_LEVEL_SPEC.md`, `docs/05_MCP_WORKFLOW.md`, `docs/06_ASSETS.md` as needed

## Fixed environment

- Unity **6000.3.15f1** (Unity 6.3 LTS). Do not upgrade the editor or switch render pipeline.
- **URP** with Render Graph (the 6.3 default). Never enable URP Compatibility Mode.
- **New Input System** package. Active Input Handling = Input System Package (New). Never use `UnityEngine.Input` (legacy).
- 2.5D: 3D meshes and 3D physics (Rigidbody, Collider, `PhysicsMaterial`), gameplay locked to the XY plane (Z is depth, frozen).
- UI: uGUI + TextMeshPro. Camera: Cinemachine 3.x. Animation: Animator + Animation Rigging package. Cutscenes: Timeline.
- Assets: **free only** (see `docs/06_ASSETS.md`). Never add a paid asset or an unlicensed file.
- Unity MCP: CoplayDev `unity-mcp` (see `docs/05_MCP_WORKFLOW.md`).

## The working loop (every script, every time)

1. State what you will build and which spec it comes from.
2. Write or edit the script(s) with the Unity MCP script tools.
3. **Wait for compilation**, then read the Unity console. Fix every error and every warning you caused before moving on.
4. Wire the scene or prefab through MCP (or write an Editor menu script that does it reproducibly).
5. Verify against the phase's acceptance criteria (Play Mode test, console clean, behaviour observed).
6. `git add` + commit with message `phaseN: <what>`; at the end of a phase create the tag named in the build plan.
7. Append an entry to `docs/BUILD_LOG.md` (template inside). **This is mandatory** — the log becomes the student prompt cards and Debug Lab.

Do not start phase N+1 until phase N's acceptance criteria pass and the tag exists.

## Architecture rules (hard)

- **One class per file; filename equals class name; namespace `Kairos` (sub-namespaces allowed).**
- **Interface first.** Write or update the interface before any implementation of it.
- **Controller coordinates, state machine transitions, states behave, effects execute, targets hold state, attributes are data.**
- **No hard dependencies between systems.** Cross-system communication goes through ScriptableObject event channels (`docs/systems/events-data.md`), not `FindObjectOfType` or static singletons (a single `Bootstrap` object is the only allowed exception).
- **Only `PlayerInputReader` reads input devices.** Everything else consumes its properties and events.
- **New ability = one new `AbilityDefinition` subclass + one asset.** If adding an ability forces an edit to the controller or a state, the architecture is wrong — stop and fix it.
- Keep every file under ~150 lines. If it grows, split it.
- Public fields only for inspector-tuned data on small components; otherwise `[SerializeField] private`.
- Add XML `<summary>` on every public type and a short comment on every non-obvious line of physics.
- No magic numbers in logic: tunables are serialized fields with `[Header]` and `[Tooltip]`, or live in a ScriptableObject.

## Unity 6.3 API rules — use the new names

These are the renames and removals most likely to bite. They come from my knowledge of Unity 6, not from a fresh check, so **the compile step is the authority: if the console disagrees, trust the console and update this file.**

| Do not use | Use |
|---|---|
| `Rigidbody.velocity` | `Rigidbody.linearVelocity` |
| `Rigidbody.drag` / `angularDrag` | `linearDamping` / `angularDamping` |
| `PhysicMaterial` | `PhysicsMaterial` |
| `FindObjectOfType<T>()` | avoid entirely (event channels); if unavoidable `FindFirstObjectByType<T>()` |
| `FindObjectsOfType<T>()` | `FindObjectsByType<T>(FindObjectsSortMode.None)` |
| `UnityEngine.Input.*` | `UnityEngine.InputSystem` (`Keyboard.current`, `Mouse.current`) via `PlayerInputReader` |
| Cinemachine 2 classes (`CinemachineVirtualCamera`, `CinemachineFramingTransposer`) | Cinemachine 3 (`CinemachineCamera`, `CinemachinePositionComposer`, `CinemachineImpulseSource`) |
| Old-style URP render passes (`Blit`, `OnCameraSetup`, `RTHandle` helpers from pre-Render-Graph code) | URP **Fullscreen Pass Renderer Feature** with a Fullscreen Shader Graph; custom passes only via the Render Graph API |
| `Camera.main` in `Update` | cache once in `Awake`/`Start` |
| UI Toolkit | not used in this project — uGUI + TextMeshPro only |

When unsure whether an API exists in 6000.3.x, check it with the MCP reflection/docs tools if the installed MCP version has them (see `docs/05_MCP_WORKFLOW.md`), otherwise write a tiny test script and compile it. **Never guess an API silently.**

## Things you cannot reliably do through MCP — flag them as HUMAN STEPs

Stop and tell the human exactly what to do (with click-by-click steps) instead of attempting a fragile workaround:

- Authoring **Shader Graph** assets (the node graph) — the spec is in `docs/systems/feedback-layer.md`.
- Authoring **Timeline** assets and track bindings if the MCP tool group for timeline is unavailable.
- Rigging setup in **Animation Rigging** (Rig Builder, constraint bindings) if the tool group is unavailable.
- Importing assets that need a login (Mixamo) or the Asset Store.
- Changing **Project Settings** that the MCP cannot reach (Active Input Handling, Graphics/URP assets).

## Scope guardrails

- Core = three rooms, four abilities, ability FSM with timed effects, the feedback layer, one gauntlet animation event, one procedural response, one Shader Graph, one Timeline, the UI set in the GDD.
- Stretch only after cp10: player gravity flip, security drone, WebGL build. Do not start stretch work without being told.
- Do not "improve" the architecture mid-phase. Propose changes in `docs/BUILD_LOG.md` under "Architecture proposals" and wait.

## When you are stuck

1. Re-read the system spec and the acceptance criteria.
2. Make the invisible state visible (`Debug.Log`, gizmos, the on-screen `DebugOverlay`).
3. Change one thing at a time.
4. If it takes more than three attempts, write the failure in `BUILD_LOG.md` and ask the human. Do not thrash.
