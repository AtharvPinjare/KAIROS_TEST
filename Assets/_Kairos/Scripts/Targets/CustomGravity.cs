using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Per-object gravity. While enabled it turns Unity's gravity off for this body and pushes it
    /// along <see cref="Direction"/> every physics step instead. Disabled = ordinary gravity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravity : MonoBehaviour
    {
        [Header("Tuning")]
        [SerializeField, Tooltip("Acceleration in m/s^2 (matches Physics.gravity by default).")]
        private float strength = 9.81f;

        [Header("Start state")]
        [SerializeField, Tooltip("True = starts with custom gravity (the cargo pod rests against the ceiling).")]
        private bool startsEnabled;
        [SerializeField, Tooltip("Gravity direction at start when 'startsEnabled' is true.")]
        private Vector3 startDirection = Vector3.up;

        Rigidbody body;
        bool initialized;

        /// <summary>True while this object uses custom gravity instead of Unity's.</summary>
        public bool Enabled { get; private set; }

        /// <summary>The (not necessarily normalized) direction gravity pulls in while enabled.</summary>
        public Vector3 Direction { get; private set; } = Vector3.down;

        void Awake() => Initialize();

        /// <summary>Apply the serialized start state once. Safe to call from other Awake methods.</summary>
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            body = GetComponent<Rigidbody>();
            Set(startsEnabled, startsEnabled ? startDirection : Vector3.down);
        }

        /// <summary>Switch custom gravity on or off and choose its direction.</summary>
        public void Set(bool enabled, Vector3 direction)
        {
            if (body == null) body = GetComponent<Rigidbody>();
            Enabled = enabled;
            Direction = direction;
            body.useGravity = !enabled;   // never let both gravities act at once
        }

        void FixedUpdate()
        {
            // Acceleration mode ignores mass, so a heavy and a light body fall alike.
            if (Enabled) body.AddForce(Direction.normalized * strength, ForceMode.Acceleration);
        }
    }
}
