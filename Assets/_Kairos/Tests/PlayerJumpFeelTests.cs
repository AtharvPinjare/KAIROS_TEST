using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>
    /// Coyote time (0.1 s) and jump buffer (0.1 s). Each rule gets a "just inside the window" test
    /// that must jump and a "clearly outside the window" test that must not.
    /// </summary>
    public class PlayerJumpFeelTests : PlayLevelFixture
    {
        float bestUpSpeed;   // highest upward speed seen after the jump press

        /// <summary>Walk off the right edge of the test floor, wait, press Jump, and record the best upward speed.</summary>
        IEnumerator WalkOffThenJump(int stepsAfterLeaving)
        {
            BuildTestFloor();
            Teleport(new Vector3(138f, 0.05f, 0f));   // 2 m before the edge at x = 140
            yield return SettleOnGround();

            Press(Kb.dKey);
            for (int i = 0; i < 100 && Motor.Grounded; i++) yield return new WaitForFixedUpdate();
            Assert.IsFalse(Motor.Grounded, "player should have walked off the edge");

            yield return Steps(stepsAfterLeaving);
            Press(Kb.spaceKey);
            bestUpSpeed = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                bestUpSpeed = Mathf.Max(bestUpSpeed, Motor.VelocityY);
                yield return new WaitForFixedUpdate();
            }
            Release(Kb.spaceKey);
            Release(Kb.dKey);
        }

        /// <summary>Drop from 1 m and press Jump when the floor is about leadSeconds away; record the best upward speed afterwards.</summary>
        IEnumerator DropAndPress(float leadSeconds)
        {
            BuildTestFloor();
            Teleport(new Vector3(120f, 1f, 0f));
            bestUpSpeed = float.MinValue;

            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForFixedUpdate();
                // Rough time to the floor: height above the feet-skin divided by fall speed.
                float timeToFloor = (Body.position.y - 0.05f) / Mathf.Max(0.01f, -Motor.VelocityY);
                if (Motor.VelocityY < -0.5f && timeToFloor <= leadSeconds) break;
            }

            Press(Kb.spaceKey);
            yield return Steps(1);
            Release(Kb.spaceKey);
            for (int i = 0; i < 60; i++)
            {
                bestUpSpeed = Mathf.Max(bestUpSpeed, Motor.VelocityY);
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator Coyote_Jump_Works_Shortly_After_Leaving_Ledge()
        {
            yield return LoadLevel();
            yield return WalkOffThenJump(stepsAfterLeaving: 2);   // about 0.04 s after the edge
            KLog.Log("Measure", $"coyote, 0.04 s after edge: best upward speed = {bestUpSpeed:F2} m/s");
            Assert.Greater(bestUpSpeed, 3f);
        }

        [UnityTest]
        public IEnumerator Coyote_Jump_Fails_Well_After_Leaving_Ledge()
        {
            yield return LoadLevel();
            yield return WalkOffThenJump(stepsAfterLeaving: 8);   // about 0.16 s after the edge
            KLog.Log("Measure", $"coyote, 0.16 s after edge: best upward speed = {bestUpSpeed:F2} m/s");
            Assert.Less(bestUpSpeed, 1f);
        }

        [UnityTest]
        public IEnumerator Buffered_Jump_Fires_On_Landing()
        {
            yield return LoadLevel();
            yield return DropAndPress(leadSeconds: 0.07f);   // press about 0.07 s before touching down
            KLog.Log("Measure", $"buffer, pressed ~0.07 s before landing: best upward speed = {bestUpSpeed:F2} m/s");
            Assert.Greater(bestUpSpeed, 3f);
        }

        [UnityTest]
        public IEnumerator Early_Press_Is_Forgotten_Before_Landing()
        {
            yield return LoadLevel();
            yield return DropAndPress(leadSeconds: 0.25f);   // press about 0.19 s before touching down: buffer has expired
            KLog.Log("Measure", $"buffer, pressed ~0.19 s before landing: best upward speed = {bestUpSpeed:F2} m/s");
            Assert.Less(bestUpSpeed, 1f);
        }
    }
}
