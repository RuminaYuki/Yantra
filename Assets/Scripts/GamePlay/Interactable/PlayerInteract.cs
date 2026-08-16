using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CenterRayInteract))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private CenterRayInteract rayInteract;
    [SerializeField] private GameObject rootPlayer;

    [SerializeField] private bool onInteractable = false;
    private Iinteractable activeInteractable;

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
        if (!context.started)
            return;
            
        activeInteractable = rayInteract.CurrentInteractable;
        if (activeInteractable == null )
            return;

        if (!activeInteractable.HoldInteract)
        {
            activeInteractable.Interact(rootPlayer);
            return;
        }

        if (!onInteractable)
        {
            if (activeInteractable != null && activeInteractable.Interact(rootPlayer))
            {
                onInteractable = true;
                rayInteract.SetInteractEnabled(false);
                rayInteract.StopHighlightingAll();
            }
        }
        else
        {
            onInteractable = false;
            rayInteract.SetInteractEnabled(true);

            if (activeInteractable != null)
            {
                activeInteractable.CancelInteraction(rootPlayer);
                activeInteractable = null;
            }
        }
    }
}
