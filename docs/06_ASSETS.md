# 06 — Free assets plan

**Policy:** free only. Prefer CC0 so the files can sit in the public starter repo. Anything not CC0 is downloaded by each student, never committed.

## Verified (licence label read on the source page this session)
| Pack | Source | Licence | Use in KAIROS |
|---|---|---|---|
| Space Station Kit (Kenney) — 80–90 models in FBX/OBJ/glTF | kenney.nl / itch.io | CC0 | Walls, floors, bridges, doors, crates: the facility shell |
| Modular Space Kit (Kenney) — 40 objects, some animated, colour variations | kenney.nl / OpenGameArt | CC0 | Alternative modular kit; animated door candidates |
| Space Kit (Kenney) — spaceport parts, characters, weapons | kenney.nl | CC0 | Astronaut-style character, props |
| Sci-Fi Essentials Kit (Quaternius), Standard version 37 models: animated robots, crates, guns, animated screens | OpenGameArt / quaternius.com | CC0 | Cargo pod, crates, screens, props |
| Ultimate Modular Sci-Fi (Quaternius) — modular interior and props, FBX/OBJ/Blend | OpenGameArt | CC0 | Interior pieces |
| Universal Animation Library (Quaternius), Standard 45 animations on a universal humanoid rig, advertised for Unity retargeting | quaternius.com | CC0 | Locomotion, interact/idle clips |
| Kenney audio: Interface Sounds, UI Audio, Sci-fi Sounds, Impact Sounds, Digital Audio | kenney.nl | CC0 | Clicks, lasers/scans, impacts, UI blips |
| Mixamo characters and animations | mixamo.com (Adobe login) | Free to use inside games; **raw files may not be redistributed** | Backup character/animations. Students download their own. |

## Not verified (decide after checking)
- Whether each Quaternius/Kenney model imports at the right scale and with URP-compatible materials. Phase cp5 includes an import check: scale to 1 unit = 1 m, convert materials to URP Lit (Render Pipeline Converter or manual).
- Whether the Quaternius Universal Animation Library retargets cleanly to the chosen character in Unity (Humanoid avatar setup). If not, use Mixamo or the fallback below.
- Freesound (filter to CC0 only; check each file's licence) and Sonniss GDC bundles are commonly used royalty-free sources; confirm their current terms before committing any file.

## Fallbacks
- **Gauntlet clip:** author a 1-second clip yourself in the Animation window (rotate and scale a gauntlet object, add an AnimationEvent at 0.3 s). This needs no external animation and is a good teaching moment.
- **Character:** capsule + simple gauntlet mesh in cp1–cp5; swap in a rigged character only at cp6.
- **VFX:** built-in Particle System with CC0 particle textures; no paid VFX packs.
- **Fonts:** TextMeshPro default font; no external fonts needed.

## Licence file
Create `ASSET_LICENSES.md` at the repo root: pack name, URL, licence, date downloaded. Required before the starter repo goes public.

## Folder
`Assets/_Kairos/Art/ThirdParty/<Pack>/` — keep third-party files isolated so they can be removed (Mixamo) without touching our code.
