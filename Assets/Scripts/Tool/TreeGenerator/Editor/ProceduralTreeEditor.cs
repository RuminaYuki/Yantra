using System.Text;
using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>
    /// Inspector for ProceduralTree.
    /// - Live rebuild while dragging: only LOD0 without tangents is rebuilt per
    ///   change (fast), then a full rebuild (all LODs + tangents + LODGroup)
    ///   runs automatically ~0.35s after the last change.
    /// - Geometry section hides the prefab lists in Procedural mode and the
    ///   procedural-only fields in Prefabs mode.
    /// - Distances edited directly on the LODGroup component are pulled back
    ///   into the tool settings.
    /// </summary>
    [CustomEditor(typeof(ProceduralTree))]
    public class ProceduralTreeEditor : Editor
    {
        SerializedProperty _settings;
        SerializedProperty _barkMaterial;
        SerializedProperty _leafMaterial;

        bool _fullRebuildPending;
        double _fullRebuildDue;

        void OnEnable()
        {
            _settings = serializedObject.FindProperty("settings");
            _barkMaterial = serializedObject.FindProperty("barkMaterial");
            _leafMaterial = serializedObject.FindProperty("leafMaterial");
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (_fullRebuildPending && target != null)
            {
                _fullRebuildPending = false;
                ((ProceduralTree)target).Rebuild(false);
            }
        }

        void OnEditorUpdate()
        {
            if (!_fullRebuildPending || EditorApplication.timeSinceStartup < _fullRebuildDue)
                return;
            _fullRebuildPending = false;
            if (target != null)
            {
                ((ProceduralTree)target).Rebuild(false);
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            var tree = (ProceduralTree)target;

            // user may have dragged the LODGroup sliders - reflect that here
            tree.AdoptLODGroupOverrides();

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Random Seeds", EditorStyles.boldLabel);
            DrawSeedRow(_settings.FindPropertyRelative("seed"), "New Tree");
            DrawSeedRow(_settings.FindPropertyRelative("branchSeed"), "Shuffle");
            DrawSeedRow(_settings.FindPropertyRelative("leafSeed"), "Shuffle");

            EditorGUILayout.Space(6f);
            DrawGeometrySection();
            // radial segments only matter for procedural tubes
            DrawGroup(_settings.FindPropertyRelative("trunk"),
                      IsPrefabSource("trunkSource") ? new[] { "radialSegments" } : null);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("branchLevels"), true);
            DrawLeavesSection();
            DrawMeshSection();
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("wind"), true);
            DrawWindZoneHint();
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("lods"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_barkMaterial);
            EditorGUILayout.PropertyField(_leafMaterial);

            bool changed = EditorGUI.EndChangeCheck();

            // OnValidate is suppressed while the editor drives the rebuild
            // itself, so a slider drag costs exactly one cheap rebuild per tick.
            ProceduralTree.EditorIsDriving = true;
            serializedObject.ApplyModifiedProperties();
            ProceduralTree.EditorIsDriving = false;

            if (changed)
            {
                tree.Rebuild(interactive: true);
                _fullRebuildPending = true;
                _fullRebuildDue = EditorApplication.timeSinceStartup + 0.35;
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild Now"))
                    tree.Rebuild(false);
                if (GUILayout.Button("Export Meshes To Assets"))
                    ExportMeshes(tree);
            }
            if (GUILayout.Button("Export & Add To Terrain As Paintable Tree"))
                ExportAndAddToTerrain(tree);

            DrawStats(tree);
        }

        // ------------------------------------------------------------------
        // Sections with mode-dependent visibility
        // ------------------------------------------------------------------

        void DrawGeometrySection()
        {
            SerializedProperty geo = _settings.FindPropertyRelative("geometry");
            geo.isExpanded = EditorGUILayout.Foldout(geo.isExpanded, "Geometry", true);
            if (!geo.isExpanded)
                return;

            EditorGUI.indentLevel++;
            DrawSourceWithList(geo, "trunkSource", "trunkPrefabs");
            DrawSourceWithList(geo, "branchSource", "branchPrefabs");
            DrawSourceWithList(geo, "leafSource", "leafPrefabs");
            EditorGUI.indentLevel--;
        }

        static void DrawSourceWithList(SerializedProperty geo, string sourceName, string listName)
        {
            SerializedProperty source = geo.FindPropertyRelative(sourceName);
            EditorGUILayout.PropertyField(source);
            if (source.enumValueIndex == (int)GeometrySource.Prefabs)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(geo.FindPropertyRelative(listName), true);
                EditorGUI.indentLevel--;
            }
        }

        bool IsPrefabSource(string sourceName)
            => _settings.FindPropertyRelative("geometry").FindPropertyRelative(sourceName)
                   .enumValueIndex == (int)GeometrySource.Prefabs;

        void DrawLeavesSection()
        {
            // "shape" only matters for procedural cards
            DrawGroup(_settings.FindPropertyRelative("leaves"),
                      IsPrefabSource("leafSource") ? new[] { "shape" } : null);
        }

        void DrawMeshSection()
        {
            // bark UV tiling only matters for procedural tubes
            string[] hidden = IsPrefabSource("trunkSource") && IsPrefabSource("branchSource")
                ? new[] { "barkUVTiling" }
                : null;
            DrawGroup(_settings.FindPropertyRelative("mesh"), hidden);
        }

        /// <summary>Draws a settings group foldout, skipping the given child fields.</summary>
        static void DrawGroup(SerializedProperty group, string[] hiddenFields)
        {
            group.isExpanded = EditorGUILayout.Foldout(group.isExpanded, group.displayName, true);
            if (!group.isExpanded)
                return;

            EditorGUI.indentLevel++;
            SerializedProperty it = group.Copy();
            SerializedProperty end = it.GetEndProperty();
            it.NextVisible(true);
            while (!SerializedProperty.EqualContents(it, end))
            {
                if (hiddenFields == null || System.Array.IndexOf(hiddenFields, it.name) < 0)
                    EditorGUILayout.PropertyField(it, true);
                if (!it.NextVisible(false))
                    break;
            }
            EditorGUI.indentLevel--;
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Wind response values only move anything once a real WindZone is in the
        /// scene driving TreeWindZoneDriver (see TreeWind.hlsl). Offer a one-click
        /// setup so this isn't a silent no-op.
        /// </summary>
        static void DrawWindZoneHint()
        {
            var driver = Object.FindAnyObjectByType<TreeWindZoneDriver>();
            if (driver != null)
                return;

            EditorGUILayout.HelpBox(
                "Wind response values are baked into the mesh, but nothing moves yet: " +
                "no Tree Wind Zone Driver is in the scene. Add one (plus a WindZone) to " +
                "drive every tree's wind-aware material from real Unity wind.",
                MessageType.Info);
            if (GUILayout.Button("Add Wind Zone To Scene"))
                CreateWindZoneSetup();
        }

        static void CreateWindZoneSetup()
        {
            var go = new GameObject("Wind Zone");
            var zone = go.AddComponent<WindZone>();
            zone.mode = WindZoneMode.Directional;
            zone.windMain = 0.5f;
            zone.windTurbulence = 0.25f;
            zone.windPulseMagnitude = 0.3f;
            zone.windPulseFrequency = 0.2f;
            go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            var driver = go.AddComponent<TreeWindZoneDriver>();
            driver.windZone = zone;

            Undo.RegisterCreatedObjectUndo(go, "Add Wind Zone");
            Selection.activeGameObject = go;
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
            int exported = ExportMeshesTo(tree, out string folder);
            EditorUtility.DisplayDialog("Procedural Tree",
                $"Exported {exported} mesh(es) to {folder}\n\n" +
                "The tree now uses the exported assets, so you can turn it into a prefab.\n" +
                "Note: changing any setting will regenerate procedural meshes again " +
                "(the exported assets stay on disk).",
                "OK");
        }

        /// <summary>Freezes every LOD's procedural (DontSave) mesh into a real .asset on disk,
        /// so the tree survives being turned into a prefab / painted by Terrain (which both need
        /// persisted assets, not the runtime-regenerated meshes this component normally uses).</summary>
        static int ExportMeshesTo(ProceduralTree tree, out string folder)
        {
            const string root = "Assets/GeneratedTrees";
            if (!AssetDatabase.IsValidFolder(root))
                AssetDatabase.CreateFolder("Assets", "GeneratedTrees");
            folder = root + "/" + tree.name;
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
            return exported;
        }

        /// <summary>
        /// One-click path to "paint this on Terrain": freezes meshes, saves the tree as a
        /// prefab, and registers that prefab as a new tree prototype on a Terrain in the
        /// scene so it immediately shows up in the Terrain's Paint Trees brush list.
        /// </summary>
        static void ExportAndAddToTerrain(ProceduralTree tree)
        {
            var terrain = Terrain.activeTerrain != null ? Terrain.activeTerrain : Object.FindAnyObjectByType<Terrain>();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Procedural Tree",
                    "No Terrain found in the scene. Add a Terrain first (GameObject > 3D Object > Terrain), " +
                    "then try again.", "OK");
                return;
            }

            // must run BEFORE ExportMeshesTo: its AssetDatabase.SaveAssets makes Terrain
            // re-probe every registered prototype, and any stale broken one (from exports
            // older than the script-free-prefab fix) logs "bounds could not be determined"
            // on each refresh until it's removed
            PruneBrokenPrototypes(terrain.terrainData);

            ExportMeshesTo(tree, out string folder);

            // Terrain instantiates a throwaway copy of the prototype prefab to measure its
            // bounds. If that copy still carries the [ExecuteAlways] ProceduralTree component,
            // its OnEnable can run mid-probe and leave the LODGroup momentarily without a valid
            // mesh (the rebuild it might queue is deferred a frame via EditorApplication.delayCall),
            // which is exactly what "couldn't be instanced because bounds could not be determined"
            // looks like. Save a script-free static clone instead - the live scene tree (with the
            // editable ProceduralTree component) is left untouched.
            var clone = Object.Instantiate(tree.gameObject);
            clone.name = tree.name;
            var liveComponent = clone.GetComponent<ProceduralTree>();
            if (liveComponent != null)
                Object.DestroyImmediate(liveComponent);

            string prefabPath = $"{folder}/{tree.name}.prefab";
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);
            Object.DestroyImmediate(clone);
            AssetDatabase.SaveAssets();

            TerrainData data = terrain.terrainData;
            var prototypes = new System.Collections.Generic.List<TreePrototype>(data.treePrototypes);

            int existingIndex = prototypes.FindIndex(p => p.prefab == prefabAsset);
            if (existingIndex < 0)
            {
                prototypes.Add(new TreePrototype { prefab = prefabAsset });
                data.treePrototypes = prototypes.ToArray();
                existingIndex = prototypes.Count - 1;
            }

            EditorUtility.SetDirty(terrain);
            terrain.Flush();

            EditorUtility.DisplayDialog("Procedural Tree",
                $"Added as tree prototype #{existingIndex} on '{terrain.name}'.\n\n" +
                "Open the Terrain component - Paint Trees brush - Edit Trees to start painting it " +
                "onto the ground.\n\n" +
                "Tip: every painted instance of this prototype looks identical (same seed, baked " +
                "into the prefab). For natural variety, export a few different Seeds from this tool " +
                "as separate prefabs/prototypes and let Terrain's brush pick between them at random.",
                "OK");
        }

        /// <summary>
        /// Removes tree prototypes that can never be painted: missing/null prefabs and
        /// prefabs from exports older than the script-free-prefab fix that still carry
        /// the ProceduralTree component. Terrain re-probes every registered prototype's
        /// bounds on each asset refresh, so a single broken entry spams
        /// "couldn't be instanced because bounds could not be determined" forever.
        /// Painted instances of surviving prototypes are remapped so they keep their type.
        /// </summary>
        static void PruneBrokenPrototypes(TerrainData data)
        {
            TreePrototype[] old = data.treePrototypes;
            var keep = new System.Collections.Generic.List<TreePrototype>(old.Length);
            var indexMap = new int[old.Length];
            for (int i = 0; i < old.Length; i++)
            {
                bool broken = old[i].prefab == null
                              || old[i].prefab.GetComponent<ProceduralTree>() != null;
                indexMap[i] = broken ? -1 : keep.Count;
                if (!broken)
                    keep.Add(old[i]);
            }
            if (keep.Count == old.Length)
                return;

            var survivors = new System.Collections.Generic.List<TreeInstance>();
            foreach (TreeInstance inst in data.treeInstances)
            {
                if (inst.prototypeIndex < 0 || inst.prototypeIndex >= indexMap.Length
                    || indexMap[inst.prototypeIndex] < 0)
                    continue;
                TreeInstance moved = inst;
                moved.prototypeIndex = indexMap[inst.prototypeIndex];
                survivors.Add(moved);
            }
            data.treePrototypes = keep.ToArray();
            data.SetTreeInstances(survivors.ToArray(), false);
            Debug.Log($"[Procedural Tree] Removed {old.Length - keep.Count} broken tree prototype(s) from the Terrain " +
                      "(missing prefab or legacy export that still carried the ProceduralTree script).");
        }

        [MenuItem("GameObject/3D Object/Procedural Tree (Tool)", false, 10)]
        static void CreateTree(MenuCommand cmd)
        {
            var go = new GameObject("Procedural Tree");
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            var tree = go.AddComponent<ProceduralTree>();
            Undo.RegisterCreatedObjectUndo(go, "Create Procedural Tree");
            Selection.activeGameObject = go;
            tree.Rebuild(false);
        }
    }
}
