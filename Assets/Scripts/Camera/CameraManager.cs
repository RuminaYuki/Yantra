using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Assign References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private YantraInputObserverSO _inputObserver;

    [Header("Camera Debug")]
    [SerializeField] private bool _isDrawing;

#if Unity_EDITOR
    private void OnValidate()
    {
        Setup();
    }
#endif
    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (!_camera) this.TryGetComponent(out _camera);
    }
}


