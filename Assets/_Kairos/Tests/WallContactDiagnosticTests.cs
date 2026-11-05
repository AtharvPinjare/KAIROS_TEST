using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>
    /// Pins down a known physics artefact found in cp1 (see BUILD_LOG.md): when the player RUNS into a wall,
    /// the step that reaches the wall can end up to one step of travel (6 m/s x 0.02 s = 0.12 m) inside it,
    /// because the gap before that step is larger than the 1 cm contact offset so no contact exists yet.
    /// The next steps push the capsule back out. Standing pushes and jumps from rest never show it.
    /// </summary>
    public class WallContactDiagnosticTests : PlayLevelFixture
    {
        [UnityTest]
        public IEnumerator Running_Into_Wall_Overlap_Is_At_Most_One_Step_And_Recovers()
        {
            yield return LoadLevel();
            var capsule = Body.GetComponent<CapsuleCollider>();
            var ledge = GameObject.Find("ExitLedge").GetComponent<BoxCollider>();
            float oneStep = 6f * Time.fixedDeltaTime;   // move speed x fixed step
            float worst = 0f, worstFinal = 0f;

            Press(Kb.dKey);
            for (int phase = 0; phase < 12; phase++)   // 12 start positions = 12 different approach phases
            {
                Teleport(new Vector3(8f + phase * 0.01f, 0.05f, 0f));
                for (int i = 0; i < 40; i++)
                {
                    yield return new WaitForFixedUpdate();
                    worst = Mathf.Max(worst, Overlap(capsule, ledge));
                }
                worstFinal = Mathf.Max(worstFinal, Overlap(capsule, ledge));   // still pushing, long after first contact
            }
            Release(Kb.dKey);

            KLog.Log("Measure", $"running into the ledge face: worst overlap = {worst:F3} m (one step = {oneStep:F3} m), worst settled overlap = {worstFinal:F3} m");
            Assert.LessOrEqual(worst, oneStep + 0.005f, "overlap may never exceed one step of travel");
            Assert.Less(worstFinal, 0.005f, "the solver must push the capsule back out");
        }

        /// <summary>Real overlap depth (m) between the player capsule and a box; 0 when they only touch or are apart.</summary>
        float Overlap(CapsuleCollider capsule, BoxCollider box)
        {
            Physics.SyncTransforms();
            bool hit = Physics.ComputePenetration(capsule, Body.position, Body.rotation,
                                                  box, box.transform.position, box.transform.rotation,
                                                  out _, out float depth);
            return hit ? depth : 0f;
        }
    }
}
