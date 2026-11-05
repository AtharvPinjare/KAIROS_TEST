using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Data only: what an object is allowed to do and its numeric limits.
    /// It stores permissions and numbers and never applies physics.
    /// Public fields are fine here because this is a tiny inspector-tuned data component.
    /// </summary>
    public class ObjectAttributes : MonoBehaviour
    {
        [Header("Capabilities")]
        public bool canManipulateGravity = true;
        public bool canChangeMass = true;
        public bool canChangeFriction = true;

        [Header("Magnetism")]
        public bool isMagnetic;
        public MagneticPolarity polarity = MagneticPolarity.Neutral;

        [Header("Mass limits")]
        [Tooltip("Lowest mass an ability may set (kg). Hard floor is 0.5.")]
        public float minimumMass = 0.5f;
        [Tooltip("Highest mass an ability may set (kg). Hard cap is 20.")]
        public float maximumMass = 20f;

        /// <summary>True when the object is magnetic and has a real (non-Neutral) polarity.</summary>
        public bool CanUseMagnetism() => isMagnetic && polarity != MagneticPolarity.Neutral;
    }
}
