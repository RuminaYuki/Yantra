using System.Collections.Generic;
using UnityEngine;

namespace YantraRecognition
{
    /// <summary>
    /// แปลงข้อมูลการวาดให้เป็น Grid
    /// รองรับทั้ง World Space (จากผู้เล่น)
    /// และ Reference Point (จาก Template)
    /// </summary>
    public static class YantraRasterizer
    {
        public static YantraGrid Rasterize(
            List<Vector3> worldPoints,
            Transform drawSurface,
            int gridSize = 64)
        {
            YantraGrid grid = new YantraGrid(gridSize, gridSize);

            if (worldPoints == null || worldPoints.Count < 2)
                return grid;

            List<Vector2> localPoints = new(worldPoints.Count);

            foreach (Vector3 worldPoint in worldPoints)
            {
                Vector3 local = drawSurface.InverseTransformPoint(worldPoint);
                localPoints.Add(new Vector2(local.x, local.y));
            }

            RasterizeInternal(localPoints, grid);

            return grid;
        }

        /// <summary>
        /// ใช้สร้าง Grid ของ Template
        /// </summary>
        public static YantraGrid RasterizeReference(
            List<Vector2> points,
            int gridSize = 64)
        {
            YantraGrid grid = new YantraGrid(gridSize, gridSize);

            if (points == null || points.Count < 2)
                return grid;

            RasterizeInternal(points, grid);

            return grid;
        }

        private static void RasterizeInternal(
            List<Vector2> points,
            YantraGrid grid)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector2 p in points)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            float width = Mathf.Max(maxX - minX, Mathf.Epsilon);
            float height = Mathf.Max(maxY - minY, Mathf.Epsilon);

            Vector2Int previous = Normalize(
                points[0],
                minX,
                minY,
                width,
                height,
                grid.Width);

            for (int i = 1; i < points.Count; i++)
            {
                Vector2Int current = Normalize(
                    points[i],
                    minX,
                    minY,
                    width,
                    height,
                    grid.Width);

                DrawLine(grid, previous, current);

                previous = current;
            }
        }

        private static Vector2Int Normalize(
            Vector2 point,
            float minX,
            float minY,
            float width,
            float height,
            int gridSize)
        {
            float nx = (point.x - minX) / width;
            float ny = (point.y - minY) / height;

            int x = Mathf.Clamp(
                Mathf.RoundToInt(nx * (gridSize - 1)),
                0,
                gridSize - 1);

            int y = Mathf.Clamp(
                Mathf.RoundToInt(ny * (gridSize - 1)),
                0,
                gridSize - 1);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Bresenham Line Algorithm
        /// </summary>
        private static void DrawLine(
            YantraGrid grid,
            Vector2Int start,
            Vector2Int end)
        {
            int x0 = start.x;
            int y0 = start.y;

            int x1 = end.x;
            int y1 = end.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                grid.Set(x0, y0, true);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = err * 2;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}