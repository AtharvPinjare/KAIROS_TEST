using NUnit.Framework;

namespace Kairos.Tests
{
    /// <summary>The state machine on its own, with fake states (no scene needed).</summary>
    public class AbilityStateMachineTests
    {
        class FakeState : IAbilityState
        {
            public int Entered, Exited, Ticked;
            public void Enter() => Entered++;
            public void Tick() => Ticked++;
            public void Exit() => Exited++;
        }

        [Test]
        public void ChangeState_To_The_Current_State_Does_Nothing()
        {
            var machine = new AbilityStateMachine();
            var a = new FakeState();
            machine.Initialize(a);

            machine.ChangeState(a);

            Assert.AreEqual(1, a.Entered, "Enter must not run twice");
            Assert.AreEqual(0, a.Exited, "Exit must not run");
            Assert.AreSame(a, machine.Current);
        }

        [Test]
        public void ChangeState_To_Null_Is_Ignored()
        {
            var machine = new AbilityStateMachine();
            var a = new FakeState();
            machine.Initialize(a);

            machine.ChangeState(null);

            Assert.AreSame(a, machine.Current);
            Assert.AreEqual(0, a.Exited);
        }

        [Test]
        public void ChangeState_Exits_Old_Then_Enters_New_And_Tick_Goes_To_New()
        {
            var machine = new AbilityStateMachine();
            var a = new FakeState();
            var b = new FakeState();
            machine.Initialize(a);

            machine.ChangeState(b);
            machine.Tick();

            Assert.AreEqual(1, a.Exited);
            Assert.AreEqual(1, b.Entered);
            Assert.AreEqual(0, a.Ticked);
            Assert.AreEqual(1, b.Ticked);
            Assert.AreSame(b, machine.Current);
        }
    }
}
