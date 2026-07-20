using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialConverterTool
{
    /// <summary>Result summary shown to the user after each operation.</summary>
    public class ConversionReport
    {
        public readonly List<string> created = new List<string>();
        public readonly List<string> warnings = new List<string>();

        public void Show(string title, int objectCount, int inputCount)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Inputs: {inputCount}   Objects detected: {objectCount}");
            sb.AppendLine($"Created / updated: {created.Count}");
            int shown = 0;
            foreach (string c in created)
            {
                if (shown++ >= 10) { sb.AppendLine($"... and {created.Count - 10} more (see Console)"); break; }
                sb.AppendLine("  " + Path.GetFileName(c));
            }
            if (warnings.Count > 0)
            {
                sb.AppendLine($"Warnings: {warnings.Count}");
                shown = 0;
                foreach (string w in warnings)
                {
                    if (shown++ >= 5) { sb.AppendLine("  ... (see Console)"); break; }
                    sb.AppendLine("  " + w);
                }
            }

            var log = new System.Text.StringBuilder($"[Material Converter] {title}\n");
            foreach (string c in created) log.AppendLine("created: " + c);
            foreach (string w in warnings) log.AppendLine("warning: " + w);
            Debug.Log(log.ToString());

            EditorUtility.DisplayDialog("Material Converter - " + title, sb.ToString(), "OK");
        }
    }

    /// <summary>All maps that belong to one object (grouped by file name).</summary>
    public class TextureSet
    {
        public string objectName;
        public string folder;
        public int textureCount;
        public readonly Dictionary<TextureMapType, Texture2D> maps =
            new Dictionary<TextureMapType, Texture2D>();

        public Texture2D Get(TextureMapType type)
            => maps.TryGetValue(type, out Texture2D t) ? t : null;
    }

    public static class MaterialConverterCore
    {
        const string HdrpShaderName = "HDRP/Lit";
        const string UrpShaderName = "Universal Render Pipeline/Lit";

        // ------------------------------------------------------------------
        // Grouping
        // ------------------------------------------------------------------

        /// <summary>Groups selected textures into per-object sets using their names.</summary>
        public static List<TextureSet> BuildSets(IEnumerable<Texture2D> textures, ConversionReport report)
        {
            var byObject = new Dictionary<string, TextureSet>();
            foreach (Texture2D tex in textures)
            {
                ParsedTexture parsed = TextureNameParser.Parse(tex);
                string key = parsed.objectName.ToLowerInvariant();
                if (!byObject.TryGetValue(key, out TextureSet set))
                {
                    set = new TextureSet
                    {
                        objectName = parsed.objectName,
                        folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(tex)).Replace('\\', '/')
                    };
                    byObject.Add(key, set);
                }

                TextureMapType type = parsed.mapType;
                if (type == TextureMapType.Unknown)
                {
                    report.warnings.Add($"'{tex.name}': map type not recognized, treated as Base Color.");
                    type = TextureMapType.BaseColor;
                }
                if (set.maps.ContainsKey(type))
                    report.warnings.Add($"'{tex.name}': duplicate {type} map for '{set.objectName}', ignored.");
                else
                    set.maps.Add(type, tex);
                set.textureCount++;
            }
            return new List<TextureSet>(byObject.Values);
        }

        // ------------------------------------------------------------------
        // Texture packing / unpacking
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds an HDRP Mask Map (R = metallic, G = AO, B = detail, A = smoothness)
        /// from whatever the set contains: metallic / roughness / smoothness / AO /
        /// URP metallic-smoothness / existing mask map.
        /// </summary>
        public static Texture2D CreateHdrpMaskMap(TextureSet set, ConversionReport report)
        {
            Texture2D metallic = set.Get(TextureMapType.Metallic);
            Texture2D rough = set.Get(TextureMapType.Roughness);
            Texture2D smooth = set.Get(TextureMapType.Smoothness);
            Texture2D ao = set.Get(TextureMapType.Occlusion);
            Texture2D urpPacked = set.Get(TextureMapType.MetallicSmoothness);

            if (metallic == null && rough == null && smooth == null && ao == null && urpPacked == null)
            {
                report.warnings.Add($"'{set.objectName}': no metallic/roughness/smoothness/AO source, Mask Map skipped.");
                return null;
            }

            var urpPx = TexturePixelReader.Read(urpPacked);

            TexturePacker.ChannelSource r =
                metallic != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(metallic), 0)
                : urpPx != null ? TexturePacker.ChannelSource.From(urpPx, 0)
                : TexturePacker.ChannelSource.Constant(0f);

            TexturePacker.ChannelSource g =
                ao != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(ao), 0)
                : TexturePacker.ChannelSource.Constant(1f);

            TexturePacker.ChannelSource a =
                smooth != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(smooth), 0)
                : rough != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(rough), 0, invert: true)
                : urpPx != null ? TexturePacker.ChannelSource.From(urpPx, 3)
                : TexturePacker.ChannelSource.Constant(0.5f);

            string path = $"{set.folder}/{set.objectName}_MaskMap.png";
            Texture2D result = TexturePacker.Pack(path, r, g, TexturePacker.ChannelSource.Constant(1f), a);
            report.created.Add(path);
            return result;
        }

        /// <summary>
        /// Builds a URP Metallic-Smoothness map (R = metallic, A = smoothness)
        /// from metallic / roughness / smoothness / existing HDRP mask map.
        /// </summary>
        public static Texture2D CreateUrpMetallicSmoothness(TextureSet set, ConversionReport report)
        {
            Texture2D metallic = set.Get(TextureMapType.Metallic);
            Texture2D rough = set.Get(TextureMapType.Roughness);
            Texture2D smooth = set.Get(TextureMapType.Smoothness);
            Texture2D mask = set.Get(TextureMapType.MaskMap);

            if (metallic == null && rough == null && smooth == null && mask == null)
            {
                report.warnings.Add($"'{set.objectName}': no metallic/roughness/smoothness source, Metallic-Smoothness skipped.");
                return null;
            }

            var maskPx = TexturePixelReader.Read(mask);

            TexturePacker.ChannelSource metalCh =
                metallic != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(metallic), 0)
                : maskPx != null ? TexturePacker.ChannelSource.From(maskPx, 0)
                : TexturePacker.ChannelSource.Constant(0f);

            TexturePacker.ChannelSource a =
                smooth != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(smooth), 0)
                : rough != null ? TexturePacker.ChannelSource.From(TexturePixelReader.Read(rough), 0, invert: true)
                : maskPx != null ? TexturePacker.ChannelSource.From(maskPx, 3)
                : TexturePacker.ChannelSource.Constant(0.5f);

            string path = $"{set.folder}/{set.objectName}_MetallicSmoothness.png";
            Texture2D result = TexturePacker.Pack(path, metalCh, metalCh, metalCh, a);
            report.created.Add(path);
            return result;
        }

        /// <summary>Extracts the AO stored in a Mask Map's G channel into a standalone texture.</summary>
        public static Texture2D ExtractOcclusion(string folder, string objectName,
                                                 TexturePixelReader.PixelData maskPx, ConversionReport report)
        {
            var g = TexturePacker.ChannelSource.From(maskPx, 1);
            string path = $"{folder}/{objectName}_Occlusion.png";
            Texture2D result = TexturePacker.Pack(path, g, g, g, TexturePacker.ChannelSource.Constant(1f));
            report.created.Add(path);
            return result;
        }

        // ------------------------------------------------------------------
        // Material creation (from texture sets)
        // ------------------------------------------------------------------

        public static Shader FindHdrpShader() => Shader.Find(HdrpShaderName);
        public static Shader FindUrpShader() => Shader.Find(UrpShaderName);

        public static Material GetOrCreateMaterial(string path, Shader shader, ConversionReport report)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
                report.created.Add(path);
            }
            else
            {
                mat.shader = shader;
                report.created.Add(path + " (updated)");
            }
            return mat;
        }

        /// <summary>Creates/updates an HDRP/Lit material named after the object.</summary>
        public static void CreateHdrpMaterialFromSet(TextureSet set, ConversionReport report)
        {
            Shader shader = FindHdrpShader();
            if (shader == null)
            {
                report.warnings.Add("HDRP/Lit shader not found - is the HDRP package installed?");
                return;
            }

            Texture2D mask = set.Get(TextureMapType.MaskMap) ?? CreateHdrpMaskMap(set, report);

            Material mat = GetOrCreateMaterial($"{set.folder}/{set.objectName}.mat", shader, report);
            AssignHdrp(mat, set.Get(TextureMapType.BaseColor), set.Get(TextureMapType.Normal),
                       mask, set.Get(TextureMapType.Height), set.Get(TextureMapType.Emission));
        }

        /// <summary>Creates/updates a URP/Lit material named after the object.</summary>
        public static void CreateUrpMaterialFromSet(TextureSet set, ConversionReport report)
        {
            Shader shader = FindUrpShader();
            if (shader == null)
            {
                report.warnings.Add("URP/Lit shader not found - install the Universal RP package first.");
                return;
            }

            Texture2D ms = set.Get(TextureMapType.MetallicSmoothness) ?? CreateUrpMetallicSmoothness(set, report);

            Texture2D occlusion = set.Get(TextureMapType.Occlusion);
            Texture2D mask = set.Get(TextureMapType.MaskMap);
            if (occlusion == null && mask != null)
                occlusion = ExtractOcclusion(set.folder, set.objectName, TexturePixelReader.Read(mask), report);

            Material mat = GetOrCreateMaterial($"{set.folder}/{set.objectName}.mat", shader, report);
            AssignUrp(mat, set.Get(TextureMapType.BaseColor), set.Get(TextureMapType.Normal),
                      ms, occlusion, set.Get(TextureMapType.Height), set.Get(TextureMapType.Emission));
        }

        // ------------------------------------------------------------------
        // Material conversion (existing materials)
        // ------------------------------------------------------------------

        /// <summary>HDRP/Lit material -> new "<name>_URP.mat" URP/Lit material.</summary>
        public static void ConvertMaterialHdrpToUrp(Material src, ConversionReport report)
        {
            Shader urp = FindUrpShader();
            if (urp == null)
            {
                report.warnings.Add("URP/Lit shader not found - install the Universal RP package first.");
                return;
            }

            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(src)).Replace('\\', '/');
            Material dst = GetOrCreateMaterial($"{folder}/{src.name}_URP.mat", urp, report);

            var mask = GetTex(src, "_MaskMap") as Texture2D;
            Texture2D ms = null, occlusion = null;
            if (mask != null)
            {
                ParsedTexture parsed = TextureNameParser.Parse(mask);
                string maskFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mask)).Replace('\\', '/');
                var maskPx = TexturePixelReader.Read(mask);
                var r = TexturePacker.ChannelSource.From(maskPx, 0);
                var a = TexturePacker.ChannelSource.From(maskPx, 3);
                string msPath = $"{maskFolder}/{parsed.objectName}_MetallicSmoothness.png";
                ms = TexturePacker.Pack(msPath, r, r, r, a);
                report.created.Add(msPath);
                occlusion = ExtractOcclusion(maskFolder, parsed.objectName, maskPx, report);
            }

            AssignUrp(dst,
                GetTex(src, "_BaseColorMap") as Texture2D,
                GetTex(src, "_NormalMap") as Texture2D,
                ms, occlusion,
                GetTex(src, "_HeightMap") as Texture2D,
                GetTex(src, "_EmissiveColorMap") as Texture2D);

            dst.SetColor("_BaseColor", GetColor(src, "_BaseColor", Color.white));
            if (ms == null)
            {
                dst.SetFloat("_Metallic", GetFloat(src, "_Metallic", 0f));
                dst.SetFloat("_Smoothness", GetFloat(src, "_Smoothness", 0.5f));
            }
            if (dst.HasProperty("_BumpScale"))
                dst.SetFloat("_BumpScale", GetFloat(src, "_NormalScale", 1f));
            if (dst.HasProperty("_EmissionColor"))
                dst.SetColor("_EmissionColor", GetColor(src, "_EmissiveColor", Color.black));
            dst.SetTextureScale("_BaseMap", src.HasProperty("_BaseColorMap") ? src.GetTextureScale("_BaseColorMap") : Vector2.one);
            dst.SetTextureOffset("_BaseMap", src.HasProperty("_BaseColorMap") ? src.GetTextureOffset("_BaseColorMap") : Vector2.zero);
            dst.enableInstancing = src.enableInstancing;
            EditorUtility.SetDirty(dst);
        }

        /// <summary>URP/Lit material -> new "<name>_HDRP.mat" HDRP/Lit material.</summary>
        public static void ConvertMaterialUrpToHdrp(Material src, ConversionReport report)
        {
            Shader hdrp = FindHdrpShader();
            if (hdrp == null)
            {
                report.warnings.Add("HDRP/Lit shader not found - is the HDRP package installed?");
                return;
            }

            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(src)).Replace('\\', '/');
            Material dst = GetOrCreateMaterial($"{folder}/{src.name}_HDRP.mat", hdrp, report);

            var mg = GetTex(src, "_MetallicGlossMap") as Texture2D;
            var occlusion = GetTex(src, "_OcclusionMap") as Texture2D;
            Texture2D mask = null;
            if (mg != null || occlusion != null)
            {
                Texture2D nameSource = mg != null ? mg : occlusion;
                ParsedTexture parsed = TextureNameParser.Parse(nameSource);
                string outFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(nameSource)).Replace('\\', '/');

                var mgPx = TexturePixelReader.Read(mg);
                var occPx = TexturePixelReader.Read(occlusion);
                var r = mgPx != null ? TexturePacker.ChannelSource.From(mgPx, 0)
                                     : TexturePacker.ChannelSource.Constant(GetFloat(src, "_Metallic", 0f));
                var g = occPx != null ? TexturePacker.ChannelSource.From(occPx, 1) // URP samples AO from G
                                      : TexturePacker.ChannelSource.Constant(1f);
                var a = mgPx != null ? TexturePacker.ChannelSource.From(mgPx, 3)
                                     : TexturePacker.ChannelSource.Constant(GetFloat(src, "_Smoothness", 0.5f));
                string maskPath = $"{outFolder}/{parsed.objectName}_MaskMap.png";
                mask = TexturePacker.Pack(maskPath, r, g, TexturePacker.ChannelSource.Constant(1f), a);
                report.created.Add(maskPath);
            }

            AssignHdrp(dst,
                GetTex(src, "_BaseMap") as Texture2D,
                GetTex(src, "_BumpMap") as Texture2D,
                mask,
                GetTex(src, "_ParallaxMap") as Texture2D,
                GetTex(src, "_EmissionMap") as Texture2D);

            dst.SetColor("_BaseColor", GetColor(src, "_BaseColor", Color.white));
            if (mask == null)
            {
                dst.SetFloat("_Metallic", GetFloat(src, "_Metallic", 0f));
                dst.SetFloat("_Smoothness", GetFloat(src, "_Smoothness", 0.5f));
            }
            if (dst.HasProperty("_NormalScale"))
                dst.SetFloat("_NormalScale", GetFloat(src, "_BumpScale", 1f));
            if (dst.HasProperty("_EmissiveColor") && src.IsKeywordEnabled("_EMISSION"))
                dst.SetColor("_EmissiveColor", GetColor(src, "_EmissionColor", Color.black));
            dst.SetTextureScale("_BaseColorMap", src.HasProperty("_BaseMap") ? src.GetTextureScale("_BaseMap") : Vector2.one);
            dst.SetTextureOffset("_BaseColorMap", src.HasProperty("_BaseMap") ? src.GetTextureOffset("_BaseMap") : Vector2.zero);
            dst.enableInstancing = src.enableInstancing;
            UnityEngine.Rendering.HighDefinition.HDMaterial.ValidateMaterial(dst);
            EditorUtility.SetDirty(dst);
        }

        // ------------------------------------------------------------------
        // Property assignment helpers
        // ------------------------------------------------------------------

        static void AssignHdrp(Material mat, Texture2D baseColor, Texture2D normal,
                               Texture2D mask, Texture2D height, Texture2D emission)
        {
            if (baseColor != null) mat.SetTexture("_BaseColorMap", baseColor);
            if (normal != null)
            {
                EnsureNormalImportSettings(normal);
                mat.SetTexture("_NormalMap", normal);
            }
            if (mask != null) mat.SetTexture("_MaskMap", mask);
            if (height != null) mat.SetTexture("_HeightMap", height);
            if (emission != null)
            {
                mat.SetTexture("_EmissiveColorMap", emission);
                mat.SetColor("_EmissiveColor", Color.white);
            }
            UnityEngine.Rendering.HighDefinition.HDMaterial.ValidateMaterial(mat);
            EditorUtility.SetDirty(mat);
        }

        static void AssignUrp(Material mat, Texture2D baseColor, Texture2D normal,
                              Texture2D metallicSmoothness, Texture2D occlusion,
                              Texture2D height, Texture2D emission)
        {
            if (baseColor != null) mat.SetTexture("_BaseMap", baseColor);
            if (normal != null)
            {
                EnsureNormalImportSettings(normal);
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (metallicSmoothness != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicSmoothness);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetFloat("_Smoothness", 1f); // let the map's alpha drive smoothness
            }
            if (occlusion != null)
            {
                mat.SetTexture("_OcclusionMap", occlusion);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }
            if (height != null)
            {
                mat.SetTexture("_ParallaxMap", height);
                mat.EnableKeyword("_PARALLAXMAP");
            }
            if (emission != null)
            {
                mat.SetTexture("_EmissionMap", emission);
                mat.SetColor("_EmissionColor", Color.white);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            EditorUtility.SetDirty(mat);
        }

        static void EnsureNormalImportSettings(Texture2D tex)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        static Texture GetTex(Material m, string prop)
            => m.HasProperty(prop) ? m.GetTexture(prop) : null;

        static float GetFloat(Material m, string prop, float fallback)
            => m.HasProperty(prop) ? m.GetFloat(prop) : fallback;

        static Color GetColor(Material m, string prop, Color fallback)
            => m.HasProperty(prop) ? m.GetColor(prop) : fallback;
    }
}
