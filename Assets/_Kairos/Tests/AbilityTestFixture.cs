using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Kairos.Tests
{
    /// <summary>
    /// Base for ability tests: loads Room 1 and offers helpers to aim the mouse at world points,
    /// hold the target button, click to cast and wait for a state (all through injected input).
    /// </summary>
    public abstract class AbilityTestFixture : PlayLevelFixture
    {
        protected AbilityController Abilities;
        protected ForceTarget Pod;
        protected Mouse MouseDevice;

        /// <summary>Position of the pod's centre while it rests against the ceiling.</summary>
        protected static readonly Vector3 PodHome = new Vector3(9f, 7f, 0f);

        protected IEnumerator LoadAbilityLevel()
        {
            yield return LoadLevel();
            Abilities = GameObject.Find("Player").GetComponent<AbilityController>();
            Pod = GameObject.Find("CargoPod").GetComponent<ForceTarget>();
            MouseDevice = Mouse.current;
            pointer = Vector2.zero;   // NUnit reuses one fixture instance, so reset per test
            rightDown = leftDown = false;
            Assert.IsNotNull(Abilities, "Player needs an AbilityController (run Tools/Kairos/Build Room 1)");
            Assert.IsNotNull(Pod, "CargoPod needs a ForceTarget");
            yield return Steps(5);   // Start() has run and selected slot 0
        }

        // The mouse is driven with whole MouseState events. InputTestFixture.Press(button) writes the byte
        // that holds ALL buttons, so pressing left would silently release right (a harness quirk, not a game bug).
        Vector2 pointer;
        bool rightDown, leftDown;

        void PushMouse()
        {
            var state = new MouseState { position = pointer }
                .WithButton(MouseButton.Right, rightDown)
                .WithButton(MouseButton.Left, leftDown);
            // Queue only: the next frame processes it before any Update runs. Calling InputSystem.Update()
            // here would advance the input frame and "pressed this frame" would already be false.
            InputSystem.QueueStateEvent(MouseDevice, state);
        }

        /// <summary>Point the mouse at a world position (through the real camera).</summary>
        protected void AimAt(Vector3 world)
        {
            pointer = Camera.main.WorldToScreenPoint(world);
            PushMouse();
        }

        /// <summary>Hold the right mouse button (Ready becomes Targeting).</summary>
        protected IEnumerator HoldTarget()
        {
            rightDown = true;
            PushMouse();
            yield return Steps(4);
        }

        protected IEnumerator ReleaseTarget()
        {
            rightDown = false;
            PushMouse();
            yield return Steps(4);
        }

        /// <summary>Click the left mouse button once (the right button keeps its state).</summary>
        protected IEnumerator ClickCast()
        {
            leftDown = true;
            PushMouse();
            yield return Steps(3);
            leftDown = false;
            PushMouse();
            yield return Steps(2);
        }

        /// <summary>Hold target, aim at the pod where it is now, click: the documented Room 1 cast.</summary>
        protected IEnumerator CastOnPod()
        {
            AimAt(Pod.transform.position);
            yield return HoldTarget();
            yield return ClickCast();
        }

        /// <summary>Poll every physics step until the controller is in the named state; fails after timeoutSeconds.</summary>
        protected IEnumerator WaitForState(string stateName, float timeoutSeconds)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            while (Abilities.StateName != stateName && Time.realtimeSinceStartup < end)
                yield return new WaitForFixedUpdate();
            Assert.AreEqual(stateName, Abilities.StateName, $"never reached {stateName} within {timeoutSeconds}s");
        }

        /// <summary>Find a loaded ScriptableObject channel asset by name.</summary>
        protected static T Channel<T>(string assetName) where T : Object =>
            Resources.FindObjectsOfTypeAll<T>().First(c => c.name == assetName);
    }
}
