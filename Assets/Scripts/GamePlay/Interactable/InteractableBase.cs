using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, Iinteractable
{
    [Header("Highlight and Focus Objects")]
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject focusObject;

    [Header("Interaction Settings")]
    [Tooltip("If true, the player can interact with this object.")]
    [SerializeField]private bool canInteract = true;
    [Tooltip("If true, the highlight will be hidden when canInteract is false.")]
    [SerializeField] private bool hideInteract = false;

    //if hideInteract is true, CanInteract will always return the value of canInteract, otherwise it will return the value of false
    public bool CanInteract => hideInteract ? canInteract : true;


    private void Awake()
    {
        //Disable this script if there is no highlight or focus object assigned
        if (highlightObject == null && focusObject == null)
        {
            this.enabled = false;
            return;
        }
        highlightObject.SetActive(false);
        focusObject.SetActive(false);
    }

    public virtual void Interact()
    {
        if (!canInteract) return;
    }

    public virtual void OnFocus()
    {
        focusObject.SetActive(true);
        highlightObject.SetActive(false);
    }

    public virtual void OnLoseFocus()
    {
        focusObject.SetActive(false);
        highlightObject.SetActive(true);
    }

    public virtual void ShowHighlight()
    {
        highlightObject.SetActive(true);
        focusObject.SetActive(false);
    }

    public virtual void HideHighlight()
    {
        highlightObject.SetActive(false);
        focusObject.SetActive(false);
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}