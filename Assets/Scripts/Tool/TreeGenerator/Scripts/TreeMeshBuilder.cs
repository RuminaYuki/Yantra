using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TreeTool
{
    /// <summary>
    /// Turns a TreeSkeleton into a mesh for one LOD level.
    /// Submesh 0 = bark, submesh 1 = leaves.
    /// Wind weights are baked into vertex colors (see WindSettings tooltip).
    /// </summary>
    public static class TreeMeshBuilder
    {
        // prefab meshes are extracted once and cached; the cache is cleared on
        // every full (non-interactive) rebuild so edited prefabs get picked up
        static readonly Dictionary<GameObject, SourcePart> s_partCache =
            new Dictionary<GameObject, SourcePart>();

        public static void ClearPrefabCache() => s_partCache.Clear();

        /// <summary>
        /// Fills <paramref name="into"/> with the mesh for one LOD level.
        /// skipTangents = true is used during interactive slider drags (tangent
        /// recalculation is the most expensive step).
        /// </summary>
        public static void Build(TreeSkeleton sk, ProceduralTreeSettings s, LODLevelSettings lod,
                                 string meshName, Mesh into, bool skipTangents, out int leafCount)
        {
            List<Vector3> verts = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<Color> colors = new();
            List<int> barkTris = new();
            List<int> leafTris = new();

            float totalHeight = Mathf.Max(sk.TotalHeight, 0.01f);
            bool wind = s.wind.bakeWindData;
            float bendExp = s.wind.bendExponent;

            // ---- custom prefab sources (mode 2) ----
            SourcePart[] trunkParts = CollectParts(s.geometry.trunkSource, s.geometry.trunkPrefabs, s_partCache);
            SourcePart[] branchParts = CollectParts(s.geometry.branchSource, s.geometry.branchPrefabs, s_partCache);
            SourcePart[] leafParts = CollectParts(s.geometry.leafSource, s.geometry.leafPrefabs, s_partCache);

            // ---- bark ----
            foreach (var b in sk.Branches)
            {
                if (b.level > lod.maxBranchLevel)
                    continue;

                // each part carries its own side count and wind response: trunk vs. every branch level
                BranchLevelSettings level = b.level > 0
                    ? s.branchLevels[Mathf.Min(b.level - 1, s.branchLevels.Count - 1)]
                    : null;
                int baseRadial = level?.radialSegments ?? s.trunk.radialSegments;
                float windResponse = level?.windResponse ?? s.trunk.windResponse;

                SourcePart src = Pick(b.level == 0 ? trunkParts : branchParts, b.variantRoll);
                if (src != null && src.heightValid)
                {
                    BuildCustomBranchMesh(b, src, totalHeight, bendExp, windResponse, wind,
                                          verts, normals, uvs, colors, barkTris);
                    continue;
                }

                int radial = Mathf.Max(3, Mathf.RoundToInt(baseRadial * lod.radialResolution));
                BuildBranchTube(b, radial, s.mesh.barkUVTiling, totalHeight, bendExp, windResponse, wind,
                                verts, normals, uvs, colors, barkTris);
            }

            // ---- roots (optional, same tube pipeline as bark) ----
            if (s.roots.enabled)
            {
                int rootRadial = Mathf.Max(3, Mathf.RoundToInt(s.roots.radialSegments * lod.radialResolution));
                foreach (var root in sk.Roots)
                {
                    // fine roots carry their own (usually thinner) side count via radialOverride
                    int radial = root.radialOverride > 0
                        ? Mathf.Max(3, Mathf.RoundToInt(root.radialOverride * lod.radialResolution))
                        : rootRadial;
                    BuildBranchTube(root, radial, s.mesh.barkUVTiling, totalHeight, bendExp,
                                    s.roots.windResponse, wind, verts, normals, uvs, colors, barkTris);
                }
            }

            // ---- leaves ----
            leafCount = 0;
            if (s.leaves.enabled && lod.leafDensity > 0f)
            {
                float density = lod.leafDensity;
                float sizeMul = (s.lods.compensateLeafSize && density < 1f)
                    ? Mathf.Clamp(1f / Mathf.Sqrt(Mathf.Max(density, 0.05f)), 1f, 1.8f)
                    : 1f;

                float flutterResponse = s.leaves.windFlutterResponse;
                foreach (var leaf in sk.Leaves)
                {
                    // deterministic thinning: same leaves survive every rebuild
                    if (density < 1f && Frac(leaf.index * 0.61803398875f) > density)
                        continue;

                    // leaves inherit their host branch's sway response for the local-flex channel
                    float hostResponse = leaf.branchLevel == 0
                        ? s.trunk.windResponse
                        : s.branchLevels[Mathf.Min(leaf.branchLevel - 1, s.branchLevels.Count - 1)].windResponse;

                    SourcePart src = Pick(leafParts, leaf.variantRoll);
                    if (src != null)
                        BuildCustomLeafMesh(leaf, src, sizeMul, bendExp, hostResponse, flutterResponse, wind,
                                            verts, normals, uvs, colors, leafTris);
                    else
                        BuildLeaf(leaf, s.leaves.shape, sizeMul, bendExp, hostResponse, flutterResponse, wind,
                                  verts, normals, uvs, colors, leafTris);
                    leafCount++;
                }
            }

            into.Clear();
            into.name = meshName;
            into.indexFormat = verts.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            into.SetVertices(verts);
            into.SetNormals(normals);
            into.SetUVs(0, uvs);
            into.SetColors(colors);
            into.subMeshCount = 2;
            into.SetTriangles(barkTris, 0);
            into.SetTriangles(leafTris, 1);
            if (s.mesh.generateTangents && !skipTangents)
                into.RecalculateTangents();
            into.RecalculateBounds();
        }

        static void BuildBranchTube(TreeSkeleton.Branch b, int radial, float uvTiling,
                                    float totalHeight, float bendExp, float windResponse, bool wind,
                                    List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                                    List<Color> colors, List<int> tris)
        {
            var pts = b.points;
            int ringVerts = radial + 1; // extra vertex for the UV seam
            int baseIndex = verts.Count;
            float vCoord = 0f;
            Color c = Color.clear;

            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                if (i > 0)
                    vCoord += Vector3.Distance(pts[i - 1].position, p.position) * uvTiling;

                if (wind)
                {
                    float tAlong = (float)i / (pts.Count - 1);
                    // Vertex colors are stored 8-bit-per-channel on the mesh, so any value
                    // baked here above 1 silently clamps on the GPU - clamp here too so what
                    // TreeMeshBuilder computes always matches what the shader actually reads
                    // (unclamped values above 1 previously created a sharp, unintended kink
                    // wherever the curve crossed 1 and got clipped flat by the mesh format).
                    c = new Color(
                        Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(p.position.y / totalHeight), bendExp) * windResponse), // R main bend
                        Mathf.Clamp01((b.level == 0 ? 0f : tAlong) * windResponse),     // G local branch sway
                        0f,                                                            // B flutter (bark = 0)
                        b.windPhase);                                                  // A phase
                }

                for (int r = 0; r <= radial; r++)
                {
                    float ang = (float)r / radial * Mathf.PI * 2f;
                    Vector3 dir = p.rotation * new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                    verts.Add(p.position + dir * p.radius);
                    normals.Add(dir);
                    uvs.Add(new Vector2((float)r / radial, vCoord));
                    colors.Add(c);
                }
            }

            // side quads
            for (int i = 0; i < pts.Count - 1; i++)
            {
                int row0 = baseIndex + i * ringVerts;
                int row1 = row0 + ringVerts;
                for (int r = 0; r < radial; r++)
                {
                    tris.Add(row0 + r); tris.Add(row0 + r + 1); tris.Add(row1 + r);
                    tris.Add(row0 + r + 1); tris.Add(row1 + r + 1); tris.Add(row1 + r);
                }
            }

            // tip cap
            var tip = pts[pts.Count - 1];
            Vector3 tipDir = tip.rotation * Vector3.forward;
            int tipIndex = verts.Count;
            verts.Add(tip.position + tipDir * tip.radius);
            normals.Add(tipDir);
            uvs.Add(new Vector2(0.5f, vCoord + tip.radius * uvTiling));
            colors.Add(c);

            int lastRow = baseIndex + (pts.Count - 1) * ringVerts;
            for (int r = 0; r < radial; r++)
            {
                tris.Add(lastRow + r); tris.Add(lastRow + r + 1); tris.Add(tipIndex);
            }
        }

        static void BuildLeaf(TreeSkeleton.Leaf leaf, LeafShape shape, float sizeMul,
                              float bendExp, float hostResponse, float flutterResponse, bool wind,
                              List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                              List<Color> colors, List<int> tris)
        {
            int planes = shape switch
            {
                LeafShape.Quad => 1,
                LeafShape.Cross => 2,
                _ => 3
            };
            float spin = 180f / planes;
            float size = leaf.size * sizeMul;

            Color c = wind
                ? new Color(Mathf.Clamp01(Mathf.Pow(leaf.heightT, bendExp) * hostResponse), Mathf.Clamp01(hostResponse), Mathf.Clamp01(flutterResponse), leaf.windPhase)
                : Color.clear;

            for (int pIdx = 0; pIdx < planes; pIdx++)
            {
                // The pivot is the card's bottom-left corner (UV 0,0) - the leaf
                // stem - glued to the branch. The extra -45° spin around the card
                // normal makes the card's DIAGONAL point away from the branch, so
                // a leaf texture drawn stem-at-bottom-left, tip-at-top-right sits
                // correctly.
                Quaternion rot = leaf.rotation
                               * Quaternion.AngleAxis(spin * pIdx, Vector3.forward)
                               * Quaternion.AngleAxis(-45f, Vector3.up);
                Vector3 right = rot * Vector3.right;
                Vector3 fwd = rot * Vector3.forward;
                Vector3 normal = rot * Vector3.up;

                int i0 = verts.Count;
                verts.Add(leaf.position);                                // UV (0,0) - stem
                verts.Add(leaf.position + right * size);                 // UV (1,0)
                verts.Add(leaf.position + right * size + fwd * size);    // UV (1,1) - tip
                verts.Add(leaf.position + fwd * size);                   // UV (0,1)
                for (int k = 0; k < 4; k++)
                {
                    normals.Add(normal);
                    colors.Add(c);
                }
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 1);
                tris.Add(i0); tris.Add(i0 + 3); tris.Add(i0 + 2);
            }
        }

        // ------------------------------------------------------------------
        // Custom prefab geometry (mode 2)
        // ------------------------------------------------------------------

        /// <summary>A prefab's meshes flattened into one vertex/triangle list (prefab local space).</summary>
        class SourcePart
        {
            public Vector3[] verts;
            public Vector3[] normals;
            public Vector2[] uvs;
            public int[] tris;
            public float minY, maxY;    // height range for spline bending
            public float refRadius;     // XZ radius near the base, used to fit the branch radius
            public bool heightValid;    // tall enough to bend along a spline
        }

        static SourcePart[] CollectParts(GeometrySource source, List<GameObject> prefabs,
                                         Dictionary<GameObject, SourcePart> cache)
        {
            if (source != GeometrySource.Prefabs || prefabs == null)
                return null;
            List<SourcePart> list = new();
            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                    continue;
                if (!cache.TryGetValue(prefab, out SourcePart part))
                {
                    part = ExtractPart(prefab);
                    cache[prefab] = part;
                }
                if (part != null)
                    list.Add(part);
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

        static SourcePart Pick(SourcePart[] parts, float roll)
        {
            if (parts == null || parts.Length == 0)
                return null;
            return parts[Mathf.Min((int)(roll * parts.Length), parts.Length - 1)];
        }

        static SourcePart ExtractPart(GameObject prefab)
        {
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            List<Vector3> v = new();
            List<Vector3> n = new();
            List<Vector2> u = new();
            List<int> t = new();
            Matrix4x4 rootInv = prefab.transform.worldToLocalMatrix;

            foreach (var mf in filters)
            {
                Mesh m = mf.sharedMesh;
                if (m == null)
                    continue;
                Matrix4x4 mtx = rootInv * mf.transform.localToWorldMatrix;
                int baseIdx = v.Count;
                Vector3[] mv = m.vertices;
                Vector3[] mn = m.normals;
                Vector2[] mu = m.uv;
                for (int i = 0; i < mv.Length; i++)
                {
                    v.Add(mtx.MultiplyPoint3x4(mv[i]));
                    n.Add(mn.Length == mv.Length ? mtx.MultiplyVector(mn[i]).normalized : Vector3.up);
                    u.Add(mu.Length == mv.Length ? mu[i] : Vector2.zero);
                }
                for (int sm = 0; sm < m.subMeshCount; sm++)
                {
                    int[] idx = m.GetTriangles(sm);
                    for (int i = 0; i < idx.Length; i++)
                        t.Add(baseIdx + idx[i]);
                }
            }
            if (v.Count == 0 || t.Count == 0)
                return null;

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in v)
            {
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            float cut = minY + (maxY - minY) * 0.15f;
            float rBase = 0f, rAll = 0f;
            foreach (var p in v)
            {
                float rr = Mathf.Sqrt(p.x * p.x + p.z * p.z);
                rAll = Mathf.Max(rAll, rr);
                if (p.y <= cut)
                    rBase = Mathf.Max(rBase, rr);
            }

            return new SourcePart
            {
                verts = v.ToArray(),
                normals = n.ToArray(),
                uvs = u.ToArray(),
                tris = t.ToArray(),
                minY = minY,
                maxY = maxY,
                refRadius = rBase > 1e-4f ? rBase : (rAll > 1e-4f ? rAll : 1f),
                heightValid = maxY - minY > 1e-4f
            };
        }

        /// <summary>
        /// Bends a prefab mesh (modeled along +Y, pivot at the base) along the branch spline.
        /// Vertex height maps to spline t; the XZ cross-section is scaled so the mesh base
        /// matches the branch radius (spline taper/root flare apply on top of the modeled shape).
        /// </summary>
        static void BuildCustomBranchMesh(TreeSkeleton.Branch b, SourcePart src,
                                          float totalHeight, float bendExp, float windResponse, bool wind,
                                          List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                                          List<Color> colors, List<int> tris)
        {
            int baseIdx = verts.Count;
            float invH = 1f / (src.maxY - src.minY);
            float invR = 1f / src.refRadius;

            for (int i = 0; i < src.verts.Length; i++)
            {
                Vector3 v = src.verts[i];
                float t = Mathf.Clamp01((v.y - src.minY) * invH);
                var p = b.Sample(t);
                float radial = p.radius * invR;

                // mesh local (x, y, z) -> (x, -z, y): +Y becomes the spline direction
                // without mirroring, so the original triangle winding stays correct.
                verts.Add(p.position + p.rotation * new Vector3(v.x * radial, -v.z * radial, 0f));
                Vector3 nrm = src.normals[i];
                normals.Add((p.rotation * new Vector3(nrm.x, -nrm.z, nrm.y)).normalized);
                uvs.Add(src.uvs[i]);
                colors.Add(wind
                    ? new Color(Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(p.position.y / totalHeight), bendExp) * windResponse),
                                Mathf.Clamp01((b.level == 0 ? 0f : t) * windResponse), 0f, b.windPhase)
                    : Color.clear);
            }
            for (int i = 0; i < src.tris.Length; i++)
                tris.Add(baseIdx + src.tris[i]);
        }

        /// <summary>
        /// Places a leaf/cluster prefab (pivot at attach point, growing along +Z, +Y = normal)
        /// at the leaf position, scaled by the leaf size.
        /// </summary>
        static void BuildCustomLeafMesh(TreeSkeleton.Leaf leaf, SourcePart src, float sizeMul,
                                        float bendExp, float hostResponse, float flutterResponse, bool wind,
                                        List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                                        List<Color> colors, List<int> tris)
        {
            int baseIdx = verts.Count;
            float scale = leaf.size * sizeMul;
            Color c = wind
                ? new Color(Mathf.Clamp01(Mathf.Pow(leaf.heightT, bendExp) * hostResponse), Mathf.Clamp01(hostResponse), Mathf.Clamp01(flutterResponse), leaf.windPhase)
                : Color.clear;

            for (int i = 0; i < src.verts.Length; i++)
            {
                verts.Add(leaf.position + leaf.rotation * (src.verts[i] * scale));
                normals.Add(leaf.rotation * src.normals[i]);
                uvs.Add(src.uvs[i]);
                colors.Add(c);
            }
            for (int i = 0; i < src.tris.Length; i++)
                tris.Add(baseIdx + src.tris[i]);
        }

        static float Frac(float f) => f - Mathf.Floor(f);
    }
}
