using System.Text;

namespace YantraRecognition
{
    /// <summary>
    /// ใช้สำหรับ Debug YantraGrid
    /// </summary>
    public static class YantraGridDebugger
    {
        /// <summary>
        /// แปลง Grid เป็นข้อความ
        /// █ = มีเส้น
        /// . = ว่าง
        /// </summary>
        public static string ToText(YantraGrid grid)
        {
                StringBuilder sb = new();

                int minX = grid.Width, minY = grid.Height, maxX = -1, maxY = -1;

                for (int y = 0; y < grid.Height; y++)
                {
                    for (int x = 0; x < grid.Width; x++)
                    {
                        if (grid.Get(x, y))
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                if (maxX < 0)
                {
                    sb.AppendLine($"[Empty Grid] Size: {grid.Width}x{grid.Height}");
                    return sb.ToString();
                }

                // provide header with original size and crop bounds
                sb.AppendLine($"Size: {grid.Width}x{grid.Height}  Crop: x={minX}-{maxX} y={minY}-{maxY}");

                for (int y = maxY; y >= minY; y--)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        sb.Append(grid.Get(x, y) ? '#' : '.');
                    }

                    sb.AppendLine();
                }

                return sb.ToString();
        }
    }
}