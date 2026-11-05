using System.Collections.Generic;
using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Put this on any prop an ability may affect. It is the executor and the memory:
    /// it owns a private PhysicsMaterial copy (so friction changes never touch shared assets)
    /// and remembers its starting state so a respawn can put the prop back.
    /// </summary>
    // RequireComponent accepts at most three types; CustomGravity already requires the Rigidbody.
    [RequireComponent(typeof(Collider), typeof(ObjectAttributes), typeof(CustomGravity))]
    public class ForceTarget : MonoBehaviour
    {
        [SerializeField, Tooltip("Name shown in the debug overlay and UI. Empty = the GameObject name.")]
        private string displayName;

        // Starting state, recorded once in Awake for ResetToInitial().
        Vector3 startPosition;
        Quaternion startRotation;
        float startMass;
        bool startGravityEnabled;
        Vector3 startGravityDirection;
        float startDynamicFriction, startStaticFriction;
        PhysicsMaterialCombine startFrictionCombine;

        /// <summary>The target's Rigidbody (never kinematic, or forces would be ignored).</summary>
        public Rigidbody Body { get; private set; }
        /// <summary>Permissions and limits for abilities.</summary>
        public ObjectAttributes Attributes { get; private set; }
        /// <summary>Custom gravity driver.</summary>
        public CustomGravity Gravity { get; private set; }
        /// <summary>This object's private copy of its collider material.</summary>
        public PhysicsMaterial RuntimeMaterial { get; private set; }
        /// <summary>Human-readable name.</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Attributes = GetComponent<ObjectAttributes>();
            Gravity = GetComponent<CustomGravity>();
            Gravity.Initialize();   // make sure the start gravity is applied before we record it
            CreateRuntimeMaterial(GetComponent<Collider>());
            RecordInitialState();
        }

        /// <summary>Copy the collider's shared material (or make a default) and assign the copy.</summary>
        void CreateRuntimeMaterial(Collider col)
        {
            var shared = col.sharedMaterial;
            RuntimeMaterial = new PhysicsMaterial("Kairos_Runtime");
            if (shared != null)
            {
                RuntimeMaterial.dynamicFriction = shared.dynamicFriction;
                RuntimeMaterial.staticFriction = shared.staticFriction;
                RuntimeMaterial.bounciness = shared.bounciness;
                RuntimeMaterial.frictionCombine = shared.frictionCombine;
                RuntimeMaterial.bounceCombine = shared.bounceCombine;
            }
            else
            {
                RuntimeMaterial.dynamicFriction = 0.6f;   // normal friction from the level spec
                RuntimeMaterial.staticFriction = 0.6f;
            }
            col.sharedMaterial = RuntimeMaterial;   // our own instance, so the shared asset stays untouched
        }

        void RecordInitialState()
        {
            startPosition = Body.position;
            startRotation = Body.rotation;
            startMass = Body.mass;
            startGravityEnabled = Gravity.Enabled;
            startGravityDirection = Gravity.Direction;
            startDynamicFriction = RuntimeMaterial.dynamicFriction;
            startStaticFriction = RuntimeMaterial.staticFriction;
            startFrictionCombine = RuntimeMaterial.frictionCombine;
        }

        // Per-ability memory: shared AbilityDefinition assets stay stateless, the target keeps the memento.
        readonly Dictionary<AbilityDefinition, object> memory = new Dictionary<AbilityDefinition, object>();

        /// <summary>Store what <paramref name="def"/> needs to undo itself later.</summary>
        public void Remember<T>(AbilityDefinition def, T value) => memory[def] = value;

        /// <summary>Read back what <see cref="Remember{T}"/> stored (default value if nothing was stored).</summary>
        public T Recall<T>(AbilityDefinition def) =>
            memory.TryGetValue(def, out var value) && value is T typed ? typed : default;

        /// <summary>True if <paramref name="def"/> has stored something on this target.</summary>
        public bool Has(AbilityDefinition def) => memory.ContainsKey(def);

        /// <summary>Put the prop back exactly as it was at level start (called on respawn).</summary>
        public void ResetToInitial()
        {
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            // Set both so interpolation does not draw a streak across the room.
            Body.position = startPosition;
            Body.rotation = startRotation;
            transform.SetPositionAndRotation(startPosition, startRotation);
            Body.mass = startMass;
            Gravity.Set(startGravityEnabled, startGravityDirection);
            RuntimeMaterial.dynamicFriction = startDynamicFriction;
            RuntimeMaterial.staticFriction = startStaticFriction;
            RuntimeMaterial.frictionCombine = startFrictionCombine;
            memory.Clear();   // any pending revert data is now meaningless
            Body.WakeUp();
        }
    }
}
