using UnityEngine;

namespace Kairos
{
    /// <summary>Channel carrying an int (for example the chosen ability variant).</summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Int", fileName = "OnSomething")]
    public class IntEventChannel : EventChannel<int> { }
}
