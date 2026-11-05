using System;
using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Turns a screen position into the ForceTarget under it (or null).
    /// It never reads the mouse: the pointer arrives as a parameter from PlayerInputReader.
    /// </summary>
    [Serializable]
    public class TargetFinder
    {
        [SerializeField, Tooltip("Only objects on these layers can be targeted (Targetable).")]
        private LayerMask targetableMask;
        [SerializeField, Tooltip("Furthest distance (on the XY plane) from the caster to a target, in metres.")]
        private float maxRange = 15f;
        [SerializeField, Tooltip("How far the camera ray travels.")]
        private float rayLength = 100f;

        Camera cam;

        /// <summary>Cache the camera once (never look it up per frame).</summary>
        public void Initialize() => cam = Camera.main;

        /// <summary>The target under the pointer within range of the source, otherwise null.</summary>
        public ForceTarget Find(Vector2 pointerScreenPos, Transform source)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return null;

            Ray ray = cam.ScreenPointToRay(pointerScreenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, rayLength, targetableMask)) return null;

            var target = hit.collider.GetComponentInParent<ForceTarget>();
            if (target == null) return null;

            // Range is measured in the play plane only: depth (Z) is not gameplay.
            Vector3 delta = target.transform.position - source.position;
            delta.z = 0f;
            return delta.sqrMagnitude <= maxRange * maxRange ? target : null;
        }
    }
}
