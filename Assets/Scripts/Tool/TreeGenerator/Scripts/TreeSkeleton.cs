using System.Collections.Generic;
using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// The generated structure of a tree (branch splines + leaf placements).
    /// Generated once per rebuild, then turned into one mesh per LOD level.
    /// </summary>
    public class TreeSkeleton
    {
        public readonly List<Branch> Branches = new();
        public readonly List<Branch> Roots = new();
        public readonly List<Leaf> Leaves = new();
        public float TotalHeight = 0.01f;

        public struct BranchPoint
        {
            public Vector3 position;
            public Quaternion rotation; // local forward = growth direction
            public float radius;
        }

        public class Branch
        {
            public int id;
            public int level;      // 0 = trunk, 1 = first branch group, ...
            public float length;
            public float windPhase;
            public float variantRoll; // 0-1, picks which prefab variant this branch uses
            public int radialOverride = -1; // >0 overrides the level/root-wide side count (used by fine roots)
            public readonly List<BranchPoint> points = new();

            public BranchPoint Sample(float t)
            {
                t = Mathf.Clamp01(t);
                float f = t * (points.Count - 1);
                int i = Mathf.Min(Mathf.FloorToInt(f), points.Count - 2);
                float u = f - i;
                BranchPoint a = points[i];
                BranchPoint b = points[i + 1];
                return new BranchPoint
                {
                    position = Vector3.LerpUnclamped(a.position, b.position, u),
                    rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, u),
                    radius = Mathf.LerpUnclamped(a.radius, b.radius, u)
                };
            }
        }

        public struct Leaf
        {
            public int index;
            public int branchLevel;
            public Vector3 position;
            public Quaternion rotation; // forward = away from branch, up = card normal
            public float size;
            public float windPhase;
            public float variantRoll; // 0-1, picks which prefab variant this leaf uses
            public float heightT; // attach height / total tree height
        }
    }

    /// <summary>
    /// Builds a TreeSkeleton from settings. Fully deterministic:
    /// - the master seed drives the trunk,
    /// - each branch gets its own random stream derived from (seed, branchSeed, level, parent id),
    /// - each leaf group gets its own stream derived from (seed, leafSeed, branch id).
    /// So changing leafSeed never moves a branch, and changing branchSeed never bends the trunk.
    /// </summary>
    public static class TreeSkeletonGenerator
    {
        public static TreeSkeleton Generate(ProceduralTreeSettings s)
        {
            var sk = new TreeSkeleton();
            int nextId = 0;

            // ---- trunk ----
            var trunkRand = CreateRand(s.seed, 0, 0);
            var trunk = new TreeSkeleton.Branch { id = nextId++, level = 0 };
            float height = s.trunk.height.Random(trunkRand);
            float baseRadius = s.trunk.radius.Random(trunkRand);
            trunk.length = height;
            trunk.windPhase = (float)trunkRand.NextDouble();
            trunk.variantRoll = (float)trunkRand.NextDouble();

            float leanAngle = s.trunk.lean.Random(trunkRand);
            float leanAzimuth = (float)trunkRand.NextDouble() * 360f;
            Quaternion baseRot = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            Vector3 leanAxis = Quaternion.AngleAxis(leanAzimuth, Vector3.up) * Vector3.right;
            Quaternion trunkRot = Quaternion.AngleAxis(leanAngle, leanAxis) * baseRot;

            Grow(trunk, Vector3.zero, trunkRot, height, baseRadius, 1f - s.trunk.taper,
                 s.trunk.segments, s.trunk.crookedness, 0f, trunkRand,
                 s.trunk.rootFlare, s.trunk.rootFlareHeight);
            sk.Branches.Add(trunk);

            // ---- root system (optional) ----
            // Roots get their OWN id counter (see GenerateRoots) instead of sharing nextId with
            // branches - branch/leaf random streams are seeded partly by parent.id/branch.id
            // (CreateRand(..., parent.id) / CreateRand(..., b.id) below), so if roots consumed
            // nextId here, turning roots on/off or changing the root count would shift every
            // later branch's id and silently reshuffle branch and leaf positions too.
            if (s.roots.enabled)
                GenerateRoots(sk, s, trunk, baseRadius, height);

            // ---- branch levels ----
            var parents = new List<TreeSkeleton.Branch> { trunk };
            for (int levelIndex = 0; levelIndex < s.branchLevels.Count; levelIndex++)
            {
                var ls = s.branchLevels[levelIndex];
                List<TreeSkeleton.Branch> children = new();
                if (ls.enabled)
                {
                    foreach (var parent in parents)
                    {
                        var rand = CreateRand(s.seed ^ (s.branchSeed * 486187739), levelIndex + 1, parent.id);
                        int count = ls.count.Random(rand);
                        for (int i = 0; i < count; i++)
                        {
                            // even spacing with jitter so branches don't clump
                            float t = Mathf.Lerp(ls.spawnRange.min, ls.spawnRange.max,
                                count <= 1 ? (float)rand.NextDouble() : (i + (float)rand.NextDouble()) / count);
                            var pp = parent.Sample(t);

                            float azimuth = i * 137.508f + ((float)rand.NextDouble() * 2f - 1f) * 180f * ls.azimuthRandomness;
                            float outAngle = ls.angle.Random(rand);
                            Quaternion rot = pp.rotation
                                           * Quaternion.AngleAxis(azimuth, Vector3.forward)
                                           * Quaternion.AngleAxis(outAngle, Vector3.right);

                            float length = parent.length * ls.lengthRatio.Random(rand)
                                         * Mathf.Lerp(1f, 1f - ls.lengthFalloff, t);
                            if (length < 0.02f)
                                continue;
                            float radius = pp.radius * ls.radiusRatio * ls.thicknessScale;

                            var child = new TreeSkeleton.Branch
                            {
                                id = nextId++,
                                level = levelIndex + 1,
                                length = length,
                                windPhase = (float)rand.NextDouble(),
                                variantRoll = (float)rand.NextDouble()
                            };
                            // jointFlare thickens the base and jointSmoothing curves the
                            // branch out of the parent's direction, hiding the joint
                            Grow(child, pp.position, rot, length, radius, 1f - ls.taper,
                                 ls.segments, ls.crookedness, ls.gravity, rand,
                                 ls.jointFlare, Mathf.Max(0.2f, ls.jointSmoothing * 0.8f),
                                 pp.rotation, ls.jointSmoothing);
                            children.Add(child);
                        }
                    }
                }
                sk.Branches.AddRange(children);
                parents = children;
                if (children.Count == 0)
                    break;
            }

            // ---- total height (needed for wind weights) ----
            float maxY = 0.01f;
            foreach (var b in sk.Branches)
                foreach (var p in b.points)
                    maxY = Mathf.Max(maxY, p.position.y);
            sk.TotalHeight = maxY;

            // ---- leaves ----
            if (s.leaves.enabled)
            {
                int leafIndex = 0;
                foreach (var b in sk.Branches)
                {
                    if (b.level < s.leaves.minBranchLevel)
                        continue;
                    var rand = CreateRand(s.seed ^ (s.leafSeed * 743), 1000, b.id);
                    int count = s.leaves.countPerBranch.Random(rand);
                    for (int i = 0; i < count; i++)
                    {
                        float t = s.leaves.spawnRange.Random(rand);
                        var p = b.Sample(t);

                        float azimuth = (float)rand.NextDouble() * 360f;
                        Quaternion rot = p.rotation
                                       * Quaternion.AngleAxis(azimuth, Vector3.forward)
                                       * Quaternion.AngleAxis(60f, Vector3.right)
                                       * Quaternion.Euler(s.leaves.rotationOffset);
                        var tumble = Quaternion.Euler(
                            (float)rand.NextDouble() * 360f,
                            (float)rand.NextDouble() * 360f,
                            (float)rand.NextDouble() * 360f);
                        rot = Quaternion.Slerp(rot, rot * tumble, s.leaves.orientationRandomness);

                        Vector3 outward = rot * Vector3.forward;
                        float offset = s.leaves.surfaceOffset.Random(rand);
                        Vector3 pos = p.position + outward * (p.radius + offset);

                        sk.Leaves.Add(new TreeSkeleton.Leaf
                        {
                            index = leafIndex++,
                            branchLevel = b.level,
                            position = pos,
                            rotation = rot,
                            size = s.leaves.size.Random(rand),
                            windPhase = (float)rand.NextDouble(),
                            variantRoll = (float)rand.NextDouble(),
                            heightT = Mathf.Clamp01(p.position.y / sk.TotalHeight)
                        });
                    }
                }
            }

            return sk;
        }

        /// <summary>
        /// Builds the optional root system as ordinary Branch splines (same spline+tube
        /// pipeline as everything else, so roots get LOD/wind/radial-segment support for
        /// free) but stored in <see cref="TreeSkeleton.Roots"/> instead of Branches, so
        /// leaves never try to grow on them and they're built as their own dedicated pass
        /// in TreeMeshBuilder using RootSettings rather than a branch level's settings.
        /// level is left at 0 on purpose: TreeMeshBuilder.BuildBranchTube forces the local
        /// branch-sway (G) channel to 0 for level 0, which is exactly right for roots -
        /// grounded wood, no local whip, just the same negligible whole-tree lean bark gets.
        /// </summary>
        static void GenerateRoots(TreeSkeleton sk, ProceduralTreeSettings s, TreeSkeleton.Branch trunk,
                                  float baseRadius, float trunkHeight)
        {
            var rand = CreateRand(s.seed ^ (s.rootSeed * 92821), 3000, 0);
            var r = s.roots;
            // local counter, independent from the trunk/branch id sequence - nothing outside the
            // root system ever looks up a root by id, so roots don't need to share that sequence
            int rootId = 0;

            if (r.type == RootType.Buttress)
            {
                int count = r.buttressCount.Random(rand);

                for (int i = 0; i < count; i++)
                {
                    // start height sampled PER ROOT (it's a range) so ridges don't all break away
                    // from the trunk at the exact same height - looks less mechanical/uniform
                    float startT = trunkHeight > 0.001f
                        ? Mathf.Clamp01(r.buttressStartHeight.Random(rand) / trunkHeight) : 0f;
                    var basePoint = trunk.Sample(startT);

                    // even spacing with jitter so ridges don't clump, same trick branches use
                    float azimuth = i * (360f / count) + ((float)rand.NextDouble() * 2f - 1f) * 25f;
                    // 90 = straight out from the trunk (perpendicular); Droop tips it further down
                    // so the ridge dives toward the ground instead of floating level with it
                    float outAngle = 90f + r.buttressDroop * (float)rand.NextDouble();
                    Quaternion rot = basePoint.rotation
                                   * Quaternion.AngleAxis(azimuth, Vector3.forward)
                                   * Quaternion.AngleAxis(outAngle, Vector3.right);

                    float length = r.buttressLength.Random(rand);
                    float radius = basePoint.radius * r.buttressFlare;

                    var root = new TreeSkeleton.Branch
                    {
                        id = rootId++,
                        level = 0,
                        length = length,
                        windPhase = (float)rand.NextDouble(),
                        variantRoll = (float)rand.NextDouble()
                    };
                    // curveFrom/curveBlend: starts out along the trunk's own local direction and
                    // curves smoothly into the outward+down direction, hiding the trunk joint -
                    // same mechanic BranchLevelSettings.jointSmoothing uses
                    Grow(root, basePoint.position, rot, length, radius, 1f - r.buttressTaper,
                         s.roots.segments, r.buttressCrookedness, 0.5f, rand,
                         1.2f, 0.3f, basePoint.rotation, 0.35f);
                    sk.Roots.Add(root);

                    if (r.fineRoots.enabled)
                        GenerateFineRoots(sk, r.fineRoots, root, rand, ref rootId);
                }
            }
            else // Pneumatophore
            {
                int count = r.pneumatophoreCount.Random(rand);
                Quaternion spikeBase = Quaternion.LookRotation(Vector3.up, Vector3.forward);

                for (int i = 0; i < count; i++)
                {
                    // scattered (not evenly spaced) in a ring around the base, like real
                    // mangrove pneumatophores poking up out of the mud wherever a root happens to be
                    float scatterAzimuth = (float)rand.NextDouble() * 360f;
                    float scatterRadius = Mathf.Lerp(baseRadius * 1.05f,
                        baseRadius + r.pneumatophoreSpread.Random(rand), (float)rand.NextDouble());
                    Vector3 startPos = new Vector3(
                        Mathf.Cos(scatterAzimuth * Mathf.Deg2Rad) * scatterRadius, 0f,
                        Mathf.Sin(scatterAzimuth * Mathf.Deg2Rad) * scatterRadius);

                    float leanAngle = r.pneumatophoreLean * 30f * (float)rand.NextDouble();
                    float leanAzimuth = (float)rand.NextDouble() * 360f;
                    Vector3 leanAxis = Quaternion.AngleAxis(leanAzimuth, Vector3.up) * Vector3.right;
                    Quaternion startRot = Quaternion.AngleAxis(leanAngle, leanAxis) * spikeBase;

                    float spikeHeight = r.pneumatophoreHeight.Random(rand);
                    float spikeRadius = r.pneumatophoreRadius.Random(rand);

                    var spike = new TreeSkeleton.Branch
                    {
                        id = rootId++,
                        level = 0,
                        length = spikeHeight,
                        windPhase = (float)rand.NextDouble(),
                        variantRoll = (float)rand.NextDouble()
                    };
                    Grow(spike, startPos, startRot, spikeHeight, spikeRadius, 0.5f,
                         Mathf.Max(2, s.roots.segments / 2), r.pneumatophoreLean * 0.4f, -0.1f, rand);
                    sk.Roots.Add(spike);

                    if (r.fineRoots.enabled)
                        GenerateFineRoots(sk, r.fineRoots, spike, rand, ref rootId);
                }
            }
        }

        /// <summary>
        /// Thin fibrous roots branching off a main root (Buttress ridge or Pneumatophore spike),
        /// same recursive spawn-along-a-parent-spline idea BranchLevelSettings uses off the trunk.
        /// Stored in TreeSkeleton.Roots too, so they ride the same LOD/wind/mesh pipeline. Uses
        /// gravity = 0.6 (droop) unconditionally so fibrous roots dive toward the soil regardless
        /// of which way their parent root points, matching how real fine roots grow.
        /// </summary>
        static void GenerateFineRoots(TreeSkeleton sk, FineRootSettings fr, TreeSkeleton.Branch parent,
                                      System.Random rand, ref int rootId)
        {
            int count = fr.count.Random(rand);
            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? (float)rand.NextDouble() : (i + (float)rand.NextDouble()) / count;
                var pp = parent.Sample(t);

                float azimuth = (float)rand.NextDouble() * 360f;
                float outAngle = fr.angle.Random(rand);
                Quaternion rot = pp.rotation
                               * Quaternion.AngleAxis(azimuth, Vector3.forward)
                               * Quaternion.AngleAxis(outAngle, Vector3.right);

                float length = parent.length * fr.lengthRatio.Random(rand);
                if (length < 0.01f)
                    continue;
                float radius = pp.radius * fr.radiusRatio;

                var fine = new TreeSkeleton.Branch
                {
                    id = rootId++,
                    level = 0,
                    length = length,
                    windPhase = (float)rand.NextDouble(),
                    variantRoll = (float)rand.NextDouble(),
                    radialOverride = fr.radialSegments
                };
                Grow(fine, pp.position, rot, length, radius, 0.3f,
                     fr.segments, fr.crookedness, 0.6f, rand,
                     1f, 0f, pp.rotation, 0.5f);
                sk.Roots.Add(fine);
            }
        }

        /// <summary>
        /// Grows one branch spline: random bends + gravity/phototropism pull per step.
        /// When curveFrom/curveBlend are given the branch starts out along the
        /// parent's direction and bends smoothly into its own over the first
        /// part of its length, so the joint reads as one continuous limb.
        /// </summary>
        static void Grow(TreeSkeleton.Branch b, Vector3 startPos, Quaternion startRot,
                         float length, float baseRadius, float tipRadiusFactor,
                         int segments, float crookedness, float gravity, System.Random rand,
                         float baseFlare = 1f, float baseFlareHeight = 0f,
                         Quaternion? curveFrom = null, float curveBlend = 0f)
        {
            float step = length / segments;
            Vector3 pos = startPos;
            Quaternion rot = startRot;

            int curveSteps = 0;
            float curvePerStep = 0f;
            if (curveFrom.HasValue && curveBlend > 0.001f)
            {
                rot = Quaternion.Slerp(curveFrom.Value, startRot, 0.2f);
                curveSteps = Mathf.Max(1, Mathf.RoundToInt(curveBlend * segments));
                curvePerStep = Quaternion.Angle(rot, startRot) / curveSteps;
            }

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float radius = baseRadius * Mathf.Lerp(1f, tipRadiusFactor, t);
                if (baseFlareHeight > 0f && t < baseFlareHeight)
                {
                    float f = 1f - t / baseFlareHeight;
                    radius *= Mathf.Lerp(1f, baseFlare, f * f);
                }
                b.points.Add(new TreeSkeleton.BranchPoint
                {
                    position = pos,
                    rotation = rot,
                    radius = Mathf.Max(radius, 0.003f)
                });

                if (i == segments)
                    break;

                // curve out of the parent's direction into our own
                if (i < curveSteps)
                    rot = Quaternion.RotateTowards(rot, startRot, curvePerStep);

                // random bend
                float bend = crookedness * 22f;
                rot *= Quaternion.Euler(
                    ((float)rand.NextDouble() * 2f - 1f) * bend,
                    ((float)rand.NextDouble() * 2f - 1f) * bend,
                    0f);

                // gravity (droop) or phototropism (grow toward the sky)
                if (Mathf.Abs(gravity) > 0.0001f)
                {
                    Vector3 fwd = rot * Vector3.forward;
                    Vector3 target = gravity > 0f ? Vector3.down : Vector3.up;
                    float pullRad = Mathf.Abs(gravity) * (60f / segments) * Mathf.Deg2Rad;
                    Vector3 desired = Vector3.RotateTowards(fwd, target, pullRad, 0f);
                    rot = Quaternion.FromToRotation(fwd, desired) * rot;
                }

                pos += rot * Vector3.forward * step;
            }
        }

        /// <summary>Deterministic random stream from (seed, stream, id).</summary>
        static System.Random CreateRand(int seed, int stream, int id)
        {
            unchecked
            {
                int h = seed;
                h = h * 486187739 + stream;
                h = h * 743116741 + id;
                h ^= h >> 13;
                h *= 373587911;
                h ^= h >> 16;
                return new System.Random(h);
            }
        }
    }
}
