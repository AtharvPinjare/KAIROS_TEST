# System: Feedback layer (shader, VFX, audio, game feel)

Phase: cp5. All feedback is driven by **events**; nothing here is referenced by ability code.

## 1. ForceVisual + Force Field shader

### Approach
One transparent **overlay material** (Shader Graph) is added as an extra material slot on each target's renderer. `ForceVisual` listens to events and drives it with a `MaterialPropertyBlock` per slot (`renderer.SetPropertyBlock(block, materialIndex)`). Using property blocks keeps shared material assets untouched. (They opt those renderers out of SRP Batcher batching; acceptable at this scale — mention it as a teaching point.)

### ForceVisual states
| State | Trigger | Look |
|---|---|---|
| Idle | default / after fade-out | overlay intensity 0 |
| Targeted | `OnTargetChanged(this)` | low-intensity pulse in the selected ability's colour |
| Active | `OnEffectApplied(this)` | full intensity, colour by ability, `_Progress` counts down to the end of the effect |
| Reverting | `OnEffectReverted(this)` | fade intensity to 0 over 0.3 s |

Per-state parameters pass through `_BaseColor`, `_Intensity`, `_Progress` (1→0 as the effect runs out), `_Polarity` (-1 South, 0 none, +1 North), `_GravityDir` (Vector2).

### Shader Graph spec — `KairosForceField` (**HUMAN STEP**: build in the Shader Graph editor)
Graph settings: Material = **Unlit**, Surface = **Transparent**, Blending = **Additive** (or Alpha), Render Face = **Both**, Depth Write off.

Exposed properties (names must match exactly):
| Reference | Type | Default |
|---|---|---|
| `_BaseColor` | Color (HDR) | cyan |
| `_Intensity` | Float | 0 |
| `_Progress` | Float (0–1) | 1 |
| `_Polarity` | Float | 0 |
| `_GravityDir` | Vector2 | (0,-1) |
| `_FresnelPower` | Float | 3 |
| `_ScrollSpeed` | Float | 0.6 |
| `_NoiseScale` | Float | 6 |

Node outline:
1. **Fresnel Effect** (power = `_FresnelPower`) → rim mask.
2. **UV** → `Tiling And Offset` with offset = `Time` × `_ScrollSpeed` × `_GravityDir` → **Gradient Noise** (scale `_NoiseScale`) → energy flow that visibly moves in the gravity direction.
3. Dissolve: `Step(1 - _Progress, noise)` → the effect fades away as `_Progress` falls.
4. Polarity tint: `Lerp` the colour toward red for `_Polarity > 0`, blue for `< 0` (use `Branch` or `Remap` of polarity).
5. Emission = `(rim + noise × 0.5) × dissolve × _BaseColor × _Intensity`; Alpha = `saturate(emission luminance)`.
Test material on a cube first; verify in Play Mode that changing the property block changes the look.

### Optional: full-screen "force vision" (stretch)
Use the **URP Fullscreen Pass Renderer Feature** with a Fullscreen Shader Graph (e.g. subtle edge tint while Targeting). Do not write a custom Blit pass — in URP on Unity 6.3 custom passes must use the Render Graph API.

## 2. Particles
`ParticleBursts` pools 8 `ParticleSystem` instances. On `OnGauntletPulse` → burst at the gauntlet; on `OnEffectApplied` → burst at the target in the ability colour; on `OnEffectReverted` → inward "sucking" burst. Colour from `AbilityDefinition.Colour`. CC0 particle textures only.

## 3. Audio
- AudioMixer: `Master` → `SFX`, `UI`, `Music`. Snapshots: `Default`, `Pulse` (low-pass on Music, brief).
- `SfxPlayer`: pool of 8 `AudioSource`s; clips mapped to channels in a serialized list; **pitch varied ±5%** per play; ducking by transitioning to `Pulse` for 0.4 s on `OnGauntletPulse`.
- Sources of sound (CC0 Kenney packs): select tick, target lock blip, cast whoosh, pulse hit, revert reverse sound, reject buzz, plate click, door.

## 4. Game feel ("juice")
All values live in a `FeelSettings` asset.
| Effect | Default | Notes |
|---|---|---|
| Hit-stop on pulse | 0.06 s, timeScale 0.05 | use unscaled time to restore; never overlap; must not freeze UI or audio |
| Camera impulse | strength 0.4 | `CinemachineImpulseSource.GenerateImpulse` on the player + `CinemachineImpulseListener` on the camera (verify class names for the installed Cinemachine 3 version) |
| FOV kick | +3° returning with a critically damped spring | uses `Spring` utility |
| Squash & stretch | player scale (1.1, 0.9) on land, spring back | optional |
| Coyote time / jump buffer | 0.1 s each | already in `PlayerMotor` (cp1) |

## Acceptance (cp5)
- Casting anything produces: shader, particles, sound, camera response, within 100 ms of the pulse event.
- Reverting produces a clearly different, quieter cue.
- No feedback script references an ability class.
- No GC allocations per frame from feedback code (Profiler).
