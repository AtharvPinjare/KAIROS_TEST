using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// A small on-screen box (toggle with F1) that makes the invisible state visible:
    /// state machine state, selected ability and variant, target, cooldown and active timers.
    /// The text is rebuilt 10 times a second while visible, so it adds no per-frame garbage.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private AbilityController abilities;
        [SerializeField, Tooltip("The only object allowed to read devices (for the F1 toggle).")]
        private PlayerInputReader input;

        [Header("Tuning")]
        [SerializeField] private bool startVisible = true;
        [SerializeField, Tooltip("Seconds between text rebuilds.")]
        private float refreshInterval = 0.1f;

        bool visible;
        float nextRefresh;
        string text = string.Empty;

        void Awake() => visible = startVisible;

        void Update()
        {
            if (input != null && input.DebugTogglePressed) visible = !visible;
            if (!visible || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshInterval;
            text = BuildText();
        }

        /// <summary>The overlay's text right now (also used by tests).</summary>
        public string BuildText()
        {
            string ability = abilities.Selected == null ? "none" : abilities.Selected.DisplayName;
            string variant = abilities.Selected != null && abilities.Selected.VariantNames.Length > 0
                ? abilities.Selected.VariantNames[abilities.Variant] : "-";
            string target = abilities.Target == null ? "none" : abilities.Target.DisplayName;
            return $"State: {abilities.StateName}\nAbility: {ability} / {variant}\nTarget: {target}\n" +
                   $"Cooldown: {abilities.CooldownRemaining:0.0}s   Active: {abilities.ActiveRemaining:0.0}s";
        }

        void OnGUI()
        {
            if (visible) GUI.Box(new Rect(10f, 10f, 260f, 84f), text);
        }
    }
}
