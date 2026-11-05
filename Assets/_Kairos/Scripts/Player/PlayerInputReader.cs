using UnityEngine;
using UnityEngine.InputSystem;

namespace Kairos
{
    /// <summary>
    /// The ONLY class that reads input devices (new Input System, direct polling).
    /// Everything else reads these properties. Runs before every other script
    /// so consumers never see last frame's input.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        /// <summary>Set false (cutscenes, Timeline) to make the reader report "no input".</summary>
        public bool InputEnabled { get; set; } = true;

        /// <summary>Horizontal axis, -1 (left) to 1 (right).</summary>
        public float Move { get; private set; }
        /// <summary>True only on the frame Jump went down.</summary>
        public bool JumpPressed { get; private set; }
        /// <summary>True while Jump is held.</summary>
        public bool JumpHeld { get; private set; }
        /// <summary>0..3 on the frame an ability key is pressed, otherwise -1.</summary>
        public int AbilitySelect { get; private set; } = -1;

        public bool TargetHeld { get; private set; }
        public bool TargetPressed { get; private set; }
        public bool TargetReleased { get; private set; }
        public bool CastPressed { get; private set; }
        public bool VariantPressed { get; private set; }
        public bool RespawnPressed { get; private set; }
        public bool DebugTogglePressed { get; private set; }

        /// <summary>Mouse position in screen pixels.</summary>
        public Vector2 PointerScreenPosition { get; private set; }

        /// <summary>Zero every value so a disabled reader reports "no input".</summary>
        void ClearAll()
        {
            Move = 0f;
            JumpPressed = JumpHeld = false;
            AbilitySelect = -1;
            TargetHeld = TargetPressed = TargetReleased = CastPressed = false;
            VariantPressed = RespawnPressed = DebugTogglePressed = false;
        }

        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null || mouse == null || !InputEnabled) { ClearAll(); return; }

            Move = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                 - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);

            JumpPressed = kb.spaceKey.wasPressedThisFrame;
            JumpHeld = kb.spaceKey.isPressed;

            AbilitySelect = kb.digit1Key.wasPressedThisFrame ? 0
                          : kb.digit2Key.wasPressedThisFrame ? 1
                          : kb.digit3Key.wasPressedThisFrame ? 2
                          : kb.digit4Key.wasPressedThisFrame ? 3 : -1;

            TargetPressed = mouse.rightButton.wasPressedThisFrame;
            TargetReleased = mouse.rightButton.wasReleasedThisFrame;
            TargetHeld = mouse.rightButton.isPressed;
            CastPressed = mouse.leftButton.wasPressedThisFrame;

            VariantPressed = kb.rKey.wasPressedThisFrame;
            RespawnPressed = kb.tKey.wasPressedThisFrame;
            DebugTogglePressed = kb.f1Key.wasPressedThisFrame;

            PointerScreenPosition = mouse.position.ReadValue();
        }
    }
}
