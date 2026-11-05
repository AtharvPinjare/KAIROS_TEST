using System;
using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Base for ScriptableObject "message boards" that carry one value.
    /// Unity cannot serialize open generics, so each payload type gets a tiny concrete subclass.
    /// Subscribe in OnEnable, unsubscribe in OnDisable.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        /// <summary>Fired every time Raise() is called.</summary>
        public event Action<T> Raised;

        /// <summary>Notify every listener with a value.</summary>
        public void Raise(T value) => Raised?.Invoke(value);
    }
}
