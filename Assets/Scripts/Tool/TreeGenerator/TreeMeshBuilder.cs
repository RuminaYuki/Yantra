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
        public static Mesh Build(TreeSkeleton sk, ProceduralTreeSettings s, LODLevelSettings lod,
                                 string meshName, out int leafCount)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var barkTris = new List<int>();
            var leafTris = new List<int>();

            float totalHeight = Mathf.Max(sk.TotalHeight, 0.01f);
            bool wind = s.wind.bakeWindData;
            float bendExp = s.wind.bendExponent;

            // ---- custom prefab sources (mode 2) ----
            var cache = new Dictionary<GameObject, SourcePart>();
            SourcePart[] trunkParts = CollectParts(s.geometry.trunkSource, s.geometry.trunkPrefabs, cache);
            SourcePart[] branchParts = CollectParts(s.geometry.branchSource, s.geometry.branchPrefabs, cache);
            SourcePart[] leafParts = CollectParts(s.geometry.leafSource, s.geometry.leafPrefabs, cache);

            // ---- bark ----
            foreach (var b in sk.Branches)
            {
                if (b.level > lod.maxBranchLevel)
                    continue;

                SourcePart src = Pick(b.level == 0 ? trunkParts : branchParts, b.variantRoll);
                if (src != null && src.heightValid)
                {
                    BuildCustomBranchMesh(b, src, totalHeight, bendExp, wind,
                                          verts, normals, uvs, colors, barkTris);
                    continue;
                }

                int radial = Mathf.Max(3, Mathf.RoundToInt(
                    s.mesh.radialSegments * lod.radialResolution * Mathf.Pow(s.mesh.radialDecayPerLevel, b.level)));
                BuildBranchTube(b, radial, s.mesh.barkUVTiling, totalHeight, bendExp, wind,
                                verts, normals, uvs, colors, barkTris);
            }

            // ---- leaves ----
            leafCount = 0;
            if (s.leaves.enabled && lod.leafDensity > 0f)
            {
                float density = lod.leafDensity;
                float sizeMul = (s.lods.compensateLeafSize && density < 1f)
                    ? Mathf.Clamp(1f / Mathf.Sqrt(Mathf.Max(density, 0.05f)), 1f, 1.8f)
                    : 1f;

                foreach (var leaf in sk.Leaves)
                {
                    // deterministic thinning: same leaves survive every rebuild
                    if (density < 1f && Frac(leaf.index * 0.61803398875f) > density)
                        continue;

                    SourcePart src = Pick(leafParts, leaf.variantRoll);
                    if (src != null)
                        BuildCustomLeafMesh(leaf, src, sizeMul, bendExp, wind,
                                            verts, normals, uvs, colors, leafTris);
                    else
                        BuildLeaf(leaf, s.leaves.shape, sizeMul, bendExp, wind,
                                  verts, normals, uvs, colors, leafTris);
                    leafCount++;
                }
            }

            var mesh = new Mesh { name = meshName };
            mesh.indexFormat = verts.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(barkTris, 0);
            mesh.SetTriangles(leafTris, 1);
            if (s.mesh.generateTangents)
                mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void BuildBranchTube(TreeSkeleton.Branch b, int radial, float uvTiling,
                                    float totalHeight, float bendExp, bool wind,
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
                    c = new Color(
                        Mathf.Pow(Mathf.Clamp01(p.position.y / totalHeight), bendExp), // R main bend
                        b.level == 0 ? 0f : tAlong,                                    // G branch sway
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
                              float bendExp, bool wind,
                              List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                              List<Color> colors, List<int> tris)
        {
            int planes = shape == LeafShape.Quad ? 1 : shape == LeafShape.Cross ? 2 : 3;
            float spin = 180f / planes;
            float size = leaf.size * sizeMul;
            float halfW = size * 0.5f;

            Color c = wind
                ? new Color(Mathf.Pow(leaf.heightT, bendExp), 1f, 1f, leaf.windPhase)
                : Color.clear;

            for (int pIdx = 0; pIdx < planes; pIdx++)
            {
                Quaternion rot = leaf.rotation * Quaternion.AngleAxis(spin * pIdx, Vector3.forward);
                Vector3 right = rot * Vector3.right;
                Vector3 fwd = rot * Vector3.forward;
                Vector3 normal = rot * Vector3.up;

                int i0 = verts.Count;
                // pivot at the attach point, card grows along forward
                verts.Add(leaf.position - right * halfW);
                verts.Add(leaf.position + right * halfW);
                verts.Add(leaf.position + right * halfW + fwd * size);
                verts.Add(leaf.position - right * halfW + fwd * size);
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
            var list = new List<SourcePart>();
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
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var u = new List<Vector2>();
            var t = new List<int>();
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
                                          float totalHeight, float bendExp, bool wind,
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
                    ? new Color(Mathf.Pow(Mathf.Clamp01(p.position.y / totalHeight), bendExp),
                                b.level == 0 ? 0f : t, 0f, b.windPhase)
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
                                        float bendExp, bool wind,
                                        List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
                                        List<Color> colors, List<int> tris)
        {
            int baseIdx = verts.Count;
            float scale = leaf.size * sizeMul;
            Color c = wind
                ? new Color(Mathf.Pow(leaf.heightT, bendExp), 1f, 1f, leaf.windPhase)
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
