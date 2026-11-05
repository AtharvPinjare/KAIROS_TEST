using UnityEngine;

namespace Kairos
{
    /// <summary>Channel carrying the reason a cast was refused (OnCastRejected).</summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Reject", fileName = "OnSomething")]
    public class RejectEventChannel : EventChannel<CastRejectReason> { }
}
