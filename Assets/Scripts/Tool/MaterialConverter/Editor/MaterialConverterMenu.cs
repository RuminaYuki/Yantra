using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MaterialConverterTool
{
    /// <summary>
    /// Right-click context menu (Project window) for the Material Converter tool.
    /// Select any number of textures or materials, then:
    ///
    ///   Assets > Material Converter >
    ///     Create Texture  > 1. Create New Texture On HDRP Format
    ///                       2. Create New Texture On URP Format
    ///     Convert Texture > 3. Convert Texture HDRP To URP Format
    ///                       4. Convert Texture URP To HDRP Format
    ///     Convert Material> 5. Convert Material HDRP To URP Format
    ///                       6. Convert Material URP To HDRP Format
    ///     Create Material > 7. Create And Convert URP Texture To HDRP Material
    ///                       8. Create And Convert HDRP Texture To URP Material
    ///
    /// Textures are grouped per object by name (e.g. Rock_BaseColor + Rock_Normal
    /// + Rock_Roughness -> object "Rock"), so selecting 12 maps of 3 objects
    /// creates 3 correctly-named materials.
    /// </summary>
    public static class MaterialConverterMenu
    {
        const string Root = "Assets/Material Converter/";

        // ------------------------------------------------------------------
        // Category: Create Texture
        // ------------------------------------------------------------------

        [MenuItem(Root + "Create Texture/Create New Texture On HDRP Format", false, 2000)]
        static void CreateHdrpTexture()
        {
            RunOnTextureSets("Create HDRP Texture (Mask Map)",
                (set, report) => MaterialConverterCore.CreateHdrpMaskMap(set, report));
        }

        [MenuItem(Root + "Create Texture/Create New Texture On URP Format", false, 2001)]
        static void CreateUrpTexture()
        {
            RunOnTextureSets("Create URP Texture (Metallic Smoothness)",
                (set, report) => MaterialConverterCore.CreateUrpMetallicSmoothness(set, report));
        }

        // ------------------------------------------------------------------
        // Category: Convert Texture
        // ------------------------------------------------------------------

        [MenuItem(Root + "Convert Texture/Convert Texture HDRP To URP Format", false, 2100)]
        static void ConvertTextureHdrpToUrp()
        {
            RunOnTextureSets("Convert Texture HDRP -> URP", (set, report) =>
            {
                // if the selection has no recognizable Mask Map but only one
                // texture, assume the user selected the mask map itself
                PromoteSingleTexture(set, TextureMapType.MaskMap, report);
                Texture2D mask = set.Get(TextureMapType.MaskMap);
                if (mask == null)
                {
                    report.warnings.Add($"'{set.objectName}': no Mask Map found, skipped.");
                    return;
                }
                var maskPx = TexturePixelReader.Read(mask);
                var r = TexturePacker.ChannelSource.From(maskPx, 0);
                var a = TexturePacker.ChannelSource.From(maskPx, 3);
                string path = $"{set.folder}/{set.objectName}_MetallicSmoothness.png";
                TexturePacker.Pack(path, r, r, r, a);
                report.created.Add(path);
                MaterialConverterCore.ExtractOcclusion(set.folder, set.objectName, maskPx, report);
            });
        }

        [MenuItem(Root + "Convert Texture/Convert Texture URP To HDRP Format", false, 2101)]
        static void ConvertTextureUrpToHdrp()
        {
            RunOnTextureSets("Convert Texture URP -> HDRP", (set, report) =>
            {
                PromoteSingleTexture(set, TextureMapType.MetallicSmoothness, report);
                MaterialConverterCore.CreateHdrpMaskMap(set, report);
            });
        }

        // ------------------------------------------------------------------
        // Category: Convert Material
        // ------------------------------------------------------------------

        [MenuItem(Root + "Convert Material/Convert Material HDRP To URP Format", false, 2200)]
        static void ConvertMaterialHdrpToUrp()
        {
            if (MaterialConverterCore.FindUrpShader() == null)
            {
                EditorUtility.DisplayDialog("Material Converter",
                    "URP/Lit shader not found.\nInstall the Universal RP package (com.unity.render-pipelines.universal) first.",
                    "OK");
                return;
            }
            RunOnMaterials("Convert Material HDRP -> URP",
                (mat, report) => MaterialConverterCore.ConvertMaterialHdrpToUrp(mat, report));
        }

        [MenuItem(Root + "Convert Material/Convert Material URP To HDRP Format", false, 2201)]
        static void ConvertMaterialUrpToHdrp()
        {
            RunOnMaterials("Convert Material URP -> HDRP",
                (mat, report) => MaterialConverterCore.ConvertMaterialUrpToHdrp(mat, report));
        }

        // ------------------------------------------------------------------
        // Category: Create Material
        // ------------------------------------------------------------------

        [MenuItem(Root + "Create Material/Create And Convert URP Texture To HDRP Material", false, 2300)]
        static void CreateHdrpMaterialFromUrpTextures()
        {
            RunOnTextureSets("URP Textures -> HDRP Material",
                (set, report) => MaterialConverterCore.CreateHdrpMaterialFromSet(set, report));
        }

        [MenuItem(Root + "Create Material/Create And Convert HDRP Texture To URP Material", false, 2301)]
        static void CreateUrpMaterialFromHdrpTextures()
        {
            if (MaterialConverterCore.FindUrpShader() == null)
            {
                EditorUtility.DisplayDialog("Material Converter",
                    "URP/Lit shader not found.\nInstall the Universal RP package (com.unity.render-pipelines.universal) first.",
                    "OK");
                return;
            }
            RunOnTextureSets("HDRP Textures -> URP Material",
                (set, report) => MaterialConverterCore.CreateUrpMaterialFromSet(set, report));
        }

        // ------------------------------------------------------------------
        // Validators (menu enabled only for a matching selection)
        // ------------------------------------------------------------------

        [MenuItem(Root + "Create Texture/Create New Texture On HDRP Format", true)]
        [MenuItem(Root + "Create Texture/Create New Texture On URP Format", true)]
        [MenuItem(Root + "Convert Texture/Convert Texture HDRP To URP Format", true)]
        [MenuItem(Root + "Convert Texture/Convert Texture URP To HDRP Format", true)]
        [MenuItem(Root + "Create Material/Create And Convert URP Texture To HDRP Material", true)]
        [MenuItem(Root + "Create Material/Create And Convert HDRP Texture To URP Material", true)]
        static bool HasTextureSelection()
            => Selection.GetFiltered<Texture2D>(SelectionMode.Assets).Length > 0;

        [MenuItem(Root + "Convert Material/Convert Material HDRP To URP Format", true)]
        [MenuItem(Root + "Convert Material/Convert Material URP To HDRP Format", true)]
        static bool HasMaterialSelection()
            => Selection.GetFiltered<Material>(SelectionMode.Assets).Length > 0;

        // ------------------------------------------------------------------
        // Runners
        // ------------------------------------------------------------------

        static void RunOnTextureSets(string title, System.Action<TextureSet, ConversionReport> perSet)
        {
            Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
            if (textures.Length == 0)
                return;

            var report = new ConversionReport();
            List<TextureSet> sets = MaterialConverterCore.BuildSets(textures, report);
            try
            {
                for (int i = 0; i < sets.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Material Converter",
                        $"{title}: {sets[i].objectName}", (float)i / sets.Count);
                    perSet(sets[i], report);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }
            report.Show(title, sets.Count, textures.Length);
        }

        static void RunOnMaterials(string title, System.Action<Material, ConversionReport> perMaterial)
        {
            Material[] materials = Selection.GetFiltered<Material>(SelectionMode.Assets);
            if (materials.Length == 0)
                return;

            var report = new ConversionReport();
            try
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Material Converter",
                        $"{title}: {materials[i].name}", (float)i / materials.Length);
                    perMaterial(materials[i], report);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }
            report.Show(title, materials.Length, materials.Length);
        }

        /// <summary>
        /// If a set contains exactly one texture whose type wasn't recognized
        /// (fell back to BaseColor), assume it is the packed map the user meant.
        /// </summary>
        static void PromoteSingleTexture(TextureSet set, TextureMapType assumeType, ConversionReport report)
        {
            if (set.Get(assumeType) != null || set.maps.Count != 1)
                return;
            Texture2D only = set.Get(TextureMapType.BaseColor);
            if (only == null)
                return;
            set.maps.Remove(TextureMapType.BaseColor);
            set.maps.Add(assumeType, only);
            report.warnings.Add($"'{only.name}': assumed to be a {assumeType} map.");
        }
    }
}
