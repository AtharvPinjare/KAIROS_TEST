# 02 — Conventions

## Code style
- C# 9 features available in Unity 6; prefer clarity over brevity.
- `PascalCase` types/methods/properties, `camelCase` locals/params, `_camelCase` for private fields **or** plain `camelCase` with `[SerializeField] private` — pick `[SerializeField] private camelCase` and stay consistent.
- No public mutable fields except tiny data components (`ObjectAttributes`).
- Events: `System.Action` fields prefixed `On` (e.g. `OnCooldownChanged`) on C# classes; ScriptableObject channels named `On<Thing>Channel` in the project window.
- Cache component references in `Awake`; subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- No allocations in `Update`/`FixedUpdate` (no LINQ, no `new` collections, no string concatenation for logs per frame).
- Logs: `Debug.Log` only through a tiny `KLog.Log(tag, msg)` wrapper so logs can be switched off for builds.

## Physics rules (2.5D)
- Player and props: `Rigidbody` with constraints **Freeze Position Z** and **Freeze Rotation X/Y** (props may keep Z rotation free).
- `Collision Detection = Continuous Dynamic` for props that can be pushed or pulled; `Interpolate` on player and moving props.
- Clamp: mass in `[0.5, 20]`, speed ≤ 25 m/s (`Rigidbody.maxLinearVelocity`), magnetic impulse ≤ configurable cap.
- Default `Physics.gravity` = (0, -9.81, 0). Custom gravity is applied per object with `ForceMode.Acceleration` in `FixedUpdate`, and `useGravity` is turned off while custom gravity is active.
- Use a runtime **instance** of `PhysicsMaterial` per target so friction changes never leak into shared assets or other objects.
- Friction combine: set the slippery material's `frictionCombine = Minimum` so it wins against any surface.

## Layers and tags
Layers: `Player`, `Ground`, `Targetable`, `Interactable`, `Ignore Raycast`. Targeting raycasts use a `LayerMask` serialized on `TargetFinder` and hit only `Targetable`. No string tags in logic; capabilities live in `ObjectAttributes`.

## Input
Only `PlayerInputReader` talks to `Keyboard.current` / `Mouse.current`. It exposes: `Move` (float -1..1), `JumpPressed`, `JumpHeld`, `AbilitySelect` (int 0..3 or -1), `TargetHeld`, `TargetPressed`, `TargetReleased`, `CastPressed`, `VariantPressed`, `RespawnPressed`, `DebugTogglePressed`, `PointerScreenPosition`. It also has an `InputEnabled` flag that Timeline and cutscenes use to lock gameplay input.

## Scenes
`Bootstrap` (loads once, holds channels' references) → `Level_ContainmentWing`. Keep one level scene for the slice. UI is a Canvas in the level scene, prefab-ised.

## Git
- Branch `main` only; one commit per verified step; tags `cp0`…`cp10`.
- Commit message format `<phase>: <what and why>` (e.g. `cp2: ability state machine cycles through all four states`). Students are taught the same format.
- Use the templates in `repo-templates/` (`.gitignore`, `.gitattributes`, `.github/pull_request_template.md`) when creating the starter repo.
- `.gitignore`: Unity standard (Library, Temp, Obj, Logs, UserSettings, Builds).
- **Git LFS** for `*.fbx *.png *.wav *.ogg *.psd *.blend` if repo exceeds ~500 MB; otherwise skip.
- Never commit the `Library` folder. Commit `.meta` files.
- Fallback packages: after each tag, export `Kairos_cpN.unitypackage` (Assets → Export Package, include dependencies) to `/Checkpoints/`.

## Documentation
Every system gets one spec in `docs/systems/`. When behaviour changes, update the spec in the same commit.

## BUILD_LOG
`docs/BUILD_LOG.md` is written as you go. It is the raw material for the student prompt cards and the Debug Lab. Be specific: the exact prompt, what broke, what fixed it.
