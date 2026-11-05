# 03 — Level Spec: The Containment Wing

All numbers are **starting values** to be tuned by playtesting. Units are metres. The play plane is Z = 0. Floor top surface is Y = 0.

## Player metrics (design to these, then verify in Play Mode)
| Metric | Value | Note |
|---|---|---|
| Move speed | 6 m/s | |
| Jump apex height | 2.2 m | initial vertical speed = √(2·9.81·2.2) ≈ 6.57 m/s |
| Flat jump distance | ≈ 8.0 m | air time ≈ 1.34 s × 6 m/s |
| **Safe step height** | **1.8 m** | 80% of apex — never design a required jump above this |
| **Safe gap width** | **5.5 m** | well under the 8 m maximum |
| Player capsule | height 1.8, radius 0.35, mass 1 | |

Rule: anything the player must reach **without** an ability sits inside the safe numbers. Anything that needs an ability sits clearly outside them (so the puzzle is not skippable).

## Global layout
Left to right across one scene: Room 1 → door → Room 2 → door → Room 3 → final door. Rooms are ~ 24 / 20 / 28 m wide, 9 m tall. A checkpoint sits at the entrance of each room. A `KillZone` below Y = -6 respawns the player.

## Room 1 — The Cargo Shaft (Gravity Shift)
Teaches: onboarding, one ability, nothing can go wrong badly.

| Object | Placement | Details |
|---|---|---|
| Floor | Y = 0, X 0–24 | Layer `Ground` |
| Ceiling | Y = 8 | |
| Cargo pod | 2×2×2, centre (9, 7), touching ceiling | Rigidbody mass 4, **default gravity direction = Up** (anti-grav), `canManipulateGravity = true`. `CustomGravity` enabled at start so it rests against the ceiling. Layer `Targetable`. |
| Exit ledge | top surface Y = 3.6, X 11–24 | Higher than the safe step (1.8) and beyond the 2.2 apex, so it cannot be reached directly. |
| Door R1→R2 | X = 23.5 | Opened by `GoalTrigger` on the ledge |
| Checkpoint 1 | X = 1 | |

**Solution:** select Gravity Shift, set variant **Down**, target the pod. It falls and lands at X ≈ 9 with its top at Y = 2.0. Standing on it, the ledge is 1.6 m higher (within the safe step) and 1 m across. Jump to the ledge before the 12 s effect ends.

**Design note (teaching moment):** when the effect reverts, the pod floats back to the ceiling. If the player is still standing on it they are carried up. This is deliberate: it shows why revert logic matters and why duration is a tuning value.

## Room 2 — The Counterweight Lab (Mass Control)
Teaches: a second ability, a multi-step puzzle, and using **revert** as part of the solution.

| Object | Placement | Details |
|---|---|---|
| Heavy block | 2×2×2 at X = 4, resting on floor | Rigidbody mass **10**, high-friction `PhysicsMaterial` (dynamic 1.0). Layer `Targetable`. `minimumMass 0.5`, `maximumMass 20`. |
| Pressure plate | X = 14, flush with floor | `PressurePlate`: sums mass of Rigidbodies on it, **threshold 8** |
| Brittle barrier (optional path) | X = 17, 2×3 | `BrittleBarrier` breaks if hit by a Rigidbody with mass ≥ 15 moving > 2 m/s |
| Door R2→R3 | X = 19.5 | Opens when the plate raises its `BoolEventChannel` |
| Checkpoint 2 | X = 1 | |

**Solution:** cast Mass Control, variant **Lighter**, on the block (mass → 0.5). Push it onto the plate within 8 s. When the effect reverts, the block's real mass (10) presses the plate (≥ 8) and the door opens.
**Optional solution:** variant **Heavier** (mass → 20), then shove it into the barrier to clear a shortcut.
**Playtest risk:** at mass 10 the block must feel *immovable* for the player. If the player can push it, raise its friction or linear damping until it cannot, rather than changing the player.

## Room 3 — The Magnetic Bridge (Magnetic Pulse + Friction Field)
The mastery test: two abilities in sequence, one of them timed.

| Object | Placement | Details |
|---|---|---|
| Left platform | X 0–10, Y = 0 | Player starts here |
| Pit | X 10–19 (9 m) | Wider than the safe gap — cannot be jumped. Below it a `KillZone` at Y = -6. |
| Right platform | X 19–28, Y = 0 | Switch plate at X = 24, final door at X = 27 |
| Bridge segment | 9 × 0.5 × 3, parked at centre X = 23.5 on a raised rail so its top surface is at Y = 0 | Rigidbody mass 6, constraints freeze Y, Z and all rotation (slides on X only), `isMagnetic = true`, **polarity South**, linear damping 1.5, bounciness 0. A static **hard stop** at X = 9.9 aligns its left end to the pit edge (centre X = 14.5). |
| Crate | 1.5³ at X = 2, on top of the ramp | Rigidbody mass 3, normal friction 0.6, `canChangeFriction = true` |
| Ramp | X 0–5, 18° slope descending to the right, ending flush with the left platform | |
| Switch plate | X = 24 on right platform | `PressurePlate` threshold 2 |
| Final door | X = 27 | Opens when the switch raises its channel |
| Checkpoint 3 | X = 1 | |

Player polarity: **North** (set on `AbilityController`).

**Solution:**
1. Stand at the pit edge, select Magnetic Pulse, target the bridge (South ≠ North → attraction). The impulse pulls it left until it hits the hard stop and spans the pit.
2. Wait out the 2 s cooldown.
3. Select Friction Field, target the crate. Friction drops to 0.05; on an 18° ramp (tan 18° ≈ 0.32 > 0.05) it slides down, crosses the bridge and presses the switch (≈ 3–4 s). The final door opens.
4. Cross the bridge to the door before the friction effect reverts (it affects only the crate).

**Why this works on paper:** at normal friction (0.6) the crate does not slide (0.6 > 0.32). Sliding speed at the ramp bottom ≈ 5–6 m/s; with friction 0.05 it decelerates at ≈ 0.5 m/s², travelling ≈ 30 m, so it reaches the switch about 18 m away inside the 8 s window.

## Level-design checklist (use for review and for teaching)
- Teach safely: Room 1 has no timer pressure, no way to die except the pit-free floor.
- Show cause and effect: every affected object gets the force-field shader, particles and sound.
- Allow experimentation: Room 2 has an optional Heavier-mode shortcut.
- Combine, don't repeat: Room 3 needs two abilities in sequence.
- Keep failure cheap: checkpoint at each room entrance, instant `T` respawn, props reset on respawn.
- Landmarks and sightlines: each room's goal (ledge / plate / door) is visible from the entrance.

## Prop reset
`LevelManager` stores each `ForceTarget`'s initial position/rotation/velocity/gravity/mass/friction on `Start` and restores them on respawn (call `ForceTarget.ResetToInitial()`).

## Greybox first
Build every room from primitives (cubes) with a flat colour per role (ground grey, targetable orange, plate green). Only after cp4 swap in Kenney/Quaternius art (see `06_ASSETS.md`). A solved greybox is a prerequisite for cp5.
