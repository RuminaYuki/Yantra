using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CenterRayInteract))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private CenterRayInteract rayInteract;
    [SerializeField] private GameObject rootPlayer;

    private void Awake()
    {
        rayInteract = GetComponent<CenterRayInteract>();
        if (inputAction == null)
        {
            Debug.LogError("Input Action Reference is not assigned.");
            enabled = false;
            rayInteract.enabled = false;
            return;
        }
        inputAction.action.started += HandleMoveInput;
    }

    private void HandleMoveInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            rayInteract.CurrentInteractable?.Interact(rootPlayer);
        }
    }
}
