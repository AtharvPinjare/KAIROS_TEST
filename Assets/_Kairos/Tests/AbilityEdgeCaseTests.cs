using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>Edge cases from systems/ability-fsm.md: destroyed target, rapid selection changes.</summary>
    public class AbilityEdgeCaseTests : AbilityTestFixture
    {
        readonly List<ForceTarget> highlights = new List<ForceTarget>();
        int selectedRaised, revertedRaised;
        ForceTargetEventChannel targetChannel, revertChannel;
        AbilityEventChannel selectedChannel;
        void OnTarget(ForceTarget t) => highlights.Add(t);
        void OnSelected(AbilityDefinition a) => selectedRaised++;
        void OnRevert(ForceTarget t) => revertedRaised++;

        IEnumerator LoadAndListen()
        {
            highlights.Clear();   // NUnit reuses one fixture instance for all tests
            selectedRaised = revertedRaised = 0;
            yield return LoadAbilityLevel();
            targetChannel = Channel<ForceTargetEventChannel>("OnTargetChanged");
            revertChannel = Channel<ForceTargetEventChannel>("OnEffectReverted");
            selectedChannel = Channel<AbilityEventChannel>("OnAbilitySelected");
            targetChannel.Raised += OnTarget;
            revertChannel.Raised += OnRevert;
            selectedChannel.Raised += OnSelected;
        }

        [TearDown]
        public override void TearDown()
        {
            if (targetChannel != null) targetChannel.Raised -= OnTarget;
            if (revertChannel != null) revertChannel.Raised -= OnRevert;
            if (selectedChannel != null) selectedChannel.Raised -= OnSelected;
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Target_Destroyed_During_Active_Goes_To_Cooldown_Without_Errors()
        {
            yield return LoadAndListen();
            yield return CastOnPod();
            yield return Steps(25);
            Assert.AreEqual("ActiveState", Abilities.StateName);

            Object.Destroy(Pod.gameObject);   // the target vanishes mid-effect
            yield return Steps(6);

            KLog.Log("Measure", $"after destroy: state = {Abilities.StateName}, reverts raised = {revertedRaised}");
            Assert.AreEqual("CooldownState", Abilities.StateName);
            Assert.AreEqual(0, revertedRaised, "nothing left to revert, so no revert event");
            yield return WaitForState("ReadyState", 5f);   // and the machine recovers fully
        }

        [UnityTest]
        public IEnumerator Rapid_Selection_Changes_While_Targeting_Leave_No_Stale_Highlight()
        {
            yield return LoadAndListen();
            int selectedAtStart = selectedRaised;   // Start() selects slot 0 once
            AimAt(Pod.transform.position);
            yield return HoldTarget();
            Assert.AreSame(Pod, Abilities.Target, "pod is highlighted while aiming");

            foreach (var key in new[] { Kb.digit2Key, Kb.digit3Key, Kb.digit4Key, Kb.digit1Key, Kb.rKey })
            {
                Press(key);
                yield return Steps(2);
                Release(key);
                yield return Steps(1);
            }
            yield return ReleaseTarget();

            KLog.Log("Measure", $"highlight events = {highlights.Count} (last = {(highlights[highlights.Count - 1] == null ? "null" : "target")}), " +
                                $"selected events = {selectedRaised - selectedAtStart}, variant = {Abilities.Variant}, state = {Abilities.StateName}");
            Assert.AreEqual(2, highlights.Count, "exactly one highlight on and one off");
            Assert.AreSame(Pod, highlights[0]);
            Assert.IsNull(highlights[highlights.Count - 1], "last event clears the highlight");
            Assert.IsNull(Abilities.Target);
            Assert.AreEqual("ReadyState", Abilities.StateName);
            Assert.AreEqual(1, selectedRaised - selectedAtStart, "only key 1 has an ability; empty slots are ignored");
            Assert.AreEqual(1, Abilities.Variant, "R advanced the variant from Down to Up");
        }
    }
}
