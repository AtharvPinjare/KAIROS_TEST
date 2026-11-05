using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// The single allowed "global" object: applies project-wide runtime settings once at startup.
    /// It holds no gameplay logic and nothing looks it up; it only configures the engine.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Runtime settings")]
        [SerializeField, Tooltip("Frame cap. The jump and coyote numbers are measured at the fixed step, not this.")]
        private int targetFrameRate = 60;
        [SerializeField, Tooltip("The physics step the movement numbers were measured at (seconds).")]
        private float expectedFixedTimestep = 0.02f;

        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            // The jump apex and distance are measured at 50 Hz; warn if someone changes the step.
            if (!Mathf.Approximately(Time.fixedDeltaTime, expectedFixedTimestep))
                KLog.Warn("Bootstrap", $"Fixed timestep is {Time.fixedDeltaTime}, movement was tuned at {expectedFixedTimestep}.");
        }
    }
}
