using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Trigger volume placed below the level. Anything respawnable that touches it is sent back to its checkpoint.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            var respawnable = other.GetComponentInParent<IRespawnable>();
            if (respawnable != null) respawnable.Respawn();
        }
    }
}
