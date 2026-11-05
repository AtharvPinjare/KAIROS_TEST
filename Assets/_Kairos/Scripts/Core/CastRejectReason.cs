namespace Kairos
{
    /// <summary>Why a cast attempt was refused. Raised through the OnCastRejected channel.</summary>
    public enum CastRejectReason
    {
        None,
        NoTarget,
        CapabilityLocked,
        NotMagnetic,
        OnCooldown
    }
}
