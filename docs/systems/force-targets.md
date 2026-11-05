# System: Force targets and abilities

Phases: cp2 (gravity), cp3 (mass), cp4 (magnetism, friction). Interface in `01_ARCHITECTURE.md`.

## ObjectAttributes (data only)
```csharp
public class ObjectAttributes : MonoBehaviour
{
    [Header("Capabilities")] public bool canManipulateGravity = true, canChangeMass = true, canChangeFriction = true;
    [Header("Magnetism")]    public bool isMagnetic; public MagneticPolarity polarity = MagneticPolarity.Neutral;
    [Header("Mass limits")]  public float minimumMass = 0.5f, maximumMass = 20f;
    public bool CanUseMagnetism() => isMagnetic && polarity != MagneticPolarity.Neutral;
}
public enum MagneticPolarity { Neutral, North, South }
```
It stores permissions and numbers. It never applies physics.

## ForceTarget (the executor and memory)
`[RequireComponent(typeof(Collider), typeof(ObjectAttributes), typeof(CustomGravity))]`
(`RequireComponent` accepts at most **three** types, so the Rigidbody comes from `CustomGravity`'s own `[RequireComponent(typeof(Rigidbody))]`. The original four-type attribute did not compile.)

- `Awake`: cache `Body`, `Attributes`, `Gravity`; create a **runtime `PhysicsMaterial` instance** (copy of the collider's shared material, or a new one named `Kairos_Runtime` with normal friction 0.6) and assign it to the collider so changes never touch shared assets.
- Records its initial state (position, rotation, velocity, mass, gravity enabled/direction, friction) for `ResetToInitial()`.
- Per-ability memory so shared `AbilityDefinition` assets stay stateless:
```csharp
public void Remember<T>(AbilityDefinition def, T value);
public T Recall<T>(AbilityDefinition def);
public bool Has(AbilityDefinition def);
```
- Exposes `Body`, `Attributes`, `Gravity`, `RuntimeMaterial`, `DisplayName`.

## CustomGravity
```csharp
public class CustomGravity : MonoBehaviour
{
    [SerializeField] float strength = 9.81f;
    public bool Enabled { get; private set; }
    public Vector3 Direction { get; private set; } = Vector3.down;
    public void Set(bool enabled, Vector3 direction);   // sets rb.useGravity = !enabled
    void FixedUpdate() { if (Enabled) rb.AddForce(Direction.normalized * strength, ForceMode.Acceleration); }
}
```
Default state: disabled (normal gravity). The cargo pod starts with `Set(true, Vector3.up)`: in the code this is the serialized `startsEnabled` / `startDirection` pair, applied by `Initialize()` (idempotent; `ForceTarget.Awake` calls it before recording the initial state, so Awake order between the two components does not matter).

## AbilityDefinition subclasses (each is ONE file + ONE asset)

### GravityShiftAbility
- `VariantNames = { "Down", "Up", "Left", "Right" }`.
- `CanApply`: target `Attributes.canManipulateGravity` else `CapabilityLocked`.
- `Apply`: `Remember(this, (Gravity.Enabled, Gravity.Direction))`; `Gravity.Set(true, DirectionFor(ctx.Variant))`; wake the body (`Body.WakeUp()`).
- `Revert`: `Recall` and restore exactly (for the pod this means **back to Up**).
- Duration 12.

### MassControlAbility
- `VariantNames = { "Lighter", "Heavier" }`.
- `CanApply`: `canChangeMass` else `CapabilityLocked`.
- `Apply`: `Remember(this, Body.mass)`; `Body.mass = Mathf.Clamp(variant == 0 ? attr.minimumMass : attr.maximumMass, 0.5f, 20f)`.
- `Revert`: restore remembered mass.
- Duration 8.

### MagneticPulseAbility (instant)
- `VariantNames = {}`; Duration 0; serialized `pulseImpulse = 40`, `maxImpulse = 60`.
- `CanApply`: `!attr.CanUseMagnetism()` or `ctx.SourcePolarity == Neutral` → `NotMagnetic`.
- `Apply`: `KairosMagneticSystem.ApplyPulse(...)`. `Revert`: empty.

```csharp
public static class KairosMagneticSystem
{
    public static bool ApplyPulse(Rigidbody body, ObjectAttributes attr, Vector3 sourcePos,
                                  MagneticPolarity sourcePolarity, float impulse)
    {
        if (body == null || attr == null || !attr.CanUseMagnetism() || sourcePolarity == MagneticPolarity.Neutral) return false;
        Vector3 away = body.position - sourcePos; away.z = 0f;            // 2.5D: ignore depth
        if (away.sqrMagnitude < 0.0001f) return false;
        away.Normalize();
        bool same = sourcePolarity == attr.polarity;                        // same repels, opposite attracts
        body.AddForce((same ? away : -away) * impulse, ForceMode.Impulse);
        return true;
    }
}
```
The impulse is clamped on the ability before calling. Bridge and crate objects rely on rigidbody constraints and the hard stop for alignment.

### FrictionFieldAbility
- `CanApply`: `canChangeFriction` else `CapabilityLocked`.
- `Apply`: `Remember(this, (mat.dynamicFriction, mat.staticFriction, mat.frictionCombine))`; set both frictions to serialized `slipperyFriction = 0.05` and `frictionCombine = PhysicsMaterialCombine.Minimum`.
- `Revert`: restore remembered values. Duration 8.
- Check: `PhysicsMaterialCombine` and `PhysicsMaterial` names must compile in 6000.3; fix and note in BUILD_LOG if not.

## Adding an ability (the architecture test)
Create `YourAbility : AbilityDefinition` implementing `CanApply/Apply/Revert`, create the asset (`CreateAssetMenu`), drag it into the `AbilityLoadout`. **Nothing else may change.** The capstone (Time Dilation) proves this.

## Prop reset
`ForceTarget.ResetToInitial()` restores Rigidbody pose/velocity, mass, gravity and friction. `LevelManager` calls it for every target on respawn.

## Gotchas to test
- Setting `useGravity = false` for custom gravity: re-enable on revert only when the remembered state was "not custom".
- Kinematic Rigidbodies ignore forces; targets must be non-kinematic.
- `PhysicsMaterial` instance: apply friction changes to the **instance**, never to `sharedMaterial`.
- Bridge segment stops against the hard stop and must not jitter: damping 1.5, bounciness 0, continuous collision.
