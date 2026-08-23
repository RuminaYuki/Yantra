using UnityEditor;
using UnityEngine;

public class TerrainHeightOffsetTool : ScriptableWizard
{
    [Tooltip("ตำแหน่ง Y เดิมของ Terrain ก่อนทำ Height Offset")]
    public float baseTerrainY = 0f;

    [Tooltip("พื้นที่สำรองด้านล่างสำหรับการขุด Terrain")]
    public float offsetMeters = 50f;

    [MenuItem("Tools/Terrain/Height Offset")]
    private static void Open()
    {
        DisplayWizard<TerrainHeightOffsetTool>(
            "Terrain Height Offset",
            "Apply"
        );
    }

    private void OnWizardCreate()
    {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogWarning("Please select a Terrain first.");
            return;
        }

        TerrainData data = terrain.terrainData;

        // ---------------------------------
        // หา Offset ที่ Terrain มีอยู่ตอนนี้
        // ---------------------------------

        float currentOffset =
            baseTerrainY - terrain.transform.position.y;

        // เช่น
        // Base Y = 100
        // Transform Y = 50
        // Current Offset = 50

        float deltaOffset =
            offsetMeters - currentOffset;

        if (Mathf.Approximately(deltaOffset, 0f))
        {
            Debug.Log("Terrain already has this offset.");
            return;
        }

        // ---------------------------------
        // อ่าน Heightmap
        // ---------------------------------

        int resolution = data.heightmapResolution;

        float[,] heights = data.GetHeights(
            0,
            0,
            resolution,
            resolution
        );

        float normalizedDelta =
            deltaOffset / data.size.y;

        // ---------------------------------
        // Undo
        // ---------------------------------

        Undo.RegisterCompleteObjectUndo(
            data,
            "Terrain Height Offset"
        );

        Undo.RecordObject(
            terrain.transform,
            "Terrain Height Offset"
        );

        // ---------------------------------
        // Offset Heightmap
        // ---------------------------------

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                heights[y, x] = Mathf.Clamp01(
                    heights[y, x] + normalizedDelta
                );
            }
        }

        data.SetHeights(0, 0, heights);

        // ---------------------------------
        // ตั้ง Transform จากค่าที่ต้องการ
        // ---------------------------------

        Vector3 position = terrain.transform.position;

        position.y = baseTerrainY - offsetMeters;

        terrain.transform.position = position;

        // ---------------------------------

        EditorUtility.SetDirty(data);
        EditorUtility.SetDirty(terrain.transform);

        Debug.Log(
            $"Terrain Base Y: {baseTerrainY} | " +
            $"Offset: {offsetMeters}m | " +
            $"Transform Y: {terrain.transform.position.y}"
        );
    }
}