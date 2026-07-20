using System.Collections.Generic;
using UnityEngine;

namespace MaterialConverterTool
{
    public enum TextureMapType
    {
        Unknown,
        BaseColor,
        Normal,
        Metallic,
        Roughness,
        Smoothness,
        MetallicSmoothness, // URP packed  (R = metallic, A = smoothness)
        MaskMap,            // HDRP packed (R = metallic, G = AO, B = detail, A = smoothness)
        Occlusion,
        Height,
        Emission
    }

    public class ParsedTexture
    {
        public Texture2D texture;
        public string objectName;
        public TextureMapType mapType;
    }

    /// <summary>
    /// Splits a texture file name into "object name" + "map type" so multiple
    /// selected maps can be grouped per object automatically.
    /// Example: Rock_01_BaseColor, Rock_01_Normal, Rock_01_Roughness_2K
    ///          -> object "Rock_01" with 3 maps.
    /// </summary>
    public static class TextureNameParser
    {
        static readonly Dictionary<string, TextureMapType> Suffixes = new Dictionary<string, TextureMapType>
        {
            // base color
            { "basecolor", TextureMapType.BaseColor },
            { "basecolour", TextureMapType.BaseColor },
            { "albedo", TextureMapType.BaseColor },
            { "diffuse", TextureMapType.BaseColor },
            { "diff", TextureMapType.BaseColor },
            { "basemap", TextureMapType.BaseColor },
            { "color", TextureMapType.BaseColor },
            { "colour", TextureMapType.BaseColor },
            { "col", TextureMapType.BaseColor },
            { "alb", TextureMapType.BaseColor },
            { "d", TextureMapType.BaseColor },
            // normal
            { "normal", TextureMapType.Normal },
            { "normalmap", TextureMapType.Normal },
            { "normalgl", TextureMapType.Normal },
            { "normaldx", TextureMapType.Normal },
            { "nrm", TextureMapType.Normal },
            { "nor", TextureMapType.Normal },
            { "norm", TextureMapType.Normal },
            { "bump", TextureMapType.Normal },
            { "n", TextureMapType.Normal },
            // metallic
            { "metallic", TextureMapType.Metallic },
            { "metalness", TextureMapType.Metallic },
            { "metal", TextureMapType.Metallic },
            { "mtl", TextureMapType.Metallic },
            { "met", TextureMapType.Metallic },
            { "m", TextureMapType.Metallic },
            // roughness
            { "roughness", TextureMapType.Roughness },
            { "rough", TextureMapType.Roughness },
            { "rgh", TextureMapType.Roughness },
            { "r", TextureMapType.Roughness },
            // smoothness
            { "smoothness", TextureMapType.Smoothness },
            { "smooth", TextureMapType.Smoothness },
            { "gloss", TextureMapType.Smoothness },
            { "glossiness", TextureMapType.Smoothness },
            // URP packed
            { "metallicsmoothness", TextureMapType.MetallicSmoothness },
            { "metallicgloss", TextureMapType.MetallicSmoothness },
            { "metalsmooth", TextureMapType.MetallicSmoothness },
            { "ms", TextureMapType.MetallicSmoothness },
            // HDRP packed
            { "maskmap", TextureMapType.MaskMap },
            { "mask", TextureMapType.MaskMap },
            // occlusion
            { "ao", TextureMapType.Occlusion },
            { "occlusion", TextureMapType.Occlusion },
            { "ambientocclusion", TextureMapType.Occlusion },
            { "occ", TextureMapType.Occlusion },
            { "ambocc", TextureMapType.Occlusion },
            // height
            { "height", TextureMapType.Height },
            { "heightmap", TextureMapType.Height },
            { "displacement", TextureMapType.Height },
            { "disp", TextureMapType.Height },
            { "parallax", TextureMapType.Height },
            { "h", TextureMapType.Height },
            // emission
            { "emission", TextureMapType.Emission },
            { "emissive", TextureMapType.Emission },
            { "emit", TextureMapType.Emission },
            { "glow", TextureMapType.Emission },
            { "e", TextureMapType.Emission },
        };

        // trailing tokens that are not part of the object name nor the map type
        static readonly HashSet<string> Noise = new HashSet<string>
        {
            "1k", "2k", "4k", "8k", "512", "1024", "2048", "4096", "8192",
            "ogl", "dx", "gl", "srgb", "linear", "raw", "hdr"
        };

        struct Token
        {
            public string text; // lowercase
            public int start;   // index in the original name
        }

        public static ParsedTexture Parse(Texture2D tex)
        {
            string name = tex.name;
            List<Token> tokens = Tokenize(name);

            int end = tokens.Count;
            while (end > 1 && Noise.Contains(tokens[end - 1].text))
                end--;

            TextureMapType type = TextureMapType.Unknown;
            int consumed = 0;
            if (end >= 2 && Suffixes.TryGetValue(tokens[end - 2].text + tokens[end - 1].text, out TextureMapType two))
            {
                type = two;
                consumed = 2;
            }
            else if (end >= 1 && Suffixes.TryGetValue(tokens[end - 1].text, out TextureMapType one))
            {
                type = one;
                consumed = 1;
            }

            string objName;
            if (consumed > 0 && end - consumed > 0)
                objName = name.Substring(0, tokens[end - consumed].start).TrimEnd('_', '-', '.', ' ');
            else
                objName = name; // no recognizable suffix, or the name IS the suffix

            if (string.IsNullOrEmpty(objName))
                objName = name;

            return new ParsedTexture { texture = tex, objectName = objName, mapType = type };
        }

        static List<Token> Tokenize(string name)
        {
            var tokens = new List<Token>();
            int i = 0;
            while (i < name.Length)
            {
                if (IsSeparator(name[i])) { i++; continue; }
                int start = i;
                while (i < name.Length && !IsSeparator(name[i]))
                    i++;
                tokens.Add(new Token { text = name.Substring(start, i - start).ToLowerInvariant(), start = start });
            }
            if (tokens.Count == 0)
                tokens.Add(new Token { text = name.ToLowerInvariant(), start = 0 });
            return tokens;
        }

        static bool IsSeparator(char c) => c == '_' || c == '-' || c == '.' || c == ' ';
    }
}
