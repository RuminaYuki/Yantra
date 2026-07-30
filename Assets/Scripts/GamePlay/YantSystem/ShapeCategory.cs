using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// หมวดหมู่ของยันต์เพื่อจัดกลุ่มตัวอย่างหลายๆ แบบ
/// </summary>
[System.Serializable]
public class ShapeCategory
{
    public string CategoryName;
    public List<ShapeTemplate> Templates = new List<ShapeTemplate>();
}
