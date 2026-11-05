using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Kairos.EditorTools
{
    /// <summary>Builds the player, the Cinemachine 3 side-view camera and the Bootstrap object.</summary>
    public static class PlayerRigBuilder
    {
        const string FrictionlessPath = "Assets/_Kairos/Art/PlayerNoFriction.asset";

        /// <summary>Capsule 1.8 m tall, radius 0.35 m, mass 1, pivot at the feet.</summary>
        public static GameObject BuildPlayer(Vector3 start, VoidEventChannel onRespawn)
        {
            var root = new GameObject("Player") { layer = LayerMask.NameToLayer("Player") };
            root.transform.position = start;

            var body = root.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.radius = 0.35f;
            capsule.height = 1.8f;
            capsule.sharedMaterial = FrictionlessMaterial();   // zero friction so the player never sticks to walls

            var input = root.AddComponent<PlayerInputReader>();
            var motor = root.AddComponent<PlayerMotor>();
            BuildUtil.SetRef(motor, "input", input);
            BuildUtil.SetMask(motor, "groundMask", "Ground", "Targetable");

            var respawn = root.AddComponent<PlayerRespawn>();
            BuildUtil.SetRef(respawn, "input", input);
            BuildUtil.SetRef(respawn, "onPlayerRespawn", onRespawn);

            // Unity's capsule primitive is 2 m x 1 m, so scale it to 1.8 m x 0.7 m.
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());   // the root capsule is the real collider
            visual.layer = root.layer;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            visual.GetComponent<Renderer>().sharedMaterial = BuildUtil.Mat("Player", new Color(0.2f, 0.5f, 1f));
            return root;
        }

        static PhysicsMaterial FrictionlessMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(FrictionlessPath);
            if (mat == null)
            {
                mat = new PhysicsMaterial("PlayerNoFriction");
                AssetDatabase.CreateAsset(mat, FrictionlessPath);
            }
            mat.dynamicFriction = 0f;
            mat.staticFriction = 0f;
            mat.bounciness = 0f;
            mat.frictionCombine = PhysicsMaterialCombine.Minimum;   // zero wins against any surface
            mat.bounceCombine = PhysicsMaterialCombine.Minimum;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>Side view: distance 14, FOV 40, small dead zone, slight look-ahead, follows the player.</summary>
        public static void BuildCamera(Transform follow)
        {
            var main = Camera.main;
            main.transform.SetPositionAndRotation(new Vector3(follow.position.x, 3f, -14f), Quaternion.identity);
            if (main.GetComponent<CinemachineBrain>() == null) main.gameObject.AddComponent<CinemachineBrain>();

            var rig = new GameObject("CM_SideView");
            rig.transform.SetPositionAndRotation(main.transform.position, Quaternion.identity);
            var cm = rig.AddComponent<CinemachineCamera>();
            var lens = cm.Lens; lens.FieldOfView = 40f; cm.Lens = lens;
            cm.Follow = follow;

            var composer = rig.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = 14f;
            composer.TargetOffset = new Vector3(0f, 2.5f, 0f);   // lift the view so the floor sits low on screen
            var composition = composer.Composition;
            composition.DeadZone.Enabled = true;
            composition.DeadZone.Size = new Vector2(0.1f, 0.2f);   // small: the camera ignores tiny moves and hops
            composer.Composition = composition;
            var ahead = composer.Lookahead;
            ahead.Enabled = true; ahead.Time = 0.2f; ahead.Smoothing = 10f; ahead.IgnoreY = true;
            composer.Lookahead = ahead;
        }

        /// <summary>The one allowed global object.</summary>
        public static void BuildBootstrap() => new GameObject("Bootstrap").AddComponent<Bootstrap>();
    }
}
