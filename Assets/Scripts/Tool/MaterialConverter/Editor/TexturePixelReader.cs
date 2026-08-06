using UnityEditor;
using UnityEngine;

namespace MaterialConverterTool
{
    /// <summary>
    /// Reads pixels from any project texture (PNG, EXR, ...) even when the
    /// asset is not marked Read/Write: import settings are switched to
    /// readable/uncompressed temporarily and restored afterwards, so the
    /// values match the source file (no compression artifacts).
    /// </summary>
    public static class TexturePixelReader
    {
        public class PixelData
        {
            public Color[] pixels;
            public int width;
            public int height;

            public float Channel(float u, float v, int channel)
            {
                float x = u * (width - 1);
                float y = v * (height - 1);
                int x0 = Mathf.Clamp((int)x, 0, width - 1);
                int y0 = Mathf.Clamp((int)y, 0, height - 1);
                int x1 = Mathf.Min(x0 + 1, width - 1);
                int y1 = Mathf.Min(y0 + 1, height - 1);
                float fx = x - x0;
                float fy = y - y0;
                float top = Mathf.Lerp(Get(x0, y0, channel), Get(x1, y0, channel), fx);
                float bottom = Mathf.Lerp(Get(x0, y1, channel), Get(x1, y1, channel), fx);
                return Mathf.Lerp(top, bottom, fy);
            }

            float Get(int x, int y, int channel)
            {
                Color c = pixels[y * width + x];
                switch (channel)
                {
                    case 0: return c.r;
                    case 1: return c.g;
                    case 2: return c.b;
                    default: return c.a;
                }
            }
        }

        public static PixelData Read(Texture2D tex)
        {
            if (tex == null)
                return null;

            string path = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            bool changed = false;
            bool prevReadable = false;
            bool prevCrunch = false;
            TextureImporterCompression prevCompression = TextureImporterCompression.Uncompressed;

            if (importer != null &&
                (!importer.isReadable ||
                 importer.textureCompression != TextureImporterCompression.Uncompressed ||
                 importer.crunchedCompression))
            {
                prevReadable = importer.isReadable;
                prevCompression = importer.textureCompression;
                prevCrunch = importer.crunchedCompression;

                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.crunchedCompression = false;
                importer.SaveAndReimport();
                changed = true;
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            try
            {
                return new PixelData
                {
                    pixels = tex.GetPixels(),
                    width = tex.width,
                    height = tex.height
                };
            }
            finally
            {
                if (changed)
                {
                    importer.isReadable = prevReadable;
                    importer.textureCompression = prevCompression;
                    importer.crunchedCompression = prevCrunch;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
