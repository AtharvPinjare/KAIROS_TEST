using UnityEngine;

namespace Kairos
{
    /// <summary>Channel carrying a float (for example the normalized cooldown 1 to 0).</summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Float", fileName = "OnSomething")]
    public class FloatEventChannel : EventChannel<float> { }
}
