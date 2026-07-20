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
        public readonly List<Branch> Branches = new List<Branch>();
        public readonly List<Leaf> Leaves = new List<Leaf>();
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
            public readonly List<BranchPoint> points = new List<BranchPoint>();

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

            // ---- branch levels ----
            var parents = new List<TreeSkeleton.Branch> { trunk };
            for (int levelIndex = 0; levelIndex < s.branchLevels.Count; levelIndex++)
            {
                var ls = s.branchLevels[levelIndex];
                var children = new List<TreeSkeleton.Branch>();
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
                            float radius = Mathf.Min(pp.radius * ls.radiusRatio, pp.radius);

                            var child = new TreeSkeleton.Branch
                            {
                                id = nextId++,
                                level = levelIndex + 1,
                                length = length,
                                windPhase = (float)rand.NextDouble(),
                                variantRoll = (float)rand.NextDouble()
                            };
                            Grow(child, pp.position, rot, length, radius, 1f - ls.taper,
                                 ls.segments, ls.crookedness, ls.gravity, rand);
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
                                       * Quaternion.AngleAxis(60f, Vector3.right);
                        var tumble = Quaternion.Euler(
                            (float)rand.NextDouble() * 360f,
                            (float)rand.NextDouble() * 360f,
                            (float)rand.NextDouble() * 360f);
                        rot = Quaternion.Slerp(rot, rot * tumble, s.leaves.orientationRandomness);

                        Vector3 outward = rot * Vector3.forward;
                        Vector3 pos = p.position + outward * (p.radius + s.leaves.surfaceOffset);

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

        /// <summary>Grows one branch spline: random bends + gravity/phototropism pull per step.</summary>
        static void Grow(TreeSkeleton.Branch b, Vector3 startPos, Quaternion startRot,
                         float length, float baseRadius, float tipRadiusFactor,
                         int segments, float crookedness, float gravity, System.Random rand,
                         float rootFlare = 1f, float rootFlareHeight = 0f)
        {
            float step = length / segments;
            Vector3 pos = startPos;
            Quaternion rot = startRot;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float radius = baseRadius * Mathf.Lerp(1f, tipRadiusFactor, t);
                if (rootFlareHeight > 0f && t < rootFlareHeight)
                {
                    float f = 1f - t / rootFlareHeight;
                    radius *= Mathf.Lerp(1f, rootFlare, f * f);
                }
                b.points.Add(new TreeSkeleton.BranchPoint
                {
                    position = pos,
                    rotation = rot,
                    radius = Mathf.Max(radius, 0.003f)
                });

                if (i == segments)
                    break;

                // random bend
                float bend = crookedness * 22f;
                rot = rot * Quaternion.Euler(
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
