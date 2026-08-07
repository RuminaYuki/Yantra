using System.Collections.Generic;
using UnityEngine;
using YantraRecognition;

/// <summary>
/// ScriptableObject สำหรับเก็บข้อมูล Template ของยันต์
/// </summary>
[CreateAssetMenu(fileName = "NewShapeTemplate", menuName = "Yantra/Shape Template")]
public class ShapeTemplate : ScriptableObject
{
    [Header("Template Info")]
    public string TemplateName = "Unnamed Template";

    [Header("Reference Points (Normalized 0-1)")]
    public List<Vector2> ReferencePoints = new();

    [HideInInspector]
    public YantraGrid CachedGrid;

    /// <summary>
    /// สร้าง CachedGrid จาก ReferencePoints
    /// เรียกครั้งเดียวตอนเริ่มเกม
    /// </summary>
    public void BuildCache(int gridSize = 64)
    {
        CachedGrid = YantraRasterizer.RasterizeReference(
            ReferencePoints,
            gridSize);
    }

#if UNITY_EDITOR

    public void RecordPoints(List<Vector2> normalizedPoints)
    {
        ReferencePoints = new List<Vector2>(normalizedPoints);
        UnityEditor.EditorUtility.SetDirty(this);
    }

#endif
}