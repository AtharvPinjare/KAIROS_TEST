using UnityEditor;
using UnityEngine;

namespace Kairos.EditorTools
{
    /// <summary>
    /// Scene wiring for the ability system: the controller and debug overlay on the player,
    /// and the Rigidbody + ForceTarget parts on a prop. Called by the room builders.
    /// </summary>
    public static class AbilityRigBuilder
    {
        /// <summary>Add AbilityController (all channels wired) and a DebugOverlay.</summary>
        public static void AddToPlayer(GameObject player)
        {
            var input = player.GetComponent<PlayerInputReader>();
            var loadout = AbilityAssetBuilder.Loadout(AbilityAssetBuilder.GravityShift());

            var controller = player.AddComponent<AbilityController>();
            BuildUtil.SetRef(controller, "input", input);
            BuildUtil.SetRef(controller, "loadout", loadout);
            BuildUtil.SetRef(controller, "source", player.transform);
            BuildUtil.SetMask(controller, "finder.targetableMask", "Targetable");
            WireChannels(controller);

            var overlay = new GameObject("DebugOverlay").AddComponent<DebugOverlay>();
            BuildUtil.SetRef(overlay, "abilities", controller);
            BuildUtil.SetRef(overlay, "input", input);
        }

        static void WireChannels(AbilityController c)
        {
            const string p = "channels.";
            BuildUtil.SetRef(c, p + "onAbilitySelected", AbilityAssetBuilder.Channel<AbilityEventChannel>("OnAbilitySelected"));
            BuildUtil.SetRef(c, p + "onVariantChanged", AbilityAssetBuilder.Channel<IntEventChannel>("OnVariantChanged"));
            BuildUtil.SetRef(c, p + "onTargetChanged", AbilityAssetBuilder.Channel<ForceTargetEventChannel>("OnTargetChanged"));
            BuildUtil.SetRef(c, p + "onTargetingStarted", AbilityAssetBuilder.Channel<VoidEventChannel>("OnTargetingStarted"));
            BuildUtil.SetRef(c, p + "onTargetingEnded", AbilityAssetBuilder.Channel<VoidEventChannel>("OnTargetingEnded"));
            BuildUtil.SetRef(c, p + "onCastStarted", AbilityAssetBuilder.Channel<AbilityEventChannel>("OnCastStarted"));
            BuildUtil.SetRef(c, p + "onEffectApplied", AbilityAssetBuilder.Channel<ForceTargetEventChannel>("OnEffectApplied"));
            BuildUtil.SetRef(c, p + "onEffectReverted", AbilityAssetBuilder.Channel<ForceTargetEventChannel>("OnEffectReverted"));
            BuildUtil.SetRef(c, p + "onCastRejected", AbilityAssetBuilder.Channel<RejectEventChannel>("OnCastRejected"));
            BuildUtil.SetRef(c, p + "onCooldownChanged", AbilityAssetBuilder.Channel<FloatEventChannel>("OnCooldownChanged"));
            BuildUtil.SetRef(c, p + "onPlayerRespawn", AbilityAssetBuilder.Channel<VoidEventChannel>("OnPlayerRespawn"));
        }

        /// <summary>
        /// Turn a cube into a prop abilities can affect: Rigidbody, ForceTarget, attributes, custom gravity.
        /// Slides freely in X/Y, locked in depth and rotation so it stays in the 2.5D plane and lands flat.
        /// </summary>
        public static ForceTarget MakeForceTarget(GameObject prop, float mass, bool gravityStartsUp)
        {
            var target = prop.AddComponent<ForceTarget>();   // also adds Rigidbody, ObjectAttributes, CustomGravity
            var body = prop.GetComponent<Rigidbody>();
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            var gravity = new SerializedObject(prop.GetComponent<CustomGravity>());
            gravity.FindProperty("startsEnabled").boolValue = gravityStartsUp;
            gravity.FindProperty("startDirection").vector3Value = Vector3.up;
            gravity.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }
    }
}
