using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>
    /// cp1 acceptance tests for movement. Numbers come from docs/03_LEVEL_SPEC.md:
    /// apex 2.2 +/- 0.1 m, flat distance about 8 m, exit ledge (top Y = 3.6) unreachable without an ability.
    /// Every measurement is written to the console with the [Measure] tag.
    /// </summary>
    public class PlayerMotorTests : PlayLevelFixture
    {
        float apex, distance, airTime;

        /// <summary>Jump once on the test floor, holding Jump the whole time, and record apex, distance and air time.</summary>
        IEnumerator MeasureJump(bool running)
        {
            BuildTestFloor();
            Teleport(new Vector3(101f, 0.05f, 0f));
            yield return SettleOnGround();

            if (running) { Press(Kb.dKey); yield return Steps(25); }   // 0.5 s is plenty to reach 6 m/s

            float baseY = Body.position.y;
            Press(Kb.spaceKey);
            for (int i = 0; i < 10 && Motor.VelocityY < 1f; i++) yield return new WaitForFixedUpdate();   // wait for takeoff

            float startX = Body.position.x, startTime = Time.fixedTime, maxY = baseY;
            yield return Steps(5);   // leave the ground-check skin before looking for the landing
            for (int i = 0; i < 150 && !Motor.Grounded; i++)
            {
                maxY = Mathf.Max(maxY, Body.position.y);
                yield return new WaitForFixedUpdate();
            }

            apex = maxY - baseY;
            distance = Body.position.x - startX;
            airTime = Time.fixedTime - startTime;
            Release(Kb.spaceKey);
            if (running) Release(Kb.dKey);
        }

        [UnityTest]
        public IEnumerator Jump_Apex_Is_2_2m()
        {
            yield return LoadLevel();
            yield return MeasureJump(running: false);
            KLog.Log("Measure", $"jump apex = {apex:F3} m (target 2.2 +/- 0.1), takeoff speed = {Motor.JumpSpeed:F3} m/s");
            Assert.AreEqual(2.2f, apex, 0.1f);
        }

        [UnityTest]
        public IEnumerator Jump_Distance_Is_About_8m()
        {
            yield return LoadLevel();
            yield return MeasureJump(running: true);
            KLog.Log("Measure", $"jump distance = {distance:F3} m, air time = {airTime:F3} s, apex while running = {apex:F3} m (target distance about 8)");
            Assert.AreEqual(8f, distance, 0.5f);
        }

        [UnityTest]
        public IEnumerator Player_Cannot_Reach_Exit_Ledge_Directly()
        {
            yield return LoadLevel();
            float maxY = 0f, maxX = 0f;

            Press(Kb.dKey);   // run at the ledge and jump over and over, the best a player can do
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Press(Kb.spaceKey);
                for (int i = 0; i < 35; i++)
                {
                    maxY = Mathf.Max(maxY, Body.position.y);
                    maxX = Mathf.Max(maxX, Body.position.x);
                    yield return new WaitForFixedUpdate();
                }
                Release(Kb.spaceKey);
                yield return Steps(20);
            }
            Release(Kb.dKey);

            KLog.Log("Measure", $"best feet height next to the ledge = {maxY:F2} m (ledge top is 3.6), furthest x = {maxX:F2} (ledge face is x = 11)");
            Assert.Less(maxY, 3.6f, "feet never got above the ledge top");
            Assert.Less(maxX, 11f, "player never got onto the ledge");
        }
    }
}
