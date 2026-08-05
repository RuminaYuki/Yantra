using UnityEngine;

namespace UVSlicer
{
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public sealed class UVSlicerRenderer : MonoBehaviour
    {
        [SerializeField]
        private Texture2D _textureAtlas;


        [SerializeField]
        private Vector2Int _cellSize =
            new Vector2Int(64, 64);


        [SerializeField]
        private Vector2Int _cellPosition;


        private Renderer _meshRenderer;


        private static readonly int UVOffsetProperty =
            Shader.PropertyToID("_UVOffset");


        private static readonly int UVScaleProperty =
            Shader.PropertyToID("_UVScale");



        private void OnEnable()
        {
            CacheRenderer();

            UpdateUV();
        }



        private void CacheRenderer()
        {
            if (_meshRenderer == null)
            {
                _meshRenderer =
                    GetComponent<Renderer>();
            }
        }



        public void SetCellPosition(
            Vector2Int cellPosition)
        {
            _cellPosition = cellPosition;

            UpdateUV();
        }



        public Vector2Int GetCellPosition()
        {
            return _cellPosition;
        }



        public void UpdateUV()
        {
            if (_meshRenderer == null ||
                _textureAtlas == null)
            {
                return;
            }


            MaterialPropertyBlock propertyBlock =
                new MaterialPropertyBlock();


            _meshRenderer.GetPropertyBlock(
                propertyBlock);



            int columnCount =
                _textureAtlas.width /
                _cellSize.x;


            int rowCount =
                _textureAtlas.height /
                _cellSize.y;



            Vector2 uvScale =
                new Vector2(
                    1f / columnCount,
                    1f / rowCount);



            Vector2 uvOffset =
                new Vector2(
                    _cellPosition.x * uvScale.x,
                    _cellPosition.y * uvScale.y);



            propertyBlock.SetVector(
                UVScaleProperty,
                uvScale);


            propertyBlock.SetVector(
                UVOffsetProperty,
                uvOffset);


            _meshRenderer.SetPropertyBlock(
                propertyBlock);
        }
    }
}