using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Contract for anything that can be sent back to a safe spot.
    /// Level objects (Checkpoint, KillZone) talk to this interface, never to the concrete player class,
    /// so Level code has no hard dependency on Player code.
    /// </summary>
    public interface IRespawnable
    {
        /// <summary>The world position the next Respawn() will return to.</summary>
        Vector3 CheckpointPosition { get; }

        /// <summary>Remember a new respawn position.</summary>
        void SetCheckpoint(Vector3 position);

        /// <summary>Teleport back to the current checkpoint and clear motion.</summary>
        void Respawn();
    }
}
