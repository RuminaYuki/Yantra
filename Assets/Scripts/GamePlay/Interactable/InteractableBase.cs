using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, Iinteractable
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject focusObject;

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
}