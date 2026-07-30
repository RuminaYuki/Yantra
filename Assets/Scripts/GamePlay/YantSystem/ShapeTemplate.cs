using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject สำหรับเก็บข้อมูล Template รูปต้นฉบับ
/// สร้างได้จาก Assets > Create > Yantra > Shape Template
/// </summary>
[CreateAssetMenu(fileName = "NewShapeTemplate", menuName = "Yantra/Shape Template")]
public class ShapeTemplate : ScriptableObject
{
    [Header("Template Info")]
    [Tooltip("ชื่อรูปต้นฉบับนี้ เช่น 'Triangle', 'Circle', 'Star'")]
    public string TemplateName = "Unnamed Template";

    [Header("Reference Points (Normalized 0-1)")]
    [Tooltip(
        "พิกัด 2D ของรูปต้นฉบับ ค่าควรอยู่ใน 0-1 space\n" +
        "หรือจะใช้ RecordFromDrawing() เพื่อ capture จากการวาดจริง"
    )]
    public List<Vector2> ReferencePoints = new List<Vector2>();

    [Header("Sampling")]
    [Tooltip("จำนวน point ที่จะ resample ขณะเทียบ (ยิ่งมากยิ่งละเอียด แต่ช้ากว่า)")]
    public int ResampleCount = 64;

#if UNITY_EDITOR
    /// <summary>
    /// [Editor Only] บันทึก points ที่ได้จากการวาดจริงลง Template นี้
    /// เรียกจาก Custom Editor หรือ Context Menu เพื่อสร้าง Template ใหม่
    /// </summary>
    public void RecordPoints(List<Vector2> normalizedPoints)
    {
        ReferencePoints = new List<Vector2>(normalizedPoints);
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
