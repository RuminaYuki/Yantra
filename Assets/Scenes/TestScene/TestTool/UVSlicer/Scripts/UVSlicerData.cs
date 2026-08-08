using UnityEngine;
using System;

namespace UVSlicer
{
    [Serializable]
    public struct UVSlicerData
    {
        public Vector2Int CellPosition;

        public Vector2Int CellSize;


        public UVSlicerData(
            Vector2Int cellPosition,
            Vector2Int cellSize)
        {
            CellPosition = cellPosition;
            CellSize = cellSize;
        }
    }
}