using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>
    /// cp2 acceptance: Room 1 is solvable by the documented solution, using only injected player input:
    /// cast Gravity Shift (Down) on the pod, climb onto it, hop to the exit ledge before the 12 s run out.
    /// </summary>
    public class Room1SolutionTests : AbilityTestFixture
    {
        /// <summary>Step physics until the condition is true or the timeout (seconds of real time) passes.</summary>
        static IEnumerator Until(Func<bool> condition, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end) yield return new WaitForFixedUpdate();
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator Room1_Is_Solvable_By_The_Documented_Solution()
        {
            yield return LoadAbilityLevel();

            // 1. Gravity Shift, variant Down (the default), on the pod.
            yield return CastOnPod();
            yield return ReleaseTarget();
            yield return Steps(75);   // the pod falls and settles
            Vector3 pod = Pod.Body.position;
            KLog.Log("Measure", $"pod settled at ({pod.x:F2}, {pod.y:F2}), top = {pod.y + 1f:F2}, active left = {Abilities.ActiveRemaining:F1} s");

            // 2. Run right and jump onto the pod (take-off window is about x 2.4 to 4.8).
            Press(Kb.dKey);
            yield return Until(() => Body.position.x >= 3.6f, 3f);
            Press(Kb.spaceKey);
            yield return Steps(30);   // hold through the rise: releasing early cuts the jump short (variable-height jump)
            Release(Kb.spaceKey);
            yield return Until(() => Motor.Grounded && Body.position.y > 1.9f, 3f);
            Release(Kb.dKey);
            yield return Steps(15);
            KLog.Log("Measure", $"on the pod: x = {Body.position.x:F2}, feet y = {Body.position.y:F2}, grounded = {Motor.Grounded}");
            Assert.IsTrue(Motor.Grounded && Body.position.y > 1.9f, "could not climb onto the pod");

            // 3. Step back from the pod's right edge so the hop to the ledge starts inside its window (x about 7.7 to 8.7).
            if (Body.position.x > 8.5f)
            {
                Press(Kb.aKey);
                yield return Until(() => Body.position.x <= 8.5f, 2f);
                Release(Kb.aKey);
                yield return Steps(15);
            }

            // 4. Jump to the exit ledge (top surface at Y = 3.6, starts at X = 11).
            float takeOffX = Body.position.x;
            Press(Kb.dKey);
            Press(Kb.spaceKey);
            yield return Steps(30);
            Release(Kb.spaceKey);
            yield return Until(() => Motor.Grounded && Body.position.y > 3.4f, 3f);
            Release(Kb.dKey);
            yield return Steps(15);

            float activeLeft = Abilities.ActiveRemaining;
            KLog.Log("Measure", $"hop from x = {takeOffX:F2}; on the ledge: x = {Body.position.x:F2}, feet y = {Body.position.y:F2}, " +
                                $"grounded = {Motor.Grounded}, state = {Abilities.StateName}, active left = {activeLeft:F1} s");
            Assert.IsTrue(Motor.Grounded, "should be standing");
            Assert.AreEqual(3.6f, Body.position.y, 0.1f, "feet on the exit ledge top (Y = 3.6)");
            Assert.Greater(Body.position.x, 11f, "past the ledge's edge");
            Assert.AreEqual("ActiveState", Abilities.StateName, "solved before the effect ended");
            Assert.Greater(activeLeft, 0f);
        }
    }
}
