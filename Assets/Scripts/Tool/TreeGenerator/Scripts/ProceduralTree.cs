using System.Collections.Generic;
using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// Procedural tree generator for HDRP.
    /// Lives on an empty GameObject; generates one child per LOD level
    /// (LOD0, LOD1, ...) plus a LODGroup, and rebuilds instantly whenever
    /// a value changes in the inspector (edit mode included).
    ///
    /// Meshes are marked DontSave and are regenerated on scene load, so the
    /// scene file stays small. Use "Export Meshes To Assets" in the inspector
    /// to freeze the current tree into real mesh assets (e.g. for prefabs).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Tools/Procedural Tree")]
    public class ProceduralTree : MonoBehaviour
    {
        public ProceduralTreeSettings settings = new();

        [Tooltip("HDRP bark material. Leave empty to use an auto placeholder (HDRP/Lit).")]
        public Material barkMaterial;

        [Tooltip("HDRP leaf material - should be double sided (and usually alpha clipped). " +
                 "Leave empty to use an auto placeholder.")]
        public Material leafMaterial;

        public class LODStats
        {
            public string label;
            public int vertices;
            public int triangles;
            public int leaves;
        }

        [System.NonSerialized] public readonly List<LODStats> Stats = new();
        [System.NonSerialized] public int BranchCount;

        bool _rebuildQueued;

        static Material s_defaultBark;
        static Material s_defaultLeaf;

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (NeedsRebuild())
                    QueueRebuild();
                return;
            }
#endif
            if (NeedsRebuild())
                Rebuild();
        }

        /// <summary>
        /// Set by the custom inspector while it drives rebuilds itself, so
        /// OnValidate doesn't queue a duplicate rebuild for every slider tick.
        /// </summary>
        public static bool EditorIsDriving;

        void OnValidate()
        {
            if (EditorIsDriving)
                return;
            QueueRebuild();
        }

        /// <summary>Rebuilds on the next editor tick (safe to call from OnValidate).</summary>
        public void QueueRebuild()
        {
#if UNITY_EDITOR
            if (_rebuildQueued)
                return;
            _rebuildQueued = true;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                _rebuildQueued = false;
                if (this != null && isActiveAndEnabled)
                    Rebuild();
            };
