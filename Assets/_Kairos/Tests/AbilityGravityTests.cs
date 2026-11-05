using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>cp2 acceptance: Gravity Shift flips the pod for 12 s then reverts; respawn restores it.</summary>
    public class AbilityGravityTests : AbilityTestFixture
    {
        ForceTargetEventChannel revertChannel;
        int reverted;
        void OnRevert(ForceTarget t) => reverted++;

        IEnumerator LoadAndListen()
        {
            reverted = 0;   // NUnit reuses one fixture instance for all tests
            yield return LoadAbilityLevel();
            revertChannel = Channel<ForceTargetEventChannel>("OnEffectReverted");
            revertChannel.Raised += OnRevert;
        }

        [TearDown]
        public override void TearDown()
        {
            if (revertChannel != null) revertChannel.Raised -= OnRevert;
            base.TearDown();
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator Gravity_Flips_For_12s_Then_Pod_Returns_To_Ceiling()
        {
            yield return LoadAndListen();
            Assert.AreEqual(7f, Pod.Body.position.y, 0.1f, "pod starts against the ceiling");
            Assert.IsTrue(Pod.Gravity.Enabled);
            Assert.AreEqual(Vector3.up, Pod.Gravity.Direction);

            yield return CastOnPod();
            yield return Steps(100);   // 2 s of falling
            Vector3 landed = Pod.Body.position;
            Vector3 flippedDirection = Pod.Gravity.Direction;
            KLog.Log("Measure", $"2 s after cast: pod at ({landed.x:F2}, {landed.y:F2}), top = {landed.y + 1f:F2}, gravity = {flippedDirection}");
            Assert.AreEqual(Vector3.down, flippedDirection);
            Assert.AreEqual(9f, landed.x, 0.3f, "pod lands at X about 9");
            Assert.AreEqual(2f, landed.y + 1f, 0.1f, "pod top surface at Y = 2.0");

            yield return WaitForState("CooldownState", 15f);
            Assert.AreEqual(Vector3.up, Pod.Gravity.Direction, "revert restores gravity to Up");
            Assert.IsTrue(Pod.Gravity.Enabled);
            Assert.AreEqual(1, reverted, "OnEffectReverted raised exactly once");

            yield return Steps(100);   // 2 s of rising
            KLog.Log("Measure", $"2 s after revert: pod y = {Pod.Body.position.y:F2}");
            Assert.AreEqual(7f, Pod.Body.position.y, 0.15f, "pod is back on the ceiling");
            Assert.AreEqual(1, reverted, "still exactly one revert");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator ForceReset_On_Respawn_During_Active_Restores_Pod()
        {
            yield return LoadAndListen();
            yield return CastOnPod();
            yield return Steps(100);
            Assert.AreEqual("ActiveState", Abilities.StateName);
            Assert.Less(Pod.Body.position.y, 1.2f, "pod has fallen to the floor");

            Press(Kb.tKey);   // respawn: PlayerRespawn raises OnPlayerRespawn, the controller calls ForceReset
            yield return Steps(4);
            Release(Kb.tKey);
            yield return Steps(2);

            KLog.Log("Measure", $"after respawn: state = {Abilities.StateName}, gravity = {Pod.Gravity.Direction}, reverts = {reverted}");
            Assert.AreEqual("ReadyState", Abilities.StateName);
            Assert.AreEqual(Vector3.up, Pod.Gravity.Direction);
            Assert.IsTrue(Pod.Gravity.Enabled);
            Assert.IsNull(Abilities.Target);
            Assert.AreEqual(0f, Abilities.ActiveRemaining, 0.001f);
            Assert.AreEqual(1, reverted, "OnEffectReverted raised exactly once");

            yield return Steps(100);
            Assert.AreEqual(7f, Pod.Body.position.y, 0.15f, "pod floats back to the ceiling");
        }
    }
}
