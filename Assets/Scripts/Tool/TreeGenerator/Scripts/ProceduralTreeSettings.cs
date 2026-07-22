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

        [Tooltip("Extra seed for roots only. Change it to reshuffle roots without touching anything else.")]
        public int rootSeed = 0;

        [Tooltip("Fake Wind = a self-contained material effect (no setup needed, default). " +
                 "True Wind = driven by a real Unity WindZone in the scene, with per-part response " +
                 "you set below (Trunk/Branch/Leaf/Root Wind Response, and the Wind section).")]
        public WindMode windMode = WindMode.Fake;

        [Tooltip("Turns every slider and min-max range in this inspector into a plain typed number " +
                 "field (any value, not limited to the slider's range) - one checkbox for all of them.")]
        public bool manualNumberEntry = false;

        public GeometrySourceSettings geometry = new();

        public TrunkSettings trunk = new();

        [Tooltip("Branch groups. Element 0 grows on the trunk, element 1 grows on element 0 branches, and so on.")]
        public List<BranchLevelSettings> branchLevels = new()
        {
            BranchLevelSettings.DefaultMainBranches(),
            BranchLevelSettings.DefaultTwigs()
        };

        public RootSettings roots = new();
        public LeafSettings leaves = new();
        public MeshSettings mesh = new();
        public WindSettings wind = new();
        public LODSettings lods = new();

        /// <summary>Keeps values sane when the user types numbers by hand.</summary>
        public void Validate()
        {
            trunk.Validate();
            if (branchLevels != null)
                foreach (var level in branchLevels)
                    level.Validate();
            roots.Validate();
            leaves.Validate();
            lods.Validate();
        }
    }

    public enum WindMode
    {
        [InspectorName("Fake Wind (Default)")]
        Fake,
        [InspectorName("True Wind (Real WindZone)")]
        True
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
        public List<GameObject> trunkPrefabs = new();

        [Tooltip("Branch geometry. Prefabs must be modeled growing along +Y with the pivot at the base.")]
        public GeometrySource branchSource = GeometrySource.Procedural;

        [Tooltip("Branch mesh prefabs (FBX). One is picked at random per branch. " +
                 "The mesh is bent along each branch spline and scaled to the branch radius. " +
                 "Rendered with the tree's Bark Material.")]
        public List<GameObject> branchPrefabs = new();

        [Tooltip("Leaf geometry. Prefabs should have the pivot at the attach point and grow along +Z " +
                 "(+Y = card normal), authored at roughly 1 unit size.")]
        public GeometrySource leafSource = GeometrySource.Procedural;

        [Tooltip("Leaf / leaf-cluster prefabs (FBX). One is picked at random per leaf and scaled by Leaf Size. " +
                 "Rendered with the tree's Leaf Material.")]
        public List<GameObject> leafPrefabs = new();
    }

    [Serializable]
    public class TrunkSettings
    {
        [MinMaxRange(0.1f, 300f)]
        [Tooltip("Trunk height in meters (random between min and max).")]
        public FloatRange height = new(4.5f, 6f);

        [MinMaxRange(0.01f, 30f)]
        [Tooltip("Trunk radius at the base in meters.")]
        public FloatRange radius = new(0.22f, 0.32f);

        [ToolRange(0f, 1f)]
        [Tooltip("How much the trunk thins toward the top. 0 = cylinder, 1 = needle.")]
        public float taper = 0.7f;

        [ToolRange(1f, 6f)]
        [Tooltip("Extra thickness at the very bottom (root flare).")]
        public float rootFlare = 1.5f;

        [ToolRange(0.01f, 1f)]
        [Tooltip("How far up the trunk the root flare reaches (fraction of height).")]
        public float rootFlareHeight = 0.12f;

        [ToolRange(2, 64)]
        [Tooltip("Segments along the trunk. More segments = smoother bends.")]
        public int segments = 10;

        [ToolRange(3, 128)]
        [InspectorName("Sides")]
        [Tooltip("Sides around the trunk (this part only - each branch level has its own).")]
        public int radialSegments = 8;

        [ToolRange(0f, 2f)]
        [Tooltip("Random bending along the trunk. Above 1 = extra crooked/wavy.")]
        public float crookedness = 0.18f;

        [MinMaxRange(0f, 90f)]
        [Tooltip("Random lean of the whole trunk in degrees.")]
        public FloatRange lean = new(0f, 4f);

        [FineRange(0f, 1f)]
        [Tooltip("[True Wind only] How much the trunk participates in wind sway (0 = rigid, 1 = normal). " +
                 "Keep this very low - the trunk is thick wood and should barely move at all, even when " +
                 "branches and leaves are swaying a lot. Baked into the wind vertex data - only affects " +
                 "trees using True Wind mode.")]
        public float windResponse = 0.05f;

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

        [MinMaxRange(0, 300)]
        [Tooltip("How many branches grow on each parent.")]
        public IntRange count = new(7, 11);

        [MinMaxRange(0f, 1f)]
        [Tooltip("Where along the parent these branches may spawn (0 = base, 1 = tip).")]
        public FloatRange spawnRange = new(0.3f, 0.95f);

        [MinMaxRange(0f, 179f)]
        [Tooltip("Angle away from the parent direction, in degrees.")]
        public FloatRange angle = new(35f, 65f);

        [ToolRange(0f, 1f)]
        [InspectorName("Spin Randomness")]
        [Tooltip("Random spin around the parent. 0 = perfectly even golden-angle spiral.")]
        public float azimuthRandomness = 0.35f;

        [MinMaxRange(0.01f, 8f)]
        [Tooltip("Branch length relative to its parent's length.")]
        public FloatRange lengthRatio = new(0.28f, 0.42f);

        [ToolRange(0f, 1f)]
        [Tooltip("Branches near the parent's tip get shorter by up to this amount.")]
        public float lengthFalloff = 0.35f;

        [ToolRange(0.01f, 5f)]
        [Tooltip("Branch thickness relative to the parent thickness at the spawn point. Above 1 = thicker than the parent.")]
        public float radiusRatio = 0.55f;

        [ToolRange(0.05f, 15f)]
        [Tooltip("Extra thickness multiplier for THIS level only. Deeper levels inherit it " +
                 "naturally because their Radius Ratio measures against this level's real thickness.")]
        public float thicknessScale = 1f;

        [ToolRange(3, 128)]
        [InspectorName("Sides")]
        [Tooltip("Sides around branches of this level (independent of trunk and other levels).")]
        public int radialSegments = 6;

        [ToolRange(0f, 1f)]
        [Tooltip("Fraction of the branch that curves smoothly out of the parent's direction " +
                 "instead of jutting out at the full angle. Hides the joint.")]
        public float jointSmoothing = 0.35f;

        [ToolRange(1f, 6f)]
        [Tooltip("Extra thickness at the branch base, fading out over the joint - " +
                 "blends the branch into its parent.")]
        public float jointFlare = 1.4f;

        [ToolRange(0f, 1f)]
        [Tooltip("How much the branch thins toward its tip.")]
        public float taper = 0.85f;

        [ToolRange(-3f, 3f)]
        [Tooltip("Positive = droop down, negative = curve up toward the sky.")]
        public float gravity = -0.2f;

        [ToolRange(0f, 2f)]
        [Tooltip("Random bending along the branch. Above 1 = extra crooked/wavy.")]
        public float crookedness = 0.3f;

        [ToolRange(2, 64)]
        [Tooltip("Segments along each branch.")]
        public int segments = 6;

        [FineRange(0f, 2f)]
        [Tooltip("[True Wind only] How much branches of THIS level participate in wind sway (0 = rigid, " +
                 "1 = normal, higher = floppier). Branches are still wood, not leaves - keep this low too " +
                 "(main branches ~0.1-0.2, thin twigs ~0.3-0.5) and let the Leaves' own Wind Flutter " +
                 "Response carry most of the visible motion. Only affects trees using True Wind mode.")]
        public float windResponse = 0.15f;

        public void Validate()
        {
            count.Sort();
            spawnRange.Sort();
            angle.Sort();
            lengthRatio.Sort();
            count.min = Mathf.Max(count.min, 0);
            segments = Mathf.Max(segments, 2);
        }

        public static BranchLevelSettings DefaultMainBranches() => new()
        {
            name = "Main Branches",
            count = new(7, 11),
            spawnRange = new(0.3f, 0.95f),
            angle = new(35f, 65f),
            lengthRatio = new(0.28f, 0.42f),
            radiusRatio = 0.55f,
            gravity = -0.2f,
            crookedness = 0.3f,
            segments = 6,
            radialSegments = 6,
            windResponse = 0.15f
        };

        public static BranchLevelSettings DefaultTwigs() => new()
        {
            name = "Twigs",
            count = new(3, 6),
            spawnRange = new(0.25f, 1f),
            angle = new(30f, 65f),
            lengthRatio = new(0.35f, 0.55f),
            radiusRatio = 0.5f,
            gravity = 0.05f,
            crookedness = 0.35f,
            segments = 4,
            radialSegments = 4,
            windResponse = 0.4f
        };
    }

    public enum RootType
    {
        [InspectorName("Buttress (Big Surface Roots)")]
        Buttress,
        [InspectorName("Pneumatophore (Breathing Roots)")]
        Pneumatophore
    }

    /// <summary>
    /// Optional root system, generated the same way as branches (spline + tube mesh) so it gets
    /// LOD/wind/radial-segment support for free. Two unrelated-looking presets share one enable
    /// toggle and one dropdown since a tree only ever needs one or the other, not both.
    /// </summary>
    [Serializable]
    public class RootSettings
    {
        public bool enabled = false;

        [Tooltip("Buttress = big flared surface roots (fig, ceiba, banyan). Pneumatophore = thin " +
                 "breathing-root spikes around the base (mangrove, bald cypress).")]
        public RootType type = RootType.Buttress;

        [ToolRange(3, 64)]
        [InspectorName("Sides")]
        [Tooltip("Sides around each root (independent of trunk/branches).")]
        public int radialSegments = 6;

        [ToolRange(2, 24)]
        [Tooltip("Segments along each root.")]
        public int segments = 5;

        [FineRange(0f, 1f)]
        [Tooltip("[True Wind only] How much roots participate in wind sway. Keep this at or near 0 - " +
                 "roots are anchored in the ground and shouldn't move.")]
        public float windResponse = 0.02f;

        // --- Buttress (large surface roots) ---
        [MinMaxRange(2, 30)]
        [Tooltip("[Buttress] How many root ridges radiate from the trunk base.")]
        public IntRange buttressCount = new(4, 7);

        [MinMaxRange(0.4f, 15f)]
        [Tooltip("[Buttress] How far each ridge extends from the trunk.")]
        public FloatRange buttressLength = new(0.9f, 1.7f);

        [MinMaxRange(0.02f, 3f)]
        [InspectorName("Root Start Height")]
        [Tooltip("[Buttress] How far up the trunk each ridge starts (meters) - a range instead of one " +
                 "fixed value so ridges start at slightly different heights and don't look perfectly " +
                 "uniform. Real buttress roots flare from partway up the trunk, not just the very bottom.")]
        public FloatRange buttressStartHeight = new(0.1f, 0.25f);

        [ToolRange(1f, 6f)]
        [Tooltip("[Buttress] Extra thickness where the ridge meets the trunk.")]
        public float buttressFlare = 1.8f;

        [ToolRange(0f, 1f)]
        [Tooltip("[Buttress] How much the ridge thins as it extends outward.")]
        public float buttressTaper = 0.85f;

        [ToolRange(0f, 90f)]
        [Tooltip("[Buttress] Downward angle as the ridge extends, so it dives into the ground " +
                 "instead of floating above it.")]
        public float buttressDroop = 22f;

        [ToolRange(0f, 2f)]
        [Tooltip("[Buttress] Random waviness along the ridge. Above 1 = extra crooked/wavy.")]
        public float buttressCrookedness = 0.15f;

        // --- Pneumatophore (breathing roots) ---
        [MinMaxRange(6, 200)]
        [Tooltip("[Pneumatophore] How many spikes scatter around the base.")]
        public IntRange pneumatophoreCount = new(18, 30);

        [MinMaxRange(0.08f, 3f)]
        [Tooltip("[Pneumatophore] Height of each spike above the ground.")]
        public FloatRange pneumatophoreHeight = new(0.15f, 0.35f);

        [MinMaxRange(0.01f, 0.4f)]
        [Tooltip("[Pneumatophore] Thickness of each spike.")]
        public FloatRange pneumatophoreRadius = new(0.02f, 0.045f);

        [MinMaxRange(0.3f, 15f)]
        [Tooltip("[Pneumatophore] How far from the trunk center spikes scatter (annulus around the base).")]
        public FloatRange pneumatophoreSpread = new(0.6f, 2.4f);

        [ToolRange(0f, 2f)]
        [Tooltip("[Pneumatophore] Random lean/crookedness of each spike. Above 1 = extra crooked/wavy.")]
        public float pneumatophoreLean = 0.25f;

        [Tooltip("Small fibrous roots that branch off the main roots, the same way branches grow off " +
                 "the trunk. Works with either root type above.")]
        public FineRootSettings fineRoots = new();

        public void Validate()
        {
            buttressCount.Sort();
            buttressLength.Sort();
            buttressStartHeight.Sort();
            pneumatophoreCount.Sort();
            pneumatophoreHeight.Sort();
            pneumatophoreRadius.Sort();
            pneumatophoreSpread.Sort();
            segments = Mathf.Max(segments, 2);
            fineRoots.Validate();
        }
    }

    /// <summary>Thin fibrous roots spawned along each generated root (Buttress ridge or
    /// Pneumatophore spike), the same recursive idea BranchLevelSettings uses off the trunk.</summary>
    [Serializable]
    public class FineRootSettings
    {
        public bool enabled = false;

        [MinMaxRange(0, 40)]
        [Tooltip("How many fine roots grow on each main root.")]
        public IntRange count = new(3, 6);

        [MinMaxRange(0.05f, 1f)]
        [Tooltip("Length of each fine root relative to the length of the root it grows from.")]
        public FloatRange lengthRatio = new(0.15f, 0.35f);

        [ToolRange(0.02f, 1f)]
        [Tooltip("Thickness relative to the parent root's thickness at the spawn point.")]
        public float radiusRatio = 0.25f;

        [MinMaxRange(0f, 179f)]
        [Tooltip("Angle away from the parent root's direction, in degrees.")]
        public FloatRange angle = new(20f, 70f);

        [ToolRange(0f, 2f)]
        [Tooltip("Random bending along each fine root. Above 1 = extra crooked/wavy.")]
        public float crookedness = 0.4f;

        [ToolRange(2, 16)]
        [Tooltip("Segments along each fine root.")]
        public int segments = 3;

        [ToolRange(3, 16)]
        [InspectorName("Sides")]
        [Tooltip("Sides around each fine root.")]
        public int radialSegments = 4;

        public void Validate()
        {
            count.Sort();
            count.min = Mathf.Max(count.min, 0);
            lengthRatio.Sort();
            angle.Sort();
            segments = Mathf.Max(segments, 2);
        }
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

        [MinMaxRange(0, 800)]
        [Tooltip("How many leaves grow on each eligible branch.")]
        public IntRange countPerBranch = new(10, 16);

        [MinMaxRange(0.005f, 25f)]
        [Tooltip("Leaf card size in meters.")]
        public FloatRange size = new(0.35f, 0.55f);

        [MinMaxRange(0f, 1f)]
        [Tooltip("Where along the branch leaves may spawn (0 = base, 1 = tip).")]
        public FloatRange spawnRange = new(0.35f, 1f);

        [ToolRange(0f, 1f)]
        [InspectorName("Random Rotation")]
        [Tooltip("0 = leaves follow the branch direction, 1 = fully random orientation.")]
        public float orientationRandomness = 0.6f;

        [MinMaxRange(-5f, 10f)]
        [Tooltip("Distance from the branch surface, in meters (random between min and max). " +
                 "Negative = sink into the branch, positive = float away from it.")]
        public FloatRange surfaceOffset = new(0.02f, 0.05f);

        [Tooltip("Extra rotation (degrees) applied to every leaf before the random tumble. " +
                 "Pivot is the leaf stem corner, so this tilts/spins leaves around their attach point.")]
        public Vector3 rotationOffset = Vector3.zero;

        [FineRange(0f, 2f, 1.6f)]
        [Tooltip("[True Wind only] How much leaves flutter/twist on their own in the wind, independent " +
                 "of branch movement (0 = still, 1 = normal, higher = more agitated). Only affects trees " +
                 "using True Wind mode.")]
        public float windFlutterResponse = 1.5f;

        [Min(0)]
        [Tooltip("Leaves grow on branch levels >= this value (trunk = 0, first branch group = 1, ...).")]
        public int minBranchLevel = 2;

        public void Validate()
        {
            countPerBranch.Sort();
            size.Sort();
            spawnRange.Sort();
            surfaceOffset.Sort();
            countPerBranch.min = Mathf.Max(countPerBranch.min, 0);
            minBranchLevel = Mathf.Max(minBranchLevel, 0);
        }
    }

    [Serializable]
    public class MeshSettings
    {
        // radial segment counts moved to TrunkSettings / BranchLevelSettings so
        // each part of the tree controls its own side count independently
        [Min(0.05f)]
        [InspectorName("Bark Texture Tiling")]
        [Tooltip("Vertical bark UV tiling per world meter.")]
        public float barkUVTiling = 1.5f;

        [Tooltip("Needed for normal maps on bark/leaf materials.")]
        public bool generateTangents = true;
    }

    /// <summary>
    /// [True Wind only] Global wind toggles. Per-part response amounts live next to the part they
    /// affect (TrunkSettings.windResponse, BranchLevelSettings.windResponse,
    /// LeafSettings.windFlutterResponse) so "how much this part sways" is set
    /// right where the part itself is authored. Has no effect in Fake Wind mode.
    /// </summary>
    [Serializable]
    public class WindSettings
    {
        [Tooltip("[True Wind only] Bakes per-vertex wind weights into vertex colors, read by the " +
                 "TreeWind shader function (see TreeWind.hlsl) which is driven by a real Unity WindZone " +
                 "at runtime through TreeWindZoneDriver.\n" +
                 "R = main bend x each part's Wind Response (0 at roots, 1 at the top)\n" +
                 "G = local branch sway x that branch level's Wind Response (0 at base, 1 at tip)\n" +
                 "B = leaf flutter x Leaf Wind Flutter Response (0 on bark, matches response on leaves)\n" +
                 "A = random phase per branch / leaf, so parts don't sway in lockstep")]
        public bool bakeWindData = true;

        [ToolRange(0.1f, 6f)]
        [InspectorName("Sway Curve")]
        [Tooltip("Curve of the main bend weight over tree height. Higher = only the top sways.")]
        public float bendExponent = 1.3f;
    }

    [Serializable]
    public class LODLevelSettings
    {
        [ToolRange(0.001f, 1f)]
        [InspectorName("Visible Until (Screen Size)")]
        [Tooltip("This LOD stays visible until the tree covers less than this fraction of the screen. " +
                 "Below the LAST level's value the tree is culled completely.")]
        public float screenHeight = 0.45f;

        [ToolRange(0.1f, 1f)]
        [InspectorName("Detail Level")]
        [Tooltip("Multiplier on radial segments for this LOD.")]
        public float radialResolution = 1f;

        [Min(0)]
        [InspectorName("Max Branch Depth")]
        [Tooltip("Highest branch level whose geometry is included (trunk = 0). " +
                 "Leaves are kept regardless so the canopy silhouette survives.")]
        public int maxBranchLevel = 10;

        [ToolRange(0f, 1f)]
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

        public List<LODLevelSettings> levels = new()
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
