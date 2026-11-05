using System;
using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// A ScriptableObject "message board" with no payload. One system calls Raise(),
    /// any number of other systems listen to Raised, and none of them know each other.
    /// Subscribe in OnEnable, unsubscribe in OnDisable.
    /// </summary>
    [CreateAssetMenu(menuName = "Kairos/Events/Void", fileName = "OnSomething")]
    public class VoidEventChannel : ScriptableObject
    {
        /// <summary>Fired every time Raise() is called.</summary>
        public event Action Raised;

        /// <summary>Notify every listener.</summary>
        public void Raise() => Raised?.Invoke();
    }
}
