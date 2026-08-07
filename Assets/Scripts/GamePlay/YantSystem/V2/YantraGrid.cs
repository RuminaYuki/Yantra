using System;

namespace YantraRecognition
{
    /// <summary>
    /// เก็บข้อมูล Grid สำหรับใช้ในการเปรียบเทียบยันต์
    /// แต่ละ Cell มี 2 สถานะ:
    /// true = มีเส้น
    /// false = ไม่มีเส้น
    /// </summary>
    [Serializable]
    public class YantraGrid
    {
        public int Width { get; }
        public int Height { get; }

        /// <summary>
        /// จำนวน Cell ทั้งหมด
        /// </summary>
        public int Count => cells.Length;

        private readonly bool[] cells;

        public YantraGrid(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be greater than 0.");

            if (height <= 0)
                throw new ArgumentException("Height must be greater than 0.");

            Width = width;
            Height = height;

            cells = new bool[width * height];
        }

        /// <summary>
        /// คืนค่า Cell ที่ตำแหน่ง x,y
        /// </summary>
        public bool Get(int x, int y)
        {
            if (!IsInside(x, y))
                return false;

            return cells[y * Width + x];
        }

        /// <summary>
        /// กำหนดค่า Cell
        /// </summary>
        public void Set(int x, int y, bool value)
        {
            if (!IsInside(x, y))
                return;

            cells[y * Width + x] = value;
        }

        /// <summary>
        /// ล้าง Grid ทั้งหมด
        /// </summary>
        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
        }

        /// <summary>
        /// ตรวจสอบว่าอยู่ภายใน Grid หรือไม่
        /// </summary>
        public bool IsInside(int x, int y)
        {
            return x >= 0 &&
                   x < Width &&
                   y >= 0 &&
                   y < Height;
        }

        /// <summary>
        /// คืนข้อมูล Grid ดิบ
        /// ใช้เฉพาะตอน Compare หรือ Save
        /// </summary>
        public bool[] GetRawData()
        {
            return cells;
        }
    }
}