using UnityEngine;

public class InteractSave : MonoBehaviour
{
    private Iinteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<Iinteractable>();
        if (interactable == null ) gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        interactable.OnInteract += HandleOnInteract;
    }

    private void OnDisable()
    {
        interactable.OnInteract -= HandleOnInteract;
    }

    private void HandleOnInteract(GameObject playerroot)
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.SaveAll();
    }
}
