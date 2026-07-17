using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// All authoring parameters of a procedural tree.
    /// Every random decision is driven by the three seeds, so the same
    /// settings always produce exactly the same tree.
    /// </summary>
    [Serializable]
    public class ProceduralTreeSettings
    {
        [Header("Random Seeds")]
        [Tooltip("Master seed. Controls the trunk shape and everything derived from it.")]
        public int seed = 12345;

        [Tooltip("Extra seed for branches only. Change it to reshuffle branches without touching the trunk.")]
        public int branchSeed = 0;

        [Tooltip("Extra seed for leaves only. Change it to reshuffle leaves without touching branches.")]
        public int leafSeed = 0;

        public GeometrySourceSettings geometry = new GeometrySourceSettings();

        public TrunkSettings trunk = new TrunkSettings();

        [Tooltip("Branch groups. Element 0 grows on the trunk, element 1 grows on element 0 branches, and so on.")]
        public List<BranchLevelSettings> branchLevels = new List<BranchLevelSettings>
        {
            BranchLevelSettings.DefaultMainBranches(),
            BranchLevelSettings.DefaultTwigs()
        };

        public LeafSettings leaves = new LeafSettings();
        public MeshSettings mesh = new MeshSettings();
        public WindSettings wind = new WindSettings();
        public LODSettings lods = new LODSettings();

        /// <summary>Keeps values sane when the user types numbers by hand.</summary>
        public void Validate()
        {
            trunk.Validate();
            if (branchLevels != null)
                foreach (var level in branchLevels)
                    level.Validate();
            leaves.Validate();
            lods.Validate();
        }
    }

    public enum GeometrySource
    {
        Procedural,
        Prefabs
    }

    /// <summary>
    /// Where each tree part gets its geometry from.
    /// Procedural = generated in Unity (tubes / cards).
    /// Prefabs = your own modeled meshes; one prefab is picked at random
    /// (seed-driven) per trunk / branch / leaf, and trunk/branch meshes are
    /// bent along the generated branch splines.
    /// </summary>
    [Serializable]
    public class GeometrySourceSettings
    {
        [Tooltip("Trunk geometry. Prefabs must be modeled growing along +Y with the pivot at the base.")]
        public GeometrySource trunkSource = GeometrySource.Procedural;

        [Tooltip("Trunk mesh prefabs (FBX). One is picked at random per tree. " +
                 "The mesh is bent along the trunk spline and scaled to the trunk radius. " +
                 "Rendered with the tree's Bark Material.")]
        public List<GameObject> trunkPrefabs = new List<GameObject>();

        [Tooltip("Branch geometry. Prefabs must be modeled growing along +Y with the pivot at the base.")]
        public GeometrySource branchSource = GeometrySource.Procedural;

        [Tooltip("Branch mesh prefabs (FBX). One is picked at random per branch. " +
                 "The mesh is bent along each branch spline and scaled to the branch radius. " +
                 "Rendered with the tree's Bark Material.")]
        public List<GameObject> branchPrefabs = new List<GameObject>();

        [Tooltip("Leaf geometry. Prefabs should have the pivot at the attach point and grow along +Z " +
                 "(+Y = card normal), authored at roughly 1 unit size.")]
        public GeometrySource leafSource = GeometrySource.Procedural;

        [Tooltip("Leaf / leaf-cluster prefabs (FBX). One is picked at random per leaf and scaled by Leaf Size. " +
                 "Rendered with the tree's Leaf Material.")]
        public List<GameObject> leafPrefabs = new List<GameObject>();
    }

    [Serializable]
    public class TrunkSettings
    {
        [MinMaxRange(0.5f, 40f)]
        [Tooltip("Trunk height in meters (random between min and max).")]
        public FloatRange height = new FloatRange(4.5f, 6f);

        [MinMaxRange(0.02f, 3f)]
        [Tooltip("Trunk radius at the base in meters.")]
        public FloatRange radius = new FloatRange(0.22f, 0.32f);

        [Range(0f, 1f)]
        [Tooltip("How much the trunk thins toward the top. 0 = cylinder, 1 = needle.")]
        public float taper = 0.7f;

        [Range(1f, 3f)]
        [Tooltip("Extra thickness at the very bottom (root flare).")]
        public float rootFlare = 1.5f;

        [Range(0.01f, 0.5f)]
        [Tooltip("How far up the trunk the root flare reaches (fraction of height).")]
        public float rootFlareHeight = 0.12f;

        [Range(2, 32)]
        [Tooltip("Segments along the trunk. More segments = smoother bends.")]
        public int segments = 10;

        [Range(0f, 1f)]
        [Tooltip("Random bending along the trunk.")]
        public float crookedness = 0.18f;

        [MinMaxRange(0f, 30f)]
        [Tooltip("Random lean of the whole trunk in degrees.")]
        public FloatRange lean = new FloatRange(0f, 4f);

        public void Validate()
        {
            height.Sort();
            radius.Sort();
            lean.Sort();
            segments = Mathf.Max(segments, 2);
        }
    }

    [Serializable]
    public class BranchLevelSettings
    {
        [Tooltip("Display name only.")]
        public string name = "Branches";

        public bool enabled = true;

        [MinMaxRange(0, 40)]
        [Tooltip("How many branches grow on each parent.")]
        public IntRange count = new IntRange(7, 11);

        [MinMaxRange(0f, 1f)]
        [Tooltip("Where along the parent these branches may spawn (0 = base, 1 = tip).")]
        public FloatRange spawnRange = new FloatRange(0.3f, 0.95f);

        [MinMaxRange(0f, 179f)]
        [Tooltip("Angle away from the parent direction, in degrees.")]
        public FloatRange angle = new FloatRange(35f, 65f);

        [Range(0f, 1f)]
        [Tooltip("Random spin around the parent. 0 = perfectly even golden-angle spiral.")]
        public float azimuthRandomness = 0.35f;

        [MinMaxRange(0.05f, 1.5f)]
        [Tooltip("Branch length relative to its parent's length.")]
        public FloatRange lengthRatio = new FloatRange(0.28f, 0.42f);

        [Range(0f, 1f)]
        [Tooltip("Branches near the parent's tip get shorter by up to this amount.")]
        public float lengthFalloff = 0.35f;

        [Range(0.05f, 1f)]
        [Tooltip("Branch thickness relative to the parent thickness at the spawn point.")]
        public float radiusRatio = 0.55f;

        [Range(0f, 1f)]
        [Tooltip("How much the branch thins toward its tip.")]
        public float taper = 0.85f;

        [Range(-1f, 1f)]
        [Tooltip("Positive = droop down, negative = curve up toward the sky.")]
        public float gravity = -0.2f;

        [Range(0f, 1f)]
        [Tooltip("Random bending along the branch.")]
        public float crookedness = 0.3f;

        [Range(2, 16)]
        [Tooltip("Segments along each branch.")]
        public int segments = 6;

        public void Validate()
        {
            count.Sort();
            spawnRange.Sort();
            angle.Sort();
            lengthRatio.Sort();
            count.min = Mathf.Max(count.min, 0);
            segments = Mathf.Max(segments, 2);
        }

        public static BranchLevelSettings DefaultMainBranches() => new BranchLevelSettings
        {
            name = "Main Branches",
            count = new IntRange(7, 11),
            spawnRange = new FloatRange(0.3f, 0.95f),
            angle = new FloatRange(35f, 65f),
            lengthRatio = new FloatRange(0.28f, 0.42f),
            radiusRatio = 0.55f,
            gravity = -0.2f,
            crookedness = 0.3f,
            segments = 6
        };

        public static BranchLevelSettings DefaultTwigs() => new BranchLevelSettings
        {
            name = "Twigs",
            count = new IntRange(3, 6),
            spawnRange = new FloatRange(0.25f, 1f),
            angle = new FloatRange(30f, 65f),
            lengthRatio = new FloatRange(0.35f, 0.55f),
            radiusRatio = 0.5f,
            gravity = 0.05f,
            crookedness = 0.35f,
            segments = 4
        };
    }

    public enum LeafShape
    {
        Quad,
        Cross,
        TripleCross
    }

    [Serializable]
    public class LeafSettings
    {
        public bool enabled = true;

        [Tooltip("Quad = 1 card, Cross = 2 crossed cards, TripleCross = 3 cards. Use with a double-sided leaf material.")]
        public LeafShape shape = LeafShape.Cross;

        [MinMaxRange(0, 80)]
        [Tooltip("How many leaves grow on each eligible branch.")]
        public IntRange countPerBranch = new IntRange(10, 16);

        [MinMaxRange(0.01f, 3f)]
        [Tooltip("Leaf card size in meters.")]
        public FloatRange size = new FloatRange(0.35f, 0.55f);

        [MinMaxRange(0f, 1f)]
        [Tooltip("Where along the branch leaves may spawn (0 = base, 1 = tip).")]
        public FloatRange spawnRange = new FloatRange(0.35f, 1f);

        [Range(0f, 1f)]
        [Tooltip("0 = leaves follow the branch direction, 1 = fully random orientation.")]
        public float orientationRandomness = 0.6f;

        [Range(0f, 0.5f)]
        [Tooltip("Pushes leaves outward from the branch surface, in meters.")]
        public float surfaceOffset = 0.02f;

        [Min(0)]
        [Tooltip("Leaves grow on branch levels >= this value (trunk = 0, first branch group = 1, ...).")]
        public int minBranchLevel = 2;

        public void Validate()
        {
            countPerBranch.Sort();
            size.Sort();
            spawnRange.Sort();
            countPerBranch.min = Mathf.Max(countPerBranch.min, 0);
            minBranchLevel = Mathf.Max(minBranchLevel, 0);
        }
    }

    [Serializable]
    public class MeshSettings
    {
        [Range(3, 24)]
        [Tooltip("Sides around the trunk. Deeper branch levels automatically use fewer sides.")]
        public int radialSegments = 8;

        [Range(0.3f, 1f)]
        [Tooltip("Each deeper branch level multiplies its side count by this value.")]
        public float radialDecayPerLevel = 0.7f;

        [Min(0.05f)]
        [Tooltip("Vertical bark UV tiling per world meter.")]
        public float barkUVTiling = 1.5f;

        [Tooltip("Needed for normal maps on bark/leaf materials.")]
        public bool generateTangents = true;
    }

    [Serializable]
    public class WindSettings
    {
        [Tooltip("Bakes per-vertex wind weights into vertex colors for a future wind shader.\n" +
                 "R = main bend (0 at roots, 1 at the top)\n" +
                 "G = branch sway (0 at branch base, 1 at branch tip)\n" +
                 "B = leaf flutter mask (1 on leaves, 0 on bark)\n" +
                 "A = random phase per branch / leaf")]
        public bool bakeWindData = true;

        [Range(0.5f, 3f)]
        [Tooltip("Curve of the main bend weight over tree height. Higher = only the top sways.")]
        public float bendExponent = 1.3f;
    }

    [Serializable]
    public class LODLevelSettings
    {
        [Range(0.001f, 1f)]
        [Tooltip("This LOD stays visible until the tree covers less than this fraction of the screen. " +
                 "Below the LAST level's value the tree is culled completely.")]
        public float screenHeight = 0.45f;

        [Range(0.1f, 1f)]
        [Tooltip("Multiplier on radial segments for this LOD.")]
        public float radialResolution = 1f;

        [Min(0)]
        [Tooltip("Highest branch level whose geometry is included (trunk = 0). " +
                 "Leaves are kept regardless so the canopy silhouette survives.")]
        public int maxBranchLevel = 10;

        [Range(0f, 1f)]
        [Tooltip("Fraction of leaves kept at this LOD.")]
        public float leafDensity = 1f;
    }

    [Serializable]
    public class LODSettings
    {
        [Tooltip("Off = only one full-detail mesh is generated (no LODGroup).")]
        public bool generateLODGroup = true;

        [Tooltip("Smooth cross-fade between LOD levels.")]
        public bool crossFade = true;

        [Tooltip("Scales the remaining leaves up on sparse LODs so the canopy doesn't look thin.")]
        public bool compensateLeafSize = true;

        public List<LODLevelSettings> levels = new List<LODLevelSettings>
        {
            new LODLevelSettings { screenHeight = 0.45f, radialResolution = 1f,    maxBranchLevel = 10, leafDensity = 1f },
            new LODLevelSettings { screenHeight = 0.18f, radialResolution = 0.6f,  maxBranchLevel = 2,  leafDensity = 0.5f },
            new LODLevelSettings { screenHeight = 0.05f, radialResolution = 0.35f, maxBranchLevel = 1,  leafDensity = 0.15f }
        };

        public void Validate()
        {
            if (levels == null) levels = new List<LODLevelSettings>();
            if (levels.Count == 0) levels.Add(new LODLevelSettings());
        }
    }
}