#endif
        }

        [ContextMenu("Rebuild")]
        public void Rebuild() => Rebuild(false);

        /// <summary>
        /// interactive = true is used by the inspector while a slider is being
        /// dragged: only LOD0 is rebuilt, tangents are skipped and the LODGroup
        /// is left alone, keeping the editor fluid. A full rebuild follows once
        /// the drag settles.
        /// </summary>
        public void Rebuild(bool interactive)
        {
            if (!gameObject.scene.IsValid())
                return; // prefab asset on disk - only rebuild scene/prefab-stage instances

            settings.Validate();
            AdoptLODGroupOverrides();
            if (!interactive)
                TreeMeshBuilder.ClearPrefabCache();

            var skeleton = TreeSkeletonGenerator.Generate(settings);
            BranchCount = skeleton.Branches.Count;

            Material bark = barkMaterial != null ? barkMaterial : GetDefaultBark();
            Material leaf = leafMaterial != null ? leafMaterial : GetDefaultLeaf();

            List<LODLevelSettings> levels =
                settings.lods.generateLODGroup && settings.lods.levels.Count > 0
                    ? settings.lods.levels
                    : new List<LODLevelSettings> { new() { screenHeight = 0.01f } };

            int buildCount = interactive ? 1 : levels.Count;
            if (!interactive)
                Stats.Clear();

            var renderers = new Renderer[levels.Count];
            for (int i = 0; i < buildCount; i++)
            {
                Transform child = GetOrCreateLODChild(i);
                var mf = child.GetComponent<MeshFilter>();
                var mr = child.GetComponent<MeshRenderer>();

                // reuse the existing procedural mesh to avoid churning assets
                Mesh mesh = mf.sharedMesh;
                if (mesh == null || IsAsset(mesh))
                {
                    mesh = new Mesh { hideFlags = HideFlags.DontSave };
                    mf.sharedMesh = mesh;
                }
                TreeMeshBuilder.Build(skeleton, settings, levels[i], $"{name}_LOD{i}",
                                      mesh, skipTangents: interactive, out int leafCount);

                mr.sharedMaterials = new[] { bark, leaf };
                renderers[i] = mr;

                if (!interactive)
                {
                    Stats.Add(new()
                    {
                        label = "LOD" + i,
                        vertices = mesh.vertexCount,
                        triangles = (int)((mesh.GetIndexCount(0) + mesh.GetIndexCount(1)) / 3),
                        leaves = leafCount
                    });
                }
            }

            if (interactive)
                return;

            RemoveExtraLODChildren(levels.Count);
            SetupLODGroup(levels, renderers);
        }

        /// <summary>
        /// If the user tweaked the transition distances directly on the LODGroup
        /// component, pull those values back into the tool settings so the next
        /// rebuild keeps them instead of overwriting.
        /// </summary>
        public void AdoptLODGroupOverrides()
        {
            if (!settings.lods.generateLODGroup)
                return;
            var group = GetComponent<LODGroup>();
            if (group == null)
                return;
            LOD[] lods = group.GetLODs();
            if (lods.Length != settings.lods.levels.Count)
                return;
            for (int i = 0; i < lods.Length; i++)
            {
                float h = lods[i].screenRelativeTransitionHeight;
                if (Mathf.Abs(h - settings.lods.levels[i].screenHeight) > 0.0005f)
                    settings.lods.levels[i].screenHeight = h;
            }
        }

        bool NeedsRebuild()
        {
            var lod0 = transform.Find("LOD0");
            if (lod0 == null)
                return true;
            var mf = lod0.GetComponent<MeshFilter>();
            return mf == null || mf.sharedMesh == null;
        }

        Transform GetOrCreateLODChild(int i)
        {
            string childName = "LOD" + i;
            Transform t = transform.Find(childName);
            if (t == null)
            {
                var go = new GameObject(childName, typeof(MeshFilter), typeof(MeshRenderer));
                t = go.transform;
                t.SetParent(transform, false);
            }
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            if (t.GetComponent<MeshFilter>() == null) t.gameObject.AddComponent<MeshFilter>();
            if (t.GetComponent<MeshRenderer>() == null) t.gameObject.AddComponent<MeshRenderer>();
            return t;
        }

        void RemoveExtraLODChildren(int keepCount)
        {
            for (int i = keepCount; i < keepCount + 16; i++)
            {
                Transform t = transform.Find("LOD" + i);
                if (t == null)
                    continue;
                var mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null && !IsAsset(mf.sharedMesh))
                    DestroySafe(mf.sharedMesh);
                DestroySafe(t.gameObject);
            }
        }

        void SetupLODGroup(List<LODLevelSettings> levels, Renderer[] renderers)
        {
            var group = GetComponent<LODGroup>();
            if (!settings.lods.generateLODGroup || levels.Count <= 1)
            {
                if (group != null)
                    DestroySafe(group);
                return;
            }
            if (group == null)
                group = gameObject.AddComponent<LODGroup>();

            var lods = new LOD[levels.Count];
            float prev = 1f;
            for (int i = 0; i < levels.Count; i++)
            {
                // keep transition heights strictly descending
                float h = Mathf.Min(levels[i].screenHeight, prev - 0.005f);
                h = Mathf.Max(h, 0.001f);
                prev = h;
                lods[i] = new LOD(h, new[] { renderers[i] });
            }
            group.fadeMode = settings.lods.crossFade ? LODFadeMode.CrossFade : LODFadeMode.None;
            group.animateCrossFading = settings.lods.crossFade;
            group.SetLODs(lods);
            group.RecalculateBounds();
        }

        static Material GetDefaultBark()
        {
            if (s_defaultBark == null)
                s_defaultBark = CreateDefaultMaterial("TreeTool_DefaultBark",
                    new Color(0.35f, 0.24f, 0.16f), false);
            return s_defaultBark;
        }

        static Material GetDefaultLeaf()
        {
            if (s_defaultLeaf == null)
                s_defaultLeaf = CreateDefaultMaterial("TreeTool_DefaultLeaf",
                    new Color(0.2f, 0.45f, 0.18f), true);
            return s_defaultLeaf;
        }

        static Material CreateDefaultMaterial(string matName, Color color, bool doubleSided)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            var m = new Material(shader) { name = matName, hideFlags = HideFlags.DontSave };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            m.color = color;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
            if (doubleSided)
            {
                if (m.HasProperty("_DoubleSidedEnable")) m.SetFloat("_DoubleSidedEnable", 1f);
                m.EnableKeyword("_DOUBLESIDED_ON");
                if (m.HasProperty("_CullMode"))
                    m.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);
                if (m.HasProperty("_CullModeForward"))
                    m.SetFloat("_CullModeForward", (float)UnityEngine.Rendering.CullMode.Off);
            }
            return m;
        }

        static bool IsAsset(Object o)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.IsPersistent(o);
#else
            return false;
#endif
        }

        static void DestroySafe(Object o)
        {
            if (Application.isPlaying)
                Destroy(o);
            else
                DestroyImmediate(o);
        }
    }
}
