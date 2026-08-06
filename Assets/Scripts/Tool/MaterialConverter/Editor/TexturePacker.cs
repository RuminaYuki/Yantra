using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialConverterTool
{
    /// <summary>
    /// Packs up to four channel sources into a new PNG texture asset
    /// (linear, sRGB off). Sources of different resolutions are resampled
    /// bilinearly to the largest input size.
    /// </summary>
    public static class TexturePacker
    {
        public struct ChannelSource
        {
            public TexturePixelReader.PixelData data;
            public int channel;    // 0 = R, 1 = G, 2 = B, 3 = A
            public bool invert;    // e.g. roughness -> smoothness
            public float fallback; // used when data == null

            public float Sample(float u, float v)
            {
                float value = data != null ? data.Channel(u, v, channel) : fallback;
                return invert ? 1f - value : value;
            }

            public static ChannelSource Constant(float value)
                => new ChannelSource { fallback = value };

            public static ChannelSource From(TexturePixelReader.PixelData data, int channel,
                                             bool invert = false, float fallback = 0f)
                => new ChannelSource { data = data, channel = channel, invert = invert, fallback = fallback };
        }

        /// <summary>Writes the packed PNG at assetPath (overwrites) and returns the imported texture.</summary>
        public static Texture2D Pack(string assetPath, ChannelSource r, ChannelSource g,
                                     ChannelSource b, ChannelSource a)
        {
            int w = 4, h = 4;
            foreach (var src in new[] { r, g, b, a })
            {
                if (src.data == null)
                    continue;
                w = Mathf.Max(w, src.data.width);
                h = Mathf.Max(h, src.data.height);
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = h <= 1 ? 0f : (float)y / (h - 1);
                for (int x = 0; x < w; x++)
                {
                    float u = w <= 1 ? 0f : (float)x / (w - 1);
                    pixels[y * w + x] = new Color(
                        r.Sample(u, v), g.Sample(u, v), b.Sample(u, v), a.Sample(u, v));
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer.sRGBTexture)
            {
                importer.sRGBTexture = false; // packed data maps are linear
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
