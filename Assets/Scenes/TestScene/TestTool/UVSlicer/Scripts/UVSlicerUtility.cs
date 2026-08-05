#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UVSlicer.Editor
{
    public static class UVSlicerUtility
    {
        public static void DrawGrid(
            Rect textureRect,
            Texture2D textureAtlas,
            Vector2Int cellSize,
            Vector2Int selectedCellPosition)
        {
            int columnCount =
                textureAtlas.width /
                cellSize.x;


            int rowCount =
                textureAtlas.height /
                cellSize.y;



            float cellWidth =
                textureRect.width /
                columnCount;


            float cellHeight =
                textureRect.height /
                rowCount;



            Handles.BeginGUI();



            for (int x = 0; x <= columnCount; x++)
            {
                float xPosition =
                    textureRect.x +
                    x * cellWidth;


                Handles.DrawLine(
                    new Vector3(
                        xPosition,
                        textureRect.y),

                    new Vector3(
                        xPosition,
                        textureRect.y +
                        textureRect.height));
            }



            for (int y = 0; y <= rowCount; y++)
            {
                float yPosition =
                    textureRect.y +
                    y * cellHeight;


                Handles.DrawLine(
                    new Vector3(
                        textureRect.x,
                        yPosition),

                    new Vector3(
                        textureRect.x +
                        textureRect.width,
                        yPosition));
            }



            Rect selectedCellRect =
                new Rect(
                    textureRect.x +
                    selectedCellPosition.x *
                    cellWidth,

                    textureRect.y +
                    selectedCellPosition.y *
                    cellHeight,

                    cellWidth,

                    cellHeight);



            Handles.DrawSolidRectangleWithOutline(
                selectedCellRect,
                Color.clear,
                Color.green);



            Handles.EndGUI();
        }



        public static Vector2Int GetCellPosition(
            Rect textureRect,
            Texture2D textureAtlas,
            Vector2Int cellSize,
            Vector2 mousePosition)
        {
            int columnCount =
                textureAtlas.width /
                cellSize.x;


            int rowCount =
                textureAtlas.height /
                cellSize.y;



            float cellWidth =
                textureRect.width /
                columnCount;


            float cellHeight =
                textureRect.height /
                rowCount;



            int columnIndex =
                Mathf.FloorToInt(
                    (mousePosition.x -
                    textureRect.x)
                    /
                    cellWidth);



            int rowIndex =
                Mathf.FloorToInt(
                    (mousePosition.y -
                    textureRect.y)
                    /
                    cellHeight);



            columnIndex =
                Mathf.Clamp(
                    columnIndex,
                    0,
                    columnCount - 1);


            rowIndex =
                Mathf.Clamp(
                    rowIndex,
                    0,
                    rowCount - 1);



            return new Vector2Int(
                columnIndex,
                rowIndex);
        }
    }
}
#endif