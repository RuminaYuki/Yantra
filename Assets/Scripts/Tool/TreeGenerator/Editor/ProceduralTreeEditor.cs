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

            DrawWindModeSection();
            DrawWindResponseSection();

            EditorGUILayout.Space(8f);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("manualNumberEntry"),
                new GUIContent("Manual Number Entry", "Turns every slider/range below into a plain typed field."));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Random Seeds", EditorStyles.boldLabel);
            DrawSeedRow(_settings.FindPropertyRelative("seed"), "New Tree");
            DrawSeedRow(_settings.FindPropertyRelative("branchSeed"), "Shuffle");
            DrawSeedRow(_settings.FindPropertyRelative("leafSeed"), "Shuffle");
            DrawSeedRow(_settings.FindPropertyRelative("rootSeed"), "Shuffle");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Geometry Source", EditorStyles.boldLabel);
            DrawGeometrySection();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Structure", EditorStyles.boldLabel);
            // radial segments only matter for procedural tubes; Wind Response lives in the
            // consolidated Wind Response section above instead (always hidden here)
            DrawGroup(_settings.FindPropertyRelative("trunk"),
                      MergeHidden(IsPrefabSource("trunkSource") ? new[] { "radialSegments" } : null,
                                  new[] { "windResponse" }));
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("branchLevels"), true);
            DrawRootsSection();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Foliage", EditorStyles.boldLabel);
            DrawLeavesSection();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            DrawMeshSection();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Level Of Detail", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("lods"), true);

            EditorGUILayout.Space(8f);
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
        // Wind Mode - Fake (self-contained material, default) vs True (real WindZone)
        // ------------------------------------------------------------------

        bool IsTrueWind() => _settings.FindPropertyRelative("windMode").enumValueIndex == (int)WindMode.True;

        void DrawWindModeSection()
        {
            EditorGUILayout.LabelField("Wind Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("windMode"), new GUIContent("Mode"));
            if (IsTrueWind())
                EditorGUILayout.HelpBox(
                    "True Wind: driven by a real Unity WindZone in the scene. The Wind Response panel " +
                    "right below controls every part of the tree. The leaf material switches to the " +
                    "True Wind leaf material automatically.",
                    MessageType.None);
            else
                EditorGUILayout.HelpBox(
                    "Fake Wind: a self-contained material effect, no scene setup needed. No Wind Zone " +
                    "button and no per-part Wind Response panel - they don't apply in this mode. The " +
                    "leaf material uses the project's Leaf.mat automatically.",
                    MessageType.None);
        }

        /// <summary>
        /// Every "how much does this part sway" field lives here, in one place, instead of being
        /// scattered inside Trunk/Branch Levels/Roots/Leaves - including newly ADDED Branch Levels
        /// (this iterates the list live, so a level added later shows up here automatically too,
        /// under whatever name it was given). Only relevant to True Wind, so the whole section
        /// (including the Add Wind Zone hint) only appears in that mode - nothing shows for Fake Wind.
        /// </summary>
        void DrawWindResponseSection()
        {
            if (!IsTrueWind())
                return;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Wind Response", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "How much each part sways in True Wind - trunk, every branch level (including ones " +
                "you add later), roots, and leaves, all together here.", MessageType.None);

            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("trunk").FindPropertyRelative("windResponse"),
                new GUIContent("Trunk"));

            SerializedProperty branchLevels = _settings.FindPropertyRelative("branchLevels");
            for (int i = 0; i < branchLevels.arraySize; i++)
            {
                SerializedProperty level = branchLevels.GetArrayElementAtIndex(i);
                string levelName = level.FindPropertyRelative("name").stringValue;
                EditorGUILayout.PropertyField(level.FindPropertyRelative("windResponse"),
                    new GUIContent(string.IsNullOrEmpty(levelName) ? $"Branch Level {i}" : levelName));
            }

            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("roots").FindPropertyRelative("windResponse"),
                new GUIContent("Roots"));
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("leaves").FindPropertyRelative("windFlutterResponse"),
                new GUIContent("Leaves"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(_settings.FindPropertyRelative("wind"), true);
            DrawWindZoneHint();
        }

        static string[] MergeHidden(string[] a, string[] b)
        {
            if (a == null) return b;
            if (b == null) return a;
            var merged = new string[a.Length + b.Length];
            a.CopyTo(merged, 0);
            b.CopyTo(merged, a.Length);
            return merged;
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
            // "shape" only matters for procedural cards; Wind Flutter Response moved to the
            // consolidated Wind Response section (DrawWindResponseSection) so it always lives
            // in one place rather than being hidden/shown per mode right here
            DrawGroup(_settings.FindPropertyRelative("leaves"),
                      MergeHidden(IsPrefabSource("leafSource") ? new[] { "shape" } : null,
                                  new[] { "windFlutterResponse" }));
        }

        static readonly string[] ButtressOnlyFields =
        {
            "buttressCount", "buttressLength", "buttressStartHeight",
            "buttressFlare", "buttressTaper", "buttressDroop", "buttressCrookedness"
        };
        static readonly string[] PneumatophoreOnlyFields =
        {
            "pneumatophoreCount", "pneumatophoreHeight", "pneumatophoreRadius",
            "pneumatophoreSpread", "pneumatophoreLean"
        };

        /// <summary>Roots: one Enabled toggle + one Type dropdown (Buttress / Pneumatophore) picks
        /// between two unrelated field sets - only the fields for the selected type are shown,
        /// same "hide what doesn't apply" pattern the Geometry/Leaves/Mesh sections already use.
        /// Fine Roots is its own nested foldout so it doesn't clutter the main root fields. Wind
        /// Response lives in the consolidated Wind Response section instead (always hidden here).</summary>
        void DrawRootsSection()
        {
            SerializedProperty roots = _settings.FindPropertyRelative("roots");
            SerializedProperty typeProp = roots.FindPropertyRelative("type");
            bool isPneumatophore = typeProp.enumValueIndex == (int)RootType.Pneumatophore;
            string[] typeHidden = isPneumatophore ? ButtressOnlyFields : PneumatophoreOnlyFields;
            DrawGroup(roots, MergeHidden(typeHidden, new[] { "fineRoots", "windResponse" }));

            if (!roots.isExpanded)
                return;
            EditorGUI.indentLevel++;
            DrawGroup(roots.FindPropertyRelative("fineRoots"), null);
            EditorGUI.indentLevel--;
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

        struct ExportResult
        {
            public string treeFolder;
            public string prefabFolder;
            public bool backedUp;
        }

        static void ExportMeshes(ProceduralTree tree)
        {
            var result = DoCoreExport(tree);
            EditorUtility.DisplayDialog("Procedural Tree",
                $"Exported to {result.treeFolder}\n\n" +
                "Texture/    - copies of this tree's own textures (safe to edit, never shared with other trees)\n" +
                "Material/   - copies of this tree's own Bark/Leaf materials\n" +
                $"TreePrefab/ - meshes + {tree.name}_Source.prefab, an editable prefab already connected " +
                "to this scene object\n" +
                (result.backedUp ? "Backup/     - the previous export, kept as a safety copy\n" : "") +
                "\nThis scene object is now a prefab instance of that Source prefab - keep editing it here, " +
                "or open the prefab asset directly, and Export again any time to update everything together.",
                "OK");
        }

        /// <summary>
        /// Every export (both buttons) goes through here: resolves/renames this tree's own
        /// Assets/GeneratedTrees/&lt;name&gt;/ folder, backs up a previous export if one exists,
        /// duplicates the currently-active Bark/Leaf materials (and the textures they reference)
        /// into that tree's own Material/Texture folders so editing one tree's copy never touches
        /// another tree or the shared source materials, freezes the LOD meshes into TreePrefab/,
        /// and saves+connects an editable "_Source" prefab (keeps the ProceduralTree component,
        /// so - unlike a frozen static export - it can always be opened and regenerated later).
        /// </summary>
        static ExportResult DoCoreExport(ProceduralTree tree)
        {
            string treeFolder = ResolveTreeFolder(tree);
            bool backedUp = BackupIfExists(treeFolder);

            string textureFolder = $"{treeFolder}/Texture";
            string materialFolder = $"{treeFolder}/Material";
            string prefabFolder = $"{treeFolder}/TreePrefab";
            EnsureFolder(treeFolder, "Texture");
            EnsureFolder(treeFolder, "Material");
            EnsureFolder(treeFolder, "TreePrefab");

            // duplicate whatever materials are currently on LOD0 - the last Rebuild already
            // resolved these to either the user's own assignment or the Wind Mode default
            var lod0 = tree.transform.Find("LOD0");
            var lod0Renderer = lod0 != null ? lod0.GetComponent<MeshRenderer>() : null;
            if (lod0Renderer != null && lod0Renderer.sharedMaterials.Length >= 2)
            {
                Material bark = DuplicateMaterial(lod0Renderer.sharedMaterials[0], materialFolder, textureFolder, tree.name, "Bark");
                Material leaf = DuplicateMaterial(lod0Renderer.sharedMaterials[1], materialFolder, textureFolder, tree.name, "Leaf");
                foreach (Transform child in tree.transform)
                {
                    if (!child.name.StartsWith("LOD"))
                        continue;
                    var mr = child.GetComponent<MeshRenderer>();
                    if (mr != null)
                        mr.sharedMaterials = new[] { bark, leaf };
                }
            }

            ExportMeshesTo(tree, prefabFolder);

            string sourcePath = $"{prefabFolder}/{tree.name}_Source.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(tree.gameObject, sourcePath, InteractionMode.AutomatedAction);

            AssetDatabase.SaveAssets();
            return new ExportResult { treeFolder = treeFolder, prefabFolder = prefabFolder, backedUp = backedUp };
        }
        static void EnsureFolderPath(string fullPath)
        {
            if (AssetDatabase.IsValidFolder(fullPath))
                return;

            string[] parts = fullPath.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, parts[i]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError($"[Procedural Tree] Failed to create folder '{next}'. " +
                                        "Check for an invalid character in the name, or a read-only/locked path.");
                        return;
                    }
                }
                current = next;
            }
        }
        /// <summary>Resolves Assets/ToolGenerated/Tree/&lt;tree.name&gt;/, renaming the previous export
        /// folder to match if the tree's GameObject was renamed since the last export.</summary>
        static string ResolveTreeFolder(ProceduralTree tree)
        {
            const string root = "Assets/ToolGenerated/Tree";
            EnsureFolderPath(root);

            string desired = $"{root}/{tree.name}";
            if (!string.IsNullOrEmpty(tree.exportedFolderName) && tree.exportedFolderName != tree.name)
            {
                string previous = $"{root}/{tree.exportedFolderName}";
                if (AssetDatabase.IsValidFolder(previous) && !AssetDatabase.IsValidFolder(desired))
                {
                    string err = AssetDatabase.MoveAsset(previous, desired);
                    if (!string.IsNullOrEmpty(err))
                    {
                        Debug.LogWarning($"[Procedural Tree] Could not rename export folder to match '{tree.name}': {err}");
                        desired = previous;
                    }
                }
            }

            EnsureFolderPath(desired);
            tree.exportedFolderName = System.IO.Path.GetFileName(desired);
            return desired;
        }

        /// <summary>Moves an existing TreePrefab/Material/Texture (a previous export) into a
        /// timestamped Backup/ subfolder before a fresh export overwrites them.</summary>
        static bool BackupIfExists(string treeFolder)
        {
            string[] subs = { "TreePrefab", "Material", "Texture" };
            bool any = false;
            foreach (var s in subs)
                if (AssetDatabase.IsValidFolder($"{treeFolder}/{s}"))
                    any = true;
            if (!any)
                return false;

            const string backupName = "Backup";
            string backupRoot = $"{treeFolder}/{backupName}";
            if (!AssetDatabase.IsValidFolder(backupRoot))
                AssetDatabase.CreateFolder(treeFolder, backupName);
            string stamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            AssetDatabase.CreateFolder(backupRoot, stamp);
            string backupStamp = $"{backupRoot}/{stamp}";

            foreach (var s in subs)
            {
                string src = $"{treeFolder}/{s}";
                if (!AssetDatabase.IsValidFolder(src))
                    continue;
                string err = AssetDatabase.MoveAsset(src, $"{backupStamp}/{s}");
                if (!string.IsNullOrEmpty(err))
                    Debug.LogWarning($"[Procedural Tree] Backup move failed for {src}: {err}");
            }
            return true;
        }

        static void EnsureFolder(string parent, string name)
        {
            EnsureFolderPath($"{parent}/{name}");
        }

        /// <summary>Clones a material asset (so editing this tree's copy never affects the shared
        /// source material or any other tree) and duplicates the textures it references too.</summary>
        static Material DuplicateMaterial(Material src, string materialFolder, string textureFolder, string treeName, string suffix)
        {
            if (src == null)
                return null;
            var dup = Object.Instantiate(src);
            dup.name = $"{treeName}_{suffix}";
            DuplicateTextures(dup, textureFolder, treeName, suffix);

            string path = $"{materialFolder}/{treeName}_{suffix}.mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(dup, path);
            return dup;
        }

        /// <summary>Copies every texture referenced by the material into textureFolder, renamed
        /// &lt;treeName&gt;_&lt;suffix&gt;_&lt;mapName&gt;, and repoints the material at the copies.</summary>
        static void DuplicateTextures(Material mat, string textureFolder, string treeName, string suffix)
        {
            Shader shader = mat.shader;
            int count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture)
                    continue;
                string propName = shader.GetPropertyName(i);
                Texture tex = mat.GetTexture(propName);
                if (tex == null)
                    continue;
                string srcPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(srcPath))
                    continue;

                string ext = System.IO.Path.GetExtension(srcPath);
                string mapName = propName.TrimStart('_');
                string dstPath = $"{textureFolder}/{treeName}_{suffix}_{mapName}{ext}";
                AssetDatabase.DeleteAsset(dstPath);
                if (AssetDatabase.CopyAsset(srcPath, dstPath))
                    mat.SetTexture(propName, AssetDatabase.LoadAssetAtPath<Texture>(dstPath));
            }
        }

        /// <summary>Freezes every LOD's procedural (DontSave) mesh into a real .asset on disk,
        /// so the tree survives being turned into a prefab / painted by Terrain (which both need
        /// persisted assets, not the runtime-regenerated meshes this component normally uses).</summary>
        static int ExportMeshesTo(ProceduralTree tree, string prefabFolder)
        {
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
                string path = $"{prefabFolder}/{copy.name}.asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(copy, path);
                mf.sharedMesh = copy;
                exported++;
            }
            AssetDatabase.SaveAssets();
            return exported;
        }

        /// <summary>
        /// One-click path to "paint this on Terrain": resolves which Terrain to target (asks the
        /// user to disambiguate if multiple exist and none is selected), warns before overwriting
        /// an already-painted prototype for this same tree, then runs the shared export (folder,
        /// material/texture duplication, editable Source prefab), additionally saves a SEPARATE
        /// script-free static prefab, and registers/updates it as a tree prototype on the Terrain
        /// so it shows up in the Terrain's Paint Trees brush list.
        /// </summary>
        static void ExportAndAddToTerrain(ProceduralTree tree)
        {
            Terrain terrain = ResolveTargetTerrain();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Procedural Tree",
                    "No Terrain found in the scene, or multiple Terrains exist and none is selected.\n\n" +
                    "Select the Terrain GameObject you want to paint on in the Hierarchy, then try again.",
                    "OK");
                return;
            }
            if (terrain.terrainData == null)
            {
                EditorUtility.DisplayDialog("Procedural Tree",
                    $"'{terrain.name}' has a Terrain component but no Terrain Data asset assigned.\n\n" +
                    "Select it in the Hierarchy and assign or create a Terrain Data asset in the " +
                    "'Terrain Data' field at the top of the Terrain component, then try again.",
                    "OK");
                return;
            }

            TerrainData data = terrain.terrainData;

            // must run BEFORE any AssetDatabase.SaveAssets below, which makes Terrain re-probe
            // every registered prototype - any stale broken one (from an old script-carrying export,
            // or a deleted/moved prefab) logs "bounds could not be determined" on each refresh
            // until it's removed
            PruneBrokenPrototypes(data);

            // Predict the path the static prefab will be saved to BEFORE doing any destructive
            // work, so we can warn about overwriting an existing prototype up front instead of
            // burning a backup/export cycle first.
            string predictedStaticPath = PredictStaticPrefabPath(tree);
            int existingPrototypeIndex = FindPrototypeIndexByPath(data, predictedStaticPath);
            if (existingPrototypeIndex >= 0)
            {
                bool replace = EditorUtility.DisplayDialog("Procedural Tree",
                    $"'{tree.name}' is already registered as tree prototype #{existingPrototypeIndex} " +
                    $"on '{terrain.name}'.\n\n" +
                    "Replace it with the current settings? Every already-painted instance of this " +
                    "prototype on the terrain will update to match - nothing gets re-painted or removed.",
                    "Replace Existing", "Cancel Export");
                if (!replace)
                    return;
            }

            var result = DoCoreExport(tree);

            // Terrain instantiates a throwaway copy of the prototype prefab to measure its bounds.
            // If that copy still carries the [ExecuteAlways] ProceduralTree component, its OnEnable
            // can run mid-probe and leave the LODGroup momentarily without a valid mesh, which is
            // exactly what "couldn't be instanced because bounds could not be determined" looks like.
            // Save a SEPARATE script-free static clone for Terrain - the editable Source prefab
            // from DoCoreExport (already connected to this scene object) is untouched.
            var clone = Object.Instantiate(tree.gameObject);
            clone.name = tree.name;
            var liveComponent = clone.GetComponent<ProceduralTree>();
            if (liveComponent != null)
                Object.DestroyImmediate(liveComponent);

            string staticPath = $"{result.prefabFolder}/{tree.name}.prefab";
            AssetDatabase.DeleteAsset(staticPath);
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(clone, staticPath);
            Object.DestroyImmediate(clone);
            AssetDatabase.SaveAssets();

            // Match by path, not Object reference: the export above just deleted+recreated the
            // prefab asset at this exact path, so any old TreePrototype pointing at the previous
            // instance is a broken reference now even though the path is identical - so we
            // re-resolve the index by path instead of relying on the existingPrototypeIndex above.
            var prototypes = new System.Collections.Generic.List<TreePrototype>(data.treePrototypes);
            int prototypeIndex = FindPrototypeIndexByPath(data, staticPath);
            if (prototypeIndex < 0)
            {
                prototypes.Add(new TreePrototype { prefab = prefabAsset });
                data.treePrototypes = prototypes.ToArray();
                prototypeIndex = prototypes.Count - 1;
            }
            else
            {
                prototypes[prototypeIndex] = new TreePrototype { prefab = prefabAsset };
                data.treePrototypes = prototypes.ToArray();
            }

            EditorUtility.SetDirty(terrain);
            terrain.Flush();

            EditorUtility.DisplayDialog("Procedural Tree",
                $"{(existingPrototypeIndex >= 0 ? "Updated" : "Added as")} tree prototype #{prototypeIndex} on '{terrain.name}'.\n\n" +
                $"Editable original: this scene object (already connected to {tree.name}_Source.prefab " +
                $"in {result.prefabFolder}). Edit it and click this button again to update the painted trees.\n\n" +
                "Open the Terrain component - Paint Trees brush - Edit Trees to start painting it " +
                "onto the ground.\n\n" +
                "Tip: every painted instance of this prototype looks identical (same seed, baked " +
                "into the prefab). For natural variety, export a few different Seeds from this tool " +
                "as separate prefabs/prototypes and let Terrain's brush pick between them at random.",
                "OK");
        }

        /// <summary>
        /// Picks which Terrain to target when the scene may contain more than one: prefers
        /// whatever Terrain the user currently has selected (or is a child of the selection) in
        /// the Hierarchy, falls back to the single Terrain in the scene if there's only one, and
        /// otherwise refuses to guess.
        /// </summary>
        static Terrain ResolveTargetTerrain()
        {
            if (Selection.activeGameObject != null)
            {
                var selected = Selection.activeGameObject.GetComponentInParent<Terrain>();
                if (selected != null)
                    return selected;
            }

            var all = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            if (all.Length == 1)
                return all[0];

            if (all.Length > 1)
            {
                Debug.LogWarning("[Procedural Tree] Multiple Terrains found in the scene and none is " +
                                  "selected in the Hierarchy - refusing to guess which one to export to. " +
                                  "Select the target Terrain first.");
                return null;
            }

            return Terrain.activeTerrain;
        }

        /// <summary>Path the script-free static prefab for this tree will be saved to, computed
        /// without touching the AssetDatabase - used to check for an existing prototype before
        /// running the (destructive) export.</summary>
        static string PredictStaticPrefabPath(ProceduralTree tree)
        {
            string folderName = string.IsNullOrEmpty(tree.exportedFolderName) ? tree.name : tree.exportedFolderName;
            return $"Assets/GeneratedTrees/{folderName}/TreePrefab/{tree.name}.prefab";
        }

        /// <summary>Finds a tree prototype on this TerrainData whose prefab asset lives at the given
        /// path. Comparing by path (not by Object reference) matters because Export always deletes
        /// and recreates the prefab asset at the same path, which changes its identity.</summary>
        static int FindPrototypeIndexByPath(TerrainData data, string path)
        {
            TreePrototype[] prototypes = data.treePrototypes;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prefab == null)
                    continue;
                if (AssetDatabase.GetAssetPath(prototypes[i].prefab) == path)
                    return i;
            }
            return -1;
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
