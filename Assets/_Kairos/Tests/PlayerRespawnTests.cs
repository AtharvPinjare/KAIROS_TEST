using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>cp1 acceptance: respawn with T and by falling into the KillZone, each raising OnPlayerRespawn exactly once.</summary>
    public class PlayerRespawnTests : PlayLevelFixture
    {
        int raised;
        VoidEventChannel channel;
        void CountRaise() => raised++;

        IEnumerator LoadAndListen()
        {
            raised = 0;   // NUnit reuses one fixture instance for every test, so reset the counter each time
            yield return LoadLevel();
            // The channel asset is already loaded because the scene references it.
            channel = Resources.FindObjectsOfTypeAll<VoidEventChannel>().First(c => c.name == "OnPlayerRespawn");
            channel.Raised += CountRaise;
        }

        [TearDown]
        public override void TearDown()
        {
            if (channel != null) channel.Raised -= CountRaise;
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator T_Key_Returns_Player_To_Checkpoint()
        {
            yield return LoadAndListen();

            Press(Kb.dKey);
            yield return Steps(60);   // run right for about 1.2 s
            Release(Kb.dKey);
            float movedTo = Body.position.x;
            Assert.Greater(movedTo, 5f, "player should have moved away from the checkpoint first");

            Press(Kb.tKey);
            yield return Steps(5);
            Release(Kb.tKey);

            KLog.Log("Measure", $"moved to x = {movedTo:F2}, after T: x = {Body.position.x:F2}, y = {Body.position.y:F2}, speed = {Body.linearVelocity.magnitude:F2}");
            Assert.AreEqual(1f, Body.position.x, 0.1f);
            Assert.Less(Body.linearVelocity.magnitude, 0.5f);
            Assert.AreEqual(1, raised, "OnPlayerRespawn should be raised exactly once");
        }

        [UnityTest]
        public IEnumerator KillZone_Returns_Player_To_Checkpoint()
        {
            yield return LoadAndListen();

            Teleport(new Vector3(5f, -7.5f, 0f));   // inside the KillZone volume (Y -9 to -7)
            yield return Steps(5);

            KLog.Log("Measure", $"after KillZone: x = {Body.position.x:F2}, y = {Body.position.y:F2}");
            Assert.AreEqual(1f, Body.position.x, 0.1f);
            Assert.Greater(Body.position.y, -1f);
            Assert.AreEqual(1, raised, "OnPlayerRespawn should be raised exactly once");
        }
    }
}
