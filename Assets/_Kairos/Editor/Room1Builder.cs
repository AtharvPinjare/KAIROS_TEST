using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kairos.EditorTools
{
    /// <summary>
    /// Tools/Kairos/Build Room 1: rebuilds Level_ContainmentWing with the greybox Room 1
    /// from docs/03_LEVEL_SPEC.md. Re-runnable; it replaces the scene each time.
    /// All numbers below are the level spec's numbers (metres, play plane Z = 0, floor top Y = 0).
    /// </summary>
    public static class Room1Builder
    {
        public const string ScenePath = "Assets/_Kairos/Scenes/Level_ContainmentWing.unity";
        const string Events = "Assets/_Kairos/ScriptableObjects/Events";
        const float Depth = 4f;   // Z thickness of solid pieces; the player is locked to Z = 0 inside it

        [MenuItem("Tools/Kairos/Build Room 1")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var onRespawn = BuildUtil.LoadOrCreate<VoidEventChannel>($"{Events}/OnPlayerRespawn.asset");
            var onCheckpoint = BuildUtil.LoadOrCreate<VoidEventChannel>($"{Events}/OnCheckpointReached.asset");

            var room = new GameObject("Room1_CargoShaft").transform;
            BuildShell(room);
            BuildCheckpoint(room, onCheckpoint);
            BuildKillZone();

            var player = PlayerRigBuilder.BuildPlayer(new Vector3(1f, 0.05f, 0f), onRespawn);
            PlayerRigBuilder.BuildCamera(player.transform);
            PlayerRigBuilder.BuildBootstrap();

            BuildUtil.EnsureFolder("Assets/_Kairos/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            KLog.Log("Build", $"Room 1 built and saved to {ScenePath}");
        }

        static void BuildShell(Transform room)
        {
            var ground = BuildUtil.Mat("Ground", new Color(0.45f, 0.45f, 0.48f));
            var target = BuildUtil.Mat("Targetable", new Color(1f, 0.5f, 0.1f));
            var door = BuildUtil.Mat("Door", new Color(0.6f, 0.2f, 0.2f));
            var backdrop = BuildUtil.Mat("Backdrop", new Color(0.18f, 0.18f, 0.2f));

            // Floor: X 0-24, top surface at Y = 0.
            BuildUtil.Box("Floor", new Vector3(12f, -0.5f, 0f), new Vector3(24f, 1f, Depth), room, ground, "Ground");
            // Ceiling: underside at Y = 8.
            BuildUtil.Box("Ceiling", new Vector3(12f, 8.5f, 0f), new Vector3(24f, 1f, Depth), room, ground, "Ground");
            // End walls so the player cannot leave the room sideways.
            BuildUtil.Box("Wall_Left", new Vector3(-0.5f, 4f, 0f), new Vector3(1f, 10f, Depth), room, ground, "Ground");
            BuildUtil.Box("Wall_Right", new Vector3(24.5f, 4f, 0f), new Vector3(1f, 10f, Depth), room, ground, "Ground");
            // Exit ledge: top surface Y = 3.6 (above the 1.8 safe step and the 2.2 jump apex), X 11-24.
            BuildUtil.Box("ExitLedge", new Vector3(17.5f, 1.8f, 0f), new Vector3(13f, 3.6f, Depth), room, ground, "Ground");
            // Placeholder for the R1->R2 door (cp3 replaces it): X 23-24, Y 3.6-8.
            BuildUtil.Box("Door_R1R2_Placeholder", new Vector3(23.5f, 5.8f, 0f), new Vector3(1f, 4.4f, Depth), room, door, "Ground");
            // Cargo pod 2x2x2 at (9, 7), touching the ceiling. Static in cp1; cp2 gives it a Rigidbody and ForceTarget.
            BuildUtil.Box("CargoPod", new Vector3(9f, 7f, 0f), new Vector3(2f, 2f, 2f), room, target, "Targetable");
            // Backdrop behind the play plane for depth only (no collider).
            BuildUtil.Box("Backdrop", new Vector3(12f, 4f, 2.5f), new Vector3(24f, 8f, 0.2f), room, backdrop, "Default", false);
        }

        static void BuildCheckpoint(Transform room, VoidEventChannel onCheckpoint)
        {
            var go = Trigger("Checkpoint_1", new Vector3(1f, 1.5f, 0f), new Vector3(1f, 3f, Depth), room);
            var point = new GameObject("RespawnPoint");
            point.transform.SetParent(go.transform, false);
            point.transform.position = new Vector3(1f, 0.05f, 0f);

            var checkpoint = go.AddComponent<Checkpoint>();
            BuildUtil.SetRef(checkpoint, "respawnPoint", point.transform);
            BuildUtil.SetRef(checkpoint, "onCheckpointReached", onCheckpoint);
        }

        static void BuildKillZone()
        {
            // Below Y = -6, wide enough to cover all three rooms later.
            var go = Trigger("KillZone", new Vector3(50f, -8f, 0f), new Vector3(200f, 2f, 10f), null);
            go.AddComponent<KillZone>();
        }

        static GameObject Trigger(string name, Vector3 centre, Vector3 size, Transform parent)
        {
            var go = new GameObject(name) { layer = LayerMask.NameToLayer("Interactable") };
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            box.isTrigger = true;
            return go;
        }
    }
}
