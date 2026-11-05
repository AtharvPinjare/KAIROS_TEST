# 00 — KAIROS Game Design Document

## Pitch
You are a test subject in a failing research facility. The experimental device on your arm lets you break the rules of physics on objects. Escape by changing how things fall, weigh, stick and slide.

**Genre:** 2.5D physics puzzle-platformer. **Camera:** side view (Cinemachine). **Session length:** one vertical slice, ~10 minutes of play.

## Design pillars
1. **Every power changes a rule the player already understands.**
2. **Cause and effect are visible at a glance** (shader, particles, sound, UI).
3. **Failure is cheap** — checkpoints are close, respawn is instant.
4. **Combine, don't repeat** — the final room needs two abilities in sequence.

## Core loop
Read the room → pick an ability (1–4) → hold right mouse to target → left mouse to cast → effect runs for a few seconds, then **reverts** → use the changed rule → short cooldown.

## Controls
| Input | Action |
|---|---|
| A / D or arrows | Move |
| Space | Jump (coyote time + jump buffer) |
| 1 / 2 / 3 / 4 | Select Gravity / Mass / Magnetic / Friction |
| Hold right mouse | Targeting mode (reticle on valid target) |
| Left mouse (while targeting) | Cast on target |
| Release right mouse | Cancel targeting |
| R | Cycle ability variant (gravity direction, mass light/heavy) |
| T | Respawn at last checkpoint |
| F1 | Toggle debug overlay |

## Abilities
Each ability is an `AbilityDefinition` ScriptableObject subclass. Durations and cooldown are data, not code.

| Ability | Effect on target | Variants (R) | Duration | Reverts to |
|---|---|---|---|---|
| Gravity Shift | Custom gravity direction on one Rigidbody | Up / Down / Left / Right | 12 s | Original gravity direction |
| Mass Control | Sets mass to min or max limit of the target | Lighter / Heavier | 8 s | Original mass |
| Magnetic Pulse | One impulse; same polarity repels, opposite attracts | — | instant | n/a |
| Friction Field | Switches the target's collider to slippery | — | 8 s | Original friction |

Global cooldown after any cast ends: **2 s**. Only one effect is active at a time (this keeps revert logic simple and makes timing a decision).

**Rule for "why unavailable":** a cast can be rejected for exactly these reasons — no target, target lacks the capability flag, target is not magnetic, ability on cooldown. Each reason has its own UI message.

## The state machine (the teaching centrepiece)
`Ready → Targeting → Active → Cooldown → Ready`

| State | Enter | Update | Exit |
|---|---|---|---|
| Ready | clear target highlight | read selection keys; RMB down → Targeting | — |
| Targeting | show reticle | refresh target each frame; LMB + valid target → Active; RMB up → Ready | hide reticle |
| Active | **apply effect**, start timer, raise events | count timer down; timer ≤ 0 → Cooldown | **revert effect** |
| Cooldown | start cooldown timer | count down; ≤ 0 → Ready | — |

Entering Active applies; exiting Active reverts. Respawn forces a revert and returns to Ready (`AbilityController.ForceReset()`).

## Level: the Containment Wing (three rooms, detail in `03_LEVEL_SPEC.md`)
| Room | Introduces | Puzzle |
|---|---|---|
| 1 Cargo Shaft | Gravity Shift | An anti-grav cargo pod is pinned to the ceiling. Flip its gravity so it drops beside the exit ledge and use it as a step. |
| 2 Counterweight Lab | Mass Control | A heavy block must reach a pressure plate that needs mass ≥ 8. Lighten it, push it onto the plate, wait for the effect to revert — its true mass triggers the plate. Optional: Heavier mode smashes a brittle barrier. |
| 3 Magnetic Bridge | Magnetic Pulse + Friction Field | Pull the metal bridge segment into place (hard stop aligns it), then make the crate slippery so it slides down the ramp across the bridge onto the far switch, opening the final door. |

Teach-test-combine: Room 1 and 2 have no enemies and no timer pressure. Room 3 requires two abilities in sequence.

## Feedback requirements (pillar 2)
- Targeted object: outline/emission pulse. Active effect: force-field shader on the target (colour by ability, polarity, gravity arrow).
- Cast: gauntlet animation → AnimationEvent fires the pulse → particles, camera impulse, hit-stop, sound.
- Revert: reverse-pulse sound + shader fade-out.
- UI: ability bar with cooldown ring, world-anchored reticle, one-line message for rejected casts, objective text.

## Out of scope (do not build unless told)
Player gravity flip, enemies/drone, multiple levels, saving to disk, inventory, destructible environments, multiplayer.
