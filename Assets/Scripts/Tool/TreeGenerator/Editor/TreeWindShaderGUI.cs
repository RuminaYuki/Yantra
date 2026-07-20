using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>
    /// Material Inspector for the tree wind Shader Graphs (bark / leaf).
    /// Adds a "Texture Source" dropdown - HDRP (Mask Map) or URP (Metallic +
    /// Occlusion maps) - and shows only the fields that format actually uses.
    /// The other format's fields are hidden AND unused: the shader graph
    /// branches on the same value through a Shader Feature Enum Keyword, so
    /// the unselected texture is compiled out of that material's variant, not
    /// just visually hidden.
    ///
    /// Assign via Shader Graph "Graph Settings -> Custom Editor GUI":
    ///   TreeTool.EditorTools.TreeWindShaderGUI
    ///
    /// Matches the exact property Reference names actually used on
    /// Tree-HDRP-Lit-PBR.shadergraph (not the names originally proposed in
    /// TreeGenerator_Manual.md section 8 - the graph was already wired with
    /// these before the doc's naming was finalized, so the script was updated
    /// to match the graph instead of the other way round):
    /// _BaseColor, _BaseMap, _NormalMap, _NormalScale, _Tiling, _MaskMap,
    /// _MetallicGlossMap, _AmbientOcclusionMap, and the Enum Keyword whose
    /// Reference is "MAPFORMAT" (entries: HDRP = 0, URP = 1).
    /// Also always shown, format-independent: _MetallicScale,
    /// _SmoothnessMinScale/_SmoothnessMaxScale, _AmbientOcclusionMinScale/
    /// _AmbientOcclusionMaxScale (mirrors HDRP's own "Smoothness/AO Remapping"
    /// controls), _DetailMap, _DetailMask, _DetailTiling, _DetailAlbedoStrength,
    /// _DetailNormalStrength, _DetailSmoothnessStrength, _HeightMap, _HeightScale,
    /// _EmissionMap, _EmissionColor, _EmissionIntensity.
    ///
    /// Surface Options (Surface Type, Rendering Pass, Blending Mode, Sorting
    /// Priority, Alpha Clipping, Double-Sided, Material Type, ...) are drawn by
    /// HDRP's own reusable <see cref="SurfaceOptionUIBlock"/> - same code HDRP's
    /// built-in inspector uses - so those controls are byte-identical to stock
    /// HDRP/Lit rather than hand-rolled. Everything below that is drawn here.
    /// </summary>
    public class TreeWindShaderGUI : ShaderGUI
    {
        const string BaseColor = "_BaseColor";
        const string BaseMap = "_BaseMap";
        const string NormalMap = "_NormalMap";
        const string NormalScale = "_NormalScale";
        const string Tiling = "_Tiling";
        const string MaskMap = "_MaskMap";
        const string MetallicGlossMap = "_MetallicGlossMap";
        const string OcclusionMap = "_AmbientOcclusionMap";
        const string MapFormat = "MAPFORMAT";

        const string DetailMap = "_DetailMap";
        const string DetailMask = "_DetailMask";
        const string DetailTiling = "_DetailTiling";
        const string DetailAlbedoStrength = "_DetailAlbedoStrength";
        const string DetailNormalStrength = "_DetailNormalStrength";
        const string DetailSmoothnessStrength = "_DetailSmoothnessStrength";

        const string MetallicScale = "_MetallicScale";
        const string SmoothnessRemapMin = "_SmoothnessMinScale";
        const string SmoothnessRemapMax = "_SmoothnessMaxScale";
        const string AORemapMin = "_AmbientOcclusionMinScale";
        const string AORemapMax = "_AmbientOcclusionMaxScale";

        const string HeightMap = "_HeightMap";
        const string HeightScale = "_HeightScale";

        const string EmissionMap = "_EmissionMap";
        const string EmissionColor = "_EmissionColor";
        const string EmissionIntensity = "_EmissionIntensity";

        // Shader Graph derives the compiled keyword names from the keyword's
        // Reference field verbatim + "_" + each entry's reference name - since
        // the "Map Format" keyword's Reference is "MAPFORMAT" (no leading
        // underscore), the generated keywords have no leading underscore either.
        const string HdrpKeyword = "MAPFORMAT_HDRP";
        const string UrpKeyword = "MAPFORMAT_URP";

        // Read by SurfaceOptionUIBlock below (same property HDRP/Lit and every
        // HDRP Shader Graph target use) just to check the block applies to
        // this shader at all before adding it to the block list.
        const string SurfaceType = "_SurfaceType";

        static readonly string[] HandledNames =
        {
            BaseColor, BaseMap, NormalMap, NormalScale, Tiling,
            MaskMap, MetallicGlossMap, OcclusionMap, MapFormat,
            MetallicScale, SmoothnessRemapMin, SmoothnessRemapMax, AORemapMin, AORemapMax,
            DetailMap, DetailMask, DetailTiling, DetailAlbedoStrength, DetailNormalStrength, DetailSmoothnessStrength,
            HeightMap, HeightScale,
            EmissionMap, EmissionColor, EmissionIntensity
        };

        // HDRP's own reusable Surface Options block - gives pixel-and-behavior
        // identical Surface Type / Rendering Pass / Blending Mode / Sorting
        // Priority / Alpha Clipping / Double-Sided / Material Type controls
        // (same code HDRP's own HDLitGUI composes), instead of hand-rolling
        // enum/slider widgets and risking a wrong internal value mapping.
        MaterialUIBlockList _uiBlocks;

        // Persists per Inspector instance, same pattern Unity's own foldout
        // header groups use - collapsed sections stay collapsed while you work.
        bool _baseFoldout = true;
        bool _normalFoldout = true;
        bool _textureSourceFoldout = true;
        bool _detailFoldout;
        bool _heightFoldout;
        bool _emissionFoldout;
        bool _otherFoldout = true;

        public override void OnGUI(MaterialEditor editor, MaterialProperty[] props)
        {
            if (FindProperty(SurfaceType, props, false) != null)
            {
                _uiBlocks ??= new MaterialUIBlockList
                {
                    new SurfaceOptionUIBlock(MaterialUIBlock.ExpandableBit.Base)
                };
                _uiBlocks.OnGUI(editor, props);
                EditorGUILayout.Space(4f);
            }

            _baseFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_baseFoldout, "Base");
            if (_baseFoldout)
            {
                DrawIfPresent(editor, props, BaseColor);
                DrawIfPresent(editor, props, BaseMap);
                DrawIfPresent(editor, props, Tiling);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _normalFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_normalFoldout, "Normal");
            if (_normalFoldout)
            {
                DrawIfPresent(editor, props, NormalMap);
                DrawIfPresent(editor, props, NormalScale);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _textureSourceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_textureSourceFoldout, "Texture Source (Metallic / Smoothness / AO)");
            if (_textureSourceFoldout)
            {
                DrawMapFormatSection(editor, props);
                EditorGUILayout.Space(4f);
                DrawIfPresent(editor, props, MetallicScale);
                DrawMinMaxIfPresent(editor, props, "Smoothness Remapping", SmoothnessRemapMin, SmoothnessRemapMax);
                DrawMinMaxIfPresent(editor, props, "Ambient Occlusion Remapping", AORemapMin, AORemapMax);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _detailFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_detailFoldout, "Detail");
            if (_detailFoldout)
            {
                DrawIfPresent(editor, props, DetailMap);
                DrawIfPresent(editor, props, DetailMask);
                DrawIfPresent(editor, props, DetailTiling);
                DrawIfPresent(editor, props, DetailAlbedoStrength);
                DrawIfPresent(editor, props, DetailNormalStrength);
                DrawIfPresent(editor, props, DetailSmoothnessStrength);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _heightFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_heightFoldout, "Height");
            if (_heightFoldout)
            {
                DrawIfPresent(editor, props, HeightMap);
                DrawIfPresent(editor, props, HeightScale);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _emissionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_emissionFoldout, "Emission");
            if (_emissionFoldout)
            {
                DrawIfPresent(editor, props, EmissionMap);
                DrawIfPresent(editor, props, EmissionColor);
                DrawIfPresent(editor, props, EmissionIntensity);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _otherFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_otherFoldout, "Other");
            if (_otherFoldout)
                DrawRemaining(editor, props);
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(8f);
            editor.RenderQueueField();
            editor.EnableInstancingField();
            editor.DoubleSidedGIField();
        }

        static void DrawMapFormatSection(MaterialEditor editor, MaterialProperty[] props)
        {
            MaterialProperty formatProp = FindProperty(MapFormat, props, false);
            if (formatProp == null)
            {
                EditorGUILayout.HelpBox(
                    $"Shader is missing the '{MapFormat}' Enum Keyword property (Blackboard keyword " +
                    "\"Map Format\", Reference field must read exactly \"MAPFORMAT\").",
                    MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            int format = Mathf.RoundToInt(formatProp.floatValue);
            format = EditorGUILayout.Popup("Format", format, new[] { "HDRP (Mask Map)", "URP (Metallic / AO Maps)" });
            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterPropertyChangeUndo("Map Format");
                formatProp.floatValue = format;
            }
            ApplyKeyword(editor.targets, format);

            EditorGUILayout.Space(4f);
            if (format == 0)
            {
                EditorGUILayout.HelpBox("HDRP Mask Map: R = Metallic, G = Occlusion, B = Detail, A = Smoothness", MessageType.None);
                DrawIfPresent(editor, props, MaskMap);
            }
            else
            {
                EditorGUILayout.HelpBox("URP: Metallic Gloss Map (R = Metallic, A = Smoothness) + Occlusion Map", MessageType.None);
                DrawIfPresent(editor, props, MetallicGlossMap);
                DrawIfPresent(editor, props, OcclusionMap);
            }
        }

        static void ApplyKeyword(Object[] materials, int format)
        {
            foreach (Object obj in materials)
            {
                var material = obj as Material;
                if (material == null)
                    continue;
                if (format == 0)
                {
                    material.EnableKeyword(HdrpKeyword);
                    material.DisableKeyword(UrpKeyword);
                }
                else
                {
                    material.DisableKeyword(HdrpKeyword);
                    material.EnableKeyword(UrpKeyword);
                }
            }
        }

        static void DrawIfPresent(MaterialEditor editor, MaterialProperty[] props, string name)
        {
            MaterialProperty p = FindProperty(name, props, false);
            if (p != null)
                editor.ShaderProperty(p, p.displayName);
        }

        /// <summary>Draws a single Min-Max slider for a pair of remap Float properties (mirrors
        /// HDRP's own "Smoothness/AO Remapping" fields), if both exist on the graph.</summary>
        static void DrawMinMaxIfPresent(MaterialEditor editor, MaterialProperty[] props,
                                        string label, string minName, string maxName)
        {
            MaterialProperty min = FindProperty(minName, props, false);
            MaterialProperty max = FindProperty(maxName, props, false);
            if (min == null || max == null)
                return;

            EditorGUI.BeginChangeCheck();
            float lo = min.floatValue;
            float hi = max.floatValue;
            EditorGUILayout.MinMaxSlider(label, ref lo, ref hi, 0f, 1f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(" ", GUILayout.Width(EditorGUIUtility.labelWidth));
                lo = EditorGUILayout.FloatField(lo, GUILayout.Width(50f));
                GUILayout.FlexibleSpace();
                hi = EditorGUILayout.FloatField(hi, GUILayout.Width(50f));
            }
            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterPropertyChangeUndo(label);
                min.floatValue = Mathf.Clamp01(lo);
                max.floatValue = Mathf.Clamp01(hi);
            }
        }

        static void DrawRemaining(MaterialEditor editor, MaterialProperty[] props)
        {
            foreach (MaterialProperty p in props)
            {
#pragma warning disable CS0618 // MaterialProperty.PropFlags still the actual return type of p.flags on this Editor version
                if ((p.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
#pragma warning restore CS0618
                    continue;
                if (System.Array.IndexOf(HandledNames, p.name) >= 0)
                    continue;
                editor.ShaderProperty(p, p.displayName);
            }
        }
    }
}
