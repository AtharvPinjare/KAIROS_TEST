using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// A damped spring for one float: Value chases Target with a bounce you can tune.
    /// Used later for camera recoil and the target reticle. Stable at any frame rate
    /// (implicit integration), so large time steps never explode.
    /// </summary>
    public struct Spring
    {
        /// <summary>Current value.</summary>
        public float Value;
        /// <summary>Current speed of the value.</summary>
        public float Velocity;

        /// <summary>Create a spring resting at a value.</summary>
        public Spring(float start) { Value = start; Velocity = 0f; }

        /// <summary>
        /// Advance the spring by dt seconds.
        /// frequency = oscillations per second (higher = snappier).
        /// dampingRatio = 1 settles with no bounce, below 1 overshoots, above 1 is sluggish.
        /// </summary>
        public float Step(float target, float frequency, float dampingRatio, float dt)
        {
            float omega = 2f * Mathf.PI * frequency;                 // angular frequency
            float f = 1f + 2f * dt * dampingRatio * omega;
            float hoo = dt * omega * omega;                          // dt * omega^2
            float hhoo = dt * hoo;
            float inv = 1f / (f + hhoo);
            float newValue = (f * Value + dt * Velocity + hhoo * target) * inv;
            float newVelocity = (Velocity + hoo * (target - Value)) * inv;
            Value = newValue;
            Velocity = newVelocity;
            return Value;
        }
    }
}
