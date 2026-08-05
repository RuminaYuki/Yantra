using UnityEditor;
using UnityEngine;

namespace UVSlicer.Editor
{
    public sealed class UVSlicerWindow : EditorWindow
    {
        private Texture2D _textureAtlas;


        private UVSlicerRenderer _targetRenderer;


        private Vector2Int _cellSize =
            new Vector2Int(64, 64);


        private Vector2Int _selectedCellPosition;



        [MenuItem(
            "Tools/UV Slicer")]
        private static void OpenWindow()
        {
            GetWindow<UVSlicerWindow>(
                "UV Slicer");
        }



        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "UV Slicer",
                EditorStyles.boldLabel);



            _textureAtlas =
                EditorGUILayout.ObjectField(
                    "Texture Atlas",
                    _textureAtlas,
                    typeof(Texture2D),
                    false)
                as Texture2D;



            _targetRenderer =
                EditorGUILayout.ObjectField(
                    "Target Renderer",
                    _targetRenderer,
                    typeof(UVSlicerRenderer),
                    true)
                as UVSlicerRenderer;



            _cellSize =
                EditorGUILayout.Vector2IntField(
                    "Cell Size",
                    _cellSize);



            if (_textureAtlas == null)
            {
                return;
            }



            DrawTextureGrid();



            EditorGUILayout.Space();



            EditorGUILayout.LabelField(
                "Selected Cell : " +
                _selectedCellPosition.x +
                "," +
                _selectedCellPosition.y);



            if (GUILayout.Button("Apply"))
            {
                ApplySelection();
            }
        }



        private void DrawTextureGrid()
        {
            Rect textureRect =
                GUILayoutUtility.GetRect(
                    512,
                    512);



            GUI.DrawTexture(
                textureRect,
                _textureAtlas,
                ScaleMode.ScaleToFit);



            UVSlicerUtility.DrawGrid(
                textureRect,
                _textureAtlas,
                _cellSize,
                _selectedCellPosition);



            HandleTextureClick(
                textureRect);
        }



        private void HandleTextureClick(
            Rect textureRect)
        {
            Event currentEvent =
                Event.current;


            if (currentEvent.type !=
                EventType.MouseDown)
            {
                return;
            }


            if (!textureRect.Contains(
                currentEvent.mousePosition))
            {
                return;
            }



            _selectedCellPosition =
                UVSlicerUtility.GetCellPosition(
                    textureRect,
                    _textureAtlas,
                    _cellSize,
                    currentEvent.mousePosition);



            PreviewSelection();


            Repaint();
        }



        private void PreviewSelection()
        {
            if (_targetRenderer == null)
            {
                return;
            }


            _targetRenderer.SetCellPosition(
                _selectedCellPosition);
        }



        private void ApplySelection()
        {
            if (_targetRenderer == null)
            {
                return;
            }


            Undo.RecordObject(
                _targetRenderer,
                "Apply UV Slice");


            _targetRenderer.SetCellPosition(
                _selectedCellPosition);


            EditorUtility.SetDirty(
                _targetRenderer);
        }
    }
}