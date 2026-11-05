# 05 — Unity MCP workflow (CoplayDev `unity-mcp`)

## What I verified (README of CoplayDev/unity-mcp, v10.3.0 dated 2026-10-04)
- Called "MCP for Unity". Requirements: Unity 2021.3 LTS through 6.x, Python 3.10+ via `uv`. Works with Claude Code and other MCP clients.
- Install: Unity → Package Manager → Add package from git URL: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` (the README suggests pinning a release tag such as `#v10.0.0`, or `openupm add com.coplaydev.unity-mcp`).
- Configure: `Window → MCP for Unity → Configure All Detected Clients`.
- Advertises about 50 tool entrypoints: scenes, GameObjects, scripts, assets, tests, profiling, build. Documentation site has a **tool catalog**, **tool groups** (vfx / animation / ui / testing and others), Roslyn script validation and multi-instance routing: https://coplaydev.github.io/unity-mcp/

## What I did not verify (check on your machine before relying on it)
- The exact tool names and which tool groups are on by default. **Open the tool catalog and list the tools Claude Code actually sees.** Record the list in `BUILD_LOG.md`.
- Whether your installed version has reflection/API-docs tools (a beta once added `unity_reflect` / `unity_docs`). If present, use them to confirm Unity 6.3 API signatures. If absent, rely on compile errors.
- Whether Timeline, Animation Rigging and Shader Graph can be driven through MCP. Assume **no** until a tool proves otherwise, and mark those steps HUMAN STEP.

## Setup checklist (cp0)
1. Pin the MCP package to a release tag in `Packages/manifest.json`; do not auto-update during the build.
2. Install `uv`; start the MCP server from the Unity window.
3. Register with Claude Code using the Unity window's configure option (or the command shown in the README).
4. Smoke test: ask Claude Code to read the Unity console and create then delete an empty GameObject.
5. Commit **before** any large MCP operation so a bad batch edit can be reverted with git.

## Operating rules
- **Compile loop:** after creating or editing a script, wait for domain reload to finish, then read the console. Never stack multiple script changes before checking for errors.
- Prefer **Editor menu scripts** (`Tools/Kairos/Build Room 1`, etc.) for assembling rooms. They are reproducible, reviewable in git and double as teaching material ("let Claude write the tool that builds the scene").
- Use **Play Mode tests** (Test Framework) for measurable acceptance criteria (jump height, state transitions, mass restore).
- Use the **Profiler** tools in cp9 to confirm there are no per-frame allocations.
- If the Editor is busy compiling, MCP calls can fail; wait and retry once, do not retry in a loop.
- Do not let MCP edit `ProjectSettings` blindly; read the current value first and change one setting at a time.

## Domain reload / Play Mode notes
Some MCP setups misbehave when "Reload Domain" is disabled in Enter Play Mode settings. Keep the default (domain reload on) unless a specific test needs otherwise.

## When MCP cannot do something
Write the step as a numbered HUMAN STEP with exact menu paths and values, ask the person to do it and confirm, then continue. Typical cases: Shader Graph nodes, Timeline tracks, Rig Builder bindings, Project Settings, asset downloads that need a login.

## Security hygiene
MCP lets an assistant edit your project. Keep the project in git, review diffs before tags, and never paste secrets into prompts or scripts.
