using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastCheckBookTab : MonoBehaviour
{
    [SerializeField] private YantraInputObserverSO inputObserver;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (inputObserver != null) inputObserver.OnLeftClickChannel += HandlePressLeftClickInput;
    }

    private void HandlePressLeftClickInput(Vector2 position, InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BookTab bookTab = hit.collider.GetComponent<BookTab>();
            if (bookTab != null)
            {
                bookTab.OnClick();
            }
        }
    }
}
