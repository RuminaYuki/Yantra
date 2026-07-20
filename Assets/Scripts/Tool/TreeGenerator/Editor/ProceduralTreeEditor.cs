using System.Text;
using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>
    /// Inspector for ProceduralTree: live rebuild on any change, dice buttons
    /// for the three seeds, per-LOD stats and mesh export.
    /// </summary>
    [CustomEditor(typeof(ProceduralTree))]
    public class ProceduralTreeEditor : Editor
    {
        SerializedProperty _settings;
        SerializedProperty _barkMaterial;
        SerializedProperty _leafMaterial;

        void OnEnable()
        {
            _settings = serializedObject.FindProperty("settings");
            _barkMaterial = serializedObject.FindProperty("barkMaterial");
            _leafMaterial = serializedObject.FindProperty("leafMaterial");
        }

        public override void OnInspectorGUI()
        {
            var tree = (ProceduralTree)target;
            serializedObject.Update();

            EditorGUILayout.LabelField("Random Seeds", EditorStyles.boldLabel);
            DrawSeedRow(_settings.FindPropertyRelative("seed"), "New Tree");
            DrawSeedRow(_settings.FindPropertyRelative("branchSeed"), "Shuffle");
            DrawSeedRow(_settings.FindPropertyRelative("leafSeed"), "Shuffle");

            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("geometry"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("trunk"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("branchLevels"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("leaves"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("mesh"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("wind"), true);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("lods"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_barkMaterial);
            EditorGUILayout.PropertyField(_leafMaterial);

            // Applying triggers OnValidate -> queued rebuild -> instant update while dragging.
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild Now"))
                    tree.Rebuild();
                if (GUILayout.Button("Export Meshes To Assets"))
                    ExportMeshes(tree);
            }

            DrawStats(tree);
        }

        void DrawSeedRow(SerializedProperty seedProp, string buttonLabel)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(seedProp);
                if (GUILayout.Button(buttonLabel, GUILayout.Width(72f)))
                    seedProp.intValue = Random.Range(0, 9999999);
            }
        }

        static void DrawStats(ProceduralTree tree)
        {
            if (tree.Stats.Count == 0)
                return;
            var sb = new StringBuilder();
            sb.Append("Branches: ").Append(tree.BranchCount);
            foreach (var s in tree.Stats)
            {
                sb.AppendLine();
                sb.Append(s.label)
                  .Append("  verts ").Append(s.vertices.ToString("N0"))
                  .Append("  tris ").Append(s.triangles.ToString("N0"))
                  .Append("  leaves ").Append(s.leaves.ToString("N0"));
            }
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);
        }

        static void ExportMeshes(ProceduralTree tree)
        {
            const string root = "Assets/GeneratedTrees";
            if (!AssetDatabase.IsValidFolder(root))
                AssetDatabase.CreateFolder("Assets", "GeneratedTrees");
            string folder = root + "/" + tree.name;
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(root, tree.name);

            int exported = 0;
            foreach (Transform child in tree.transform)
            {
                if (!child.name.StartsWith("LOD"))
                    continue;
                var mf = child.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    continue;

                var copy = Object.Instantiate(mf.sharedMesh);
                copy.name = tree.name + "_" + child.name;
                copy.hideFlags = HideFlags.None;
                string path = $"{folder}/{copy.name}.asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(copy, path);
                mf.sharedMesh = copy;
                exported++;
            }
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Procedural Tree",
                $"Exported {exported} mesh(es) to {folder}\n\n" +
                "The tree now uses the exported assets, so you can turn it into a prefab.\n" +
                "Note: changing any setting will regenerate procedural meshes again " +
                "(the exported assets stay on disk).",
                "OK");
        }

        [MenuItem("GameObject/3D Object/Procedural Tree (Tool)", false, 10)]
        static void CreateTree(MenuCommand cmd)
        {
            var go = new GameObject("Procedural Tree");
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            var tree = go.AddComponent<ProceduralTree>();
            Undo.RegisterCreatedObjectUndo(go, "Create Procedural Tree");
            Selection.activeGameObject = go;
            tree.Rebuild();
        }
    }
}
