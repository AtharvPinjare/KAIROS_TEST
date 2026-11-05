using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Kairos.Tests
{
    /// <summary>
    /// Base for Play Mode tests: loads the level, adds fake keyboard and mouse devices
    /// (InputTestFixture isolates them from the real hardware), and offers small timing helpers.
    /// </summary>
    public abstract class PlayLevelFixture : InputTestFixture
    {
        protected const string ScenePath = "Assets/_Kairos/Scenes/Level_ContainmentWing.unity";

        protected Keyboard Kb;
        protected PlayerMotor Motor;
        protected Rigidbody Body;

        /// <summary>Add devices, load the level and wait until the player stands still on the floor.</summary>
        protected IEnumerator LoadLevel()
        {
            Kb = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();   // PlayerInputReader needs both devices to report input

#if UNITY_EDITOR
            // The level is not in Build Settings yet, so load it by path (works in the Editor only).
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;

            var player = GameObject.Find("Player");
            Motor = player.GetComponent<PlayerMotor>();
            Body = player.GetComponent<Rigidbody>();
            yield return SettleOnGround();
        }

        /// <summary>Wait (max 2 s) until the player is grounded and almost stopped.</summary>
        protected IEnumerator SettleOnGround()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForFixedUpdate();
                if (Motor.Grounded && Body.linearVelocity.sqrMagnitude < 0.0001f) yield break;
            }
        }

        /// <summary>Wait n physics steps.</summary>
        protected static IEnumerator Steps(int n)
        {
            for (int i = 0; i < n; i++) yield return new WaitForFixedUpdate();
        }

        /// <summary>Place the player somewhere and clear its motion.</summary>
        protected void Teleport(Vector3 position)
        {
            Body.position = position;
            Body.transform.position = position;
            Body.linearVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        /// <summary>A flat test floor far to the right of Room 1 (X 100-140, top at Y = 0) so jumps are not cut short by walls.</summary>
        protected static void BuildTestFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "TestFloor";
            floor.layer = LayerMask.NameToLayer("Ground");
            floor.transform.position = new Vector3(120f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(40f, 1f, 4f);
        }
    }
}
