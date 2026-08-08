using System.Collections.Generic;
using UnityEngine;

namespace YantraRecognition
{
    /// <summary>
    /// เปรียบเทียบ Grid ของผู้เล่นกับ Template ทั้งหมด
    /// </summary>
    public static class YantraMatcher
    {
        /// <summary>
        /// ระยะที่ยอมให้คลาดเคลื่อน (Cell)
        /// </summary>
        public const int Tolerance = 1;

        public static ShapeMatchResult Match(
            YantraGrid playerGrid,
            List<ShapeCategory> categories,
            string fixedTemplateName)
        {
            ShapeMatchResult result = new ShapeMatchResult();

            float bestScore = -1f;

            foreach (ShapeCategory category in categories)
            {
                if (category == null)
                    continue;

                //บังคับให้ใช้ Template ที่กำหนดไว้
                if (!string.IsNullOrEmpty(fixedTemplateName) && category.CategoryName != fixedTemplateName)
                    continue;

                foreach (ShapeTemplate template in category.Templates)
                {
                    if (template == null)
                        continue;

                    if (template.CachedGrid == null)
                        template.BuildCache(playerGrid.Width);

                    float score = Compare(playerGrid, template.CachedGrid);

                    if (score > bestScore)
                    {
                        bestScore = score;

                        result.MatchedCategoryName = category.CategoryName;
                        result.BestMatchTemplate = template;
                        result.SimilarityPercent = score * 100f;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// เปรียบเทียบ Grid สองอัน
        /// </summary>
        private static float Compare(
            YantraGrid player,
            YantraGrid reference)
        {
            int matched = 0;
            int total = 0;

            for (int y = 0; y < reference.Height; y++)
            {
                for (int x = 0; x < reference.Width; x++)
                {
                    if (!reference.Get(x, y))
                        continue;

                    total++;

                    if (HasNearbyPixel(player, x, y))
                        matched++;
                }
            }

            if (total == 0)
                return 0;

            return (float)matched / total;
        }

        /// <summary>
        /// มี Pixel อยู่ใกล้ ๆ หรือไม่
        /// </summary>
        private static bool HasNearbyPixel(
            YantraGrid grid,
            int x,
            int y)
        {
            for (int oy = -Tolerance; oy <= Tolerance; oy++)
            {
                for (int ox = -Tolerance; ox <= Tolerance; ox++)
                {
                    if (grid.Get(x + ox, y + oy))
                        return true;
                }
            }

            return false;
        }
    }
}