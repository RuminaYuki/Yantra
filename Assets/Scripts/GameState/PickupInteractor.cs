using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PickupInteractor : MonoBehaviour
{
    private const string PickupTag = "Pickup";

    [Header("References")]
    [Required]
    [SerializeField] private YantraInputObserverSO _inputObserver;
    [SerializeField] private Camera _camera;

    [Header("Raycast")]
    [SerializeField] private float _interactRange = 3f;
    [SerializeField] private LayerMask _interactLayer = ~0;

    [Header("UI")]
    [Tooltip("Image icon ที่จะแสดงเมื่อเล็งโดน Pickup")]
    [SerializeField] private GameObject _interactIcon;

    private PlayCutsceneOnInteract _currentTarget;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void OnEnable()
    {
        if (_inputObserver != null)
            _inputObserver.OnInteractionChannel += OnInteract;
    }

    private void OnDisable()
    {
        if (_inputObserver != null)
            _inputObserver.OnInteractionChannel -= OnInteract;
    }

    private void Update()
    {
        UpdateAim();
    }

    private void UpdateAim()
    {
        if (_camera == null) return;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactLayer)
            && hit.collider.CompareTag(PickupTag))
        {
            _currentTarget = hit.collider.GetComponent<PlayCutsceneOnInteract>()
                          ?? hit.collider.GetComponentInParent<PlayCutsceneOnInteract>();

            _interactIcon?.SetActive(true);
            return;
        }

        _currentTarget = null;
        _interactIcon?.SetActive(false);
    }

    private void OnInteract()
    {
        if (_currentTarget == null) return;

        // มี cutscene → เล่น cutscene แล้ว event จะยิงเมื่อจบเอง
        _currentTarget.Interact();
    }
}
