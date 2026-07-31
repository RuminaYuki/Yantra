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
        public ProceduralTreeSettings settings = new ProceduralTreeSettings();

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

        [System.NonSerialized] public readonly List<LODStats> Stats = new List<LODStats>();
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

        void OnValidate()
        {
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
        public void Rebuild()
        {
            if (!gameObject.scene.IsValid())
                return; // prefab asset on disk - only rebuild scene/prefab-stage instances

            settings.Validate();
            var skeleton = TreeSkeletonGenerator.Generate(settings);
            BranchCount = skeleton.Branches.Count;

            Material bark = barkMaterial != null ? barkMaterial : GetDefaultBark();
            Material leaf = leafMaterial != null ? leafMaterial : GetDefaultLeaf();

            List<LODLevelSettings> levels =
                settings.lods.generateLODGroup && settings.lods.levels.Count > 0
                    ? settings.lods.levels
                    : new List<LODLevelSettings> { new LODLevelSettings { screenHeight = 0.01f } };

            Stats.Clear();
            var renderers = new Renderer[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                Transform child = GetOrCreateLODChild(i);
                var mf = child.GetComponent<MeshFilter>();
                var mr = child.GetComponent<MeshRenderer>();

                Mesh old = mf.sharedMesh;
                Mesh mesh = TreeMeshBuilder.Build(skeleton, settings, levels[i],
                                                  $"{name}_LOD{i}", out int leafCount);
                mesh.hideFlags = HideFlags.DontSave;
                mf.sharedMesh = mesh;
                if (old != null && !IsAsset(old))
                    DestroySafe(old);

                mr.sharedMaterials = new[] { bark, leaf };
                renderers[i] = mr;

                Stats.Add(new LODStats
                {
                    label = "LOD" + i,
                    vertices = mesh.vertexCount,
                    triangles = (int)((mesh.GetIndexCount(0) + mesh.GetIndexCount(1)) / 3),
                    leaves = leafCount
                });
            }

            RemoveExtraLODChildren(levels.Count);
            SetupLODGroup(levels, renderers);
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
