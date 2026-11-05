using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Remembers the last checkpoint and teleports the player back to it on T or when a KillZone asks.
    /// Raises OnPlayerRespawn so other systems (abilities, props, camera) can reset themselves
    /// without this class knowing they exist.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerRespawn : MonoBehaviour, IRespawnable
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("The only object allowed to read devices.")]
        private PlayerInputReader input;
        [SerializeField, Tooltip("Raised after every respawn (abilities, props and camera listen).")]
        private VoidEventChannel onPlayerRespawn;

        Rigidbody body;

        /// <inheritdoc />
        public Vector3 CheckpointPosition { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            CheckpointPosition = transform.position;   // the start position is the first checkpoint
        }

        void Update()
        {
            if (input.RespawnPressed) Respawn();
        }

        /// <inheritdoc />
        public void SetCheckpoint(Vector3 position) => CheckpointPosition = position;

        /// <inheritdoc />
        public void Respawn()
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            // Move both so interpolation does not draw a streak across the level.
            body.position = CheckpointPosition;
            transform.position = CheckpointPosition;
            if (onPlayerRespawn != null) onPlayerRespawn.Raise();
            KLog.Log("Respawn", $"Back to {CheckpointPosition}");
        }
    }
}
