using UnityEditor;
using UnityEngine;

namespace Kairos.EditorTools
{
    /// <summary>
    /// Small helpers shared by the Editor menu builders (Tools/Kairos/...).
    /// Builders are re-runnable: they overwrite what they made last time.
    /// </summary>
    public static class BuildUtil
    {
        /// <summary>Create every missing folder in a path such as "Assets/_Kairos/Art/Materials".</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        /// <summary>Load a ScriptableObject asset, or create it if it does not exist yet.</summary>
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>A flat-colour URP Lit material saved under Art/Materials (greybox colours per role).</summary>
        public static Material Mat(string name, Color colour)
        {
            const string folder = "Assets/_Kairos/Art/Materials";
            EnsureFolder(folder);
            string path = $"{folder}/M_{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", colour);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>A cube centred at <paramref name="centre"/> with the given size, on a named layer.</summary>
        public static GameObject Box(string name, Vector3 centre, Vector3 size, Transform parent,
                                     Material mat, string layer, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = centre;
            go.transform.localScale = size;
            go.layer = LayerMask.NameToLayer(layer);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>Set a private [SerializeField] object reference by field name.</summary>
        public static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Set a private [SerializeField] LayerMask by layer names.</summary>
        public static void SetMask(Object target, string field, params string[] layers)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = LayerMask.GetMask(layers);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
