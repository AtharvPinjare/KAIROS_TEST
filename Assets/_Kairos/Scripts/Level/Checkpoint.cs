using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Trigger volume. The first time the player enters, it stores its respawn point on the player
    /// (through IRespawnable) and raises OnCheckpointReached once.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("Where the player reappears. Leave empty to use this object's position.")]
        private Transform respawnPoint;
        [SerializeField, Tooltip("Raised once when this checkpoint is first reached.")]
        private VoidEventChannel onCheckpointReached;

        bool reached;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            var respawnable = other.GetComponentInParent<IRespawnable>();
            if (respawnable == null) return;

            Vector3 point = respawnPoint != null ? respawnPoint.position : transform.position;
            respawnable.SetCheckpoint(point);   // always update: walking back to an earlier checkpoint is allowed

            if (reached) return;
            reached = true;
            if (onCheckpointReached != null) onCheckpointReached.Raise();
            KLog.Log("Checkpoint", $"{name} reached");
        }
    }
}
