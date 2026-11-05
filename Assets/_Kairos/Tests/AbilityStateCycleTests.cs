using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kairos.Tests
{
    /// <summary>cp2 acceptance: the state label cycles Ready, Targeting, Active, Cooldown, Ready with the right timings.</summary>
    public class AbilityStateCycleTests : AbilityTestFixture
    {
        // Timestamps are taken INSIDE the event callbacks (which run in Update), so they use the same
        // clock as the ability timers: game time (Time.time) built from Time.deltaTime.
        readonly List<float> ring = new List<float>();
        float castGame, castReal, revertGame, revertReal, ringStartGame, ringEndGame;
        AbilityEventChannel castChannel;
        ForceTargetEventChannel revertChannel;
        FloatEventChannel ringChannel;

        void OnCast(AbilityDefinition a) { castGame = Time.time; castReal = Time.realtimeSinceStartup; }
        void OnRevert(ForceTarget t) { revertGame = Time.time; revertReal = Time.realtimeSinceStartup; }
        void OnRing(float v)
        {
            if (ring.Count == 0) ringStartGame = Time.time;
            ring.Add(v);
            ringEndGame = Time.time;
        }

        [TearDown]
        public override void TearDown()
        {
            if (castChannel != null) castChannel.Raised -= OnCast;
            if (revertChannel != null) revertChannel.Raised -= OnRevert;
            if (ringChannel != null) ringChannel.Raised -= OnRing;
            base.TearDown();
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator State_Label_Cycles_Ready_Targeting_Active_Cooldown_Ready()
        {
            ring.Clear();   // NUnit reuses one fixture instance for all tests
            yield return LoadAbilityLevel();
            castChannel = Channel<AbilityEventChannel>("OnCastStarted");
            revertChannel = Channel<ForceTargetEventChannel>("OnEffectReverted");
            ringChannel = Channel<FloatEventChannel>("OnCooldownChanged");
            castChannel.Raised += OnCast;
            revertChannel.Raised += OnRevert;
            ringChannel.Raised += OnRing;
            var overlay = Object.FindFirstObjectByType<DebugOverlay>();

            var seen = new List<string> { Abilities.StateName };
            Assert.That(overlay.BuildText(), Does.Contain("State: ReadyState"));

            AimAt(Pod.transform.position);
            yield return HoldTarget();
            seen.Add(Abilities.StateName);
            Assert.That(overlay.BuildText(), Does.Contain("State: TargetingState"));

            yield return ClickCast();
            seen.Add(Abilities.StateName);
            Assert.That(overlay.BuildText(), Does.Contain("State: ActiveState"));
            Assert.That(overlay.BuildText(), Does.Contain("Gravity Shift / Down"));

            yield return WaitForState("CooldownState", 20f);
            seen.Add(Abilities.StateName);
            Assert.That(overlay.BuildText(), Does.Contain("State: CooldownState"));

            yield return WaitForState("ReadyState", 10f);
            seen.Add(Abilities.StateName);

            float active = revertGame - castGame, cooldown = ringEndGame - ringStartGame;
            KLog.Log("Measure", $"states = {string.Join(" > ", seen)}; active = {active:F3} s game time ({revertReal - castReal:F2} s real); " +
                                $"cooldown = {cooldown:F3} s; ring events = {ring.Count}, first = {ring[0]:F2}, last = {ring[ring.Count - 1]:F2}");

            CollectionAssert.AreEqual(
                new[] { "ReadyState", "TargetingState", "ActiveState", "CooldownState", "ReadyState" }, seen);
            Assert.AreEqual(12f, active, 0.1f, "Gravity Shift lasts 12 s of game time");
            Assert.AreEqual(2f, cooldown, 0.1f, "cooldown lasts 2 s of game time");
            Assert.AreEqual(1f, ring[0], 0.001f, "ring starts full");
            Assert.AreEqual(0f, ring[ring.Count - 1], 0.001f, "ring ends empty");
            for (int i = 1; i < ring.Count; i++)
                Assert.LessOrEqual(ring[i], ring[i - 1] + 0.0001f, "ring only drains");
        }
    }
}
