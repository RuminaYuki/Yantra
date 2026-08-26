using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CenterRayInteract))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private CenterRayInteract rayInteract;
    [SerializeField] private GameObject rootPlayer;

    [SerializeField] private bool isHoldingInteraction = false;
    private Iinteractable activeInteraction;

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

        inputAction.action.started += HandleInteractInput;
    }

    private void OnDestroy()
    {
        if (inputAction != null)
            inputAction.action.started -= HandleInteractInput;
    }

    private void HandleInteractInput(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (isHoldingInteraction)
        {
            isHoldingInteraction = false;
            rayInteract.SetInteractEnabled(true);

            if (IsAlive(activeInteraction))
            {
                activeInteraction.CancelInteraction(rootPlayer);
            }

            activeInteraction = null;

            return;
        }

        Iinteractable targetInteractable = rayInteract.CurrentInteractable;
        if (!IsAlive(targetInteractable) || !targetInteractable.CanInteract)
            return;

        if (!targetInteractable.HoldInteract)
        {
            targetInteractable.Interact(rootPlayer);
            return;
        }

        if (targetInteractable.Interact(rootPlayer))
        {
            activeInteraction = targetInteractable;
            isHoldingInteraction = true;
            rayInteract.SetInteractEnabled(false);
            rayInteract.StopHighlightingAll();
        }
    }

    private static bool IsAlive(Iinteractable interactable)
    {
        return interactable is MonoBehaviour target && target != null;
    }
}
