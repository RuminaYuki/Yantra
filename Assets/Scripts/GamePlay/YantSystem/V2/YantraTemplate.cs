using System;
using UnityEngine;

namespace YantraRecognition
{
    /// <summary>
    /// เก็บข้อมูล Template ของยันต์ 1 แบบ
    /// </summary>
    [Serializable]
    public class YantraTemplate
    {
        [Tooltip("ชื่อของยันต์")]
        public string Name;

        [Tooltip("ประเภทของยันต์")]
        public ShapeCategory ShapeCategory;

        [Tooltip("Grid ที่ใช้เปรียบเทียบ")]
        public YantraGrid Grid;

        public YantraTemplate(string name, ShapeCategory category, YantraGrid grid)
        {
            Name = name;
            ShapeCategory = category;
            Grid = grid;
        }
    }
}