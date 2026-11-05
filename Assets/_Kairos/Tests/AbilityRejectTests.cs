using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>cp2 acceptance: a rejected cast stays in Targeting, raises OnCastRejected with the right reason, uses no cooldown.</summary>
    public class AbilityRejectTests : AbilityTestFixture
    {
        readonly List<CastRejectReason> reasons = new List<CastRejectReason>();
        RejectEventChannel rejectChannel;
        AbilityDefinition shortened;
        float originalDuration;
        void OnReject(CastRejectReason r) => reasons.Add(r);

        IEnumerator LoadAndListen()
        {
            reasons.Clear();   // NUnit reuses one fixture instance for all tests
            yield return LoadAbilityLevel();
            rejectChannel = Channel<RejectEventChannel>("OnCastRejected");
            rejectChannel.Raised += OnReject;
        }

        [TearDown]
        public override void TearDown()
        {
            if (rejectChannel != null) rejectChannel.Raised -= OnReject;
            if (shortened != null) shortened.Duration = originalDuration;   // the asset is shared: always put it back
            shortened = null;
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Cast_With_No_Target_Is_Rejected_NoTarget_And_Stays_In_Targeting()
        {
            yield return LoadAndListen();
            AimAt(new Vector3(3f, 3f, 0f));   // empty air
            yield return HoldTarget();
            yield return ClickCast();

            KLog.Log("Measure", $"no target: reasons = [{string.Join(", ", reasons)}], state = {Abilities.StateName}, cooldown = {Abilities.CooldownRemaining}");
            CollectionAssert.AreEqual(new[] { CastRejectReason.NoTarget }, reasons);
            Assert.AreEqual("TargetingState", Abilities.StateName);
            Assert.AreEqual(0f, Abilities.CooldownRemaining, "a reject does not consume the cooldown");
            Assert.AreEqual(Vector3.up, Pod.Gravity.Direction, "nothing was changed");
        }

        [UnityTest]
        public IEnumerator Cast_On_Locked_Target_Is_Rejected_CapabilityLocked_Then_Retry_Works()
        {
            yield return LoadAndListen();
            Pod.Attributes.canManipulateGravity = false;
            AimAt(Pod.transform.position);
            yield return HoldTarget();
            yield return ClickCast();

            KLog.Log("Measure", $"locked: reasons = [{string.Join(", ", reasons)}], state = {Abilities.StateName}");
            CollectionAssert.AreEqual(new[] { CastRejectReason.CapabilityLocked }, reasons);
            Assert.AreEqual("TargetingState", Abilities.StateName);
            Assert.AreEqual(0f, Abilities.CooldownRemaining);

            Pod.Attributes.canManipulateGravity = true;   // unlock and click again: still allowed, no cooldown was spent
            yield return ClickCast();
            Assert.AreEqual("ActiveState", Abilities.StateName);
            Assert.AreEqual(1, reasons.Count, "no further rejects");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator Cast_Attempt_During_Cooldown_Reports_OnCooldown()
        {
            yield return LoadAndListen();
            shortened = Abilities.Selected;
            originalDuration = shortened.Duration;
            shortened.Duration = 0.5f;   // so the test does not wait 12 s; restored in TearDown

            yield return CastOnPod();
            yield return WaitForState("CooldownState", 5f);
            var reason = Abilities.TryCast();
            KLog.Log("Measure", $"during cooldown: TryCast() = {reason}, remaining = {Abilities.CooldownRemaining:F2}");
            Assert.AreEqual(CastRejectReason.OnCooldown, reason);
        }
    }
}
