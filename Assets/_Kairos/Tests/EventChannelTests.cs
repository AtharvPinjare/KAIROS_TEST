using NUnit.Framework;
using UnityEngine;

namespace Kairos.Tests
{
    /// <summary>systems/events-data.md: every channel delivers its payload to a listener, and unsubscribing stops delivery.</summary>
    public class EventChannelTests
    {
        static T Make<T>() where T : ScriptableObject => ScriptableObject.CreateInstance<T>();

        [Test]
        public void VoidChannel_Delivers()
        {
            var channel = Make<VoidEventChannel>();
            int count = 0;
            void Listener() => count++;
            channel.Raised += Listener;
            channel.Raise();
            channel.Raised -= Listener;
            channel.Raise();
            Assert.AreEqual(1, count, "one delivery, none after unsubscribe");
        }

        [Test]
        public void FloatChannel_Delivers_Payload()
        {
            var channel = Make<FloatEventChannel>();
            float got = -1f;
            channel.Raised += v => got = v;
            channel.Raise(0.25f);
            Assert.AreEqual(0.25f, got);
        }

        [Test]
        public void IntChannel_Delivers_Payload()
        {
            var channel = Make<IntEventChannel>();
            int got = -1;
            channel.Raised += v => got = v;
            channel.Raise(3);
            Assert.AreEqual(3, got);
        }

        [Test]
        public void RejectChannel_Delivers_Reason()
        {
            var channel = Make<RejectEventChannel>();
            var got = CastRejectReason.None;
            channel.Raised += r => got = r;
            channel.Raise(CastRejectReason.NotMagnetic);
            Assert.AreEqual(CastRejectReason.NotMagnetic, got);
        }

        [Test]
        public void AbilityChannel_Delivers_Ability()
        {
            var channel = Make<AbilityEventChannel>();
            var ability = Make<GravityShiftAbility>();
            AbilityDefinition got = null;
            channel.Raised += a => got = a;
            channel.Raise(ability);
            Assert.AreSame(ability, got);
        }

        [Test]
        public void ForceTargetChannel_Delivers_Target_And_Null()
        {
            var channel = Make<ForceTargetEventChannel>();
            var go = new GameObject("Prop", typeof(BoxCollider));
            var target = go.AddComponent<ForceTarget>();
            ForceTarget got = null;
            int count = 0;
            channel.Raised += t => { got = t; count++; };

            channel.Raise(target);
            Assert.AreSame(target, got);
            channel.Raise(null);   // null means "no target"
            Assert.IsNull(got);
            Assert.AreEqual(2, count);
            Object.DestroyImmediate(go);
        }
    }
}
