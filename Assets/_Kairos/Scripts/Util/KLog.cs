using System.Diagnostics;

namespace Kairos
{
    /// <summary>
    /// The only place that calls Debug.Log. Calls compile away in release builds
    /// (they stay in the Editor and in Development Builds), so logging costs nothing in a shipped game.
    /// </summary>
    public static class KLog
    {
        /// <summary>Write one tagged line to the Unity console.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string tag, string message)
        {
            UnityEngine.Debug.Log($"[{tag}] {message}");
        }

        /// <summary>Write one tagged warning to the Unity console.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warn(string tag, string message)
        {
            UnityEngine.Debug.LogWarning($"[{tag}] {message}");
        }
    }
}
