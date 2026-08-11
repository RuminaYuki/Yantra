using UnityEngine;

public class PaperDataInteractable : MonoBehaviour
{
    [SerializeField] private InteractableBase interactable;
    [SerializeField] private PaperDataSO dataSO;

    private void Start()
    {
        if (interactable == null) interactable = GetComponent<InteractableBase>();
    }

    private void OnEnable()
    {
        interactable.OnInteract += ShowExamineUICanvas;
        interactable.OnEndInteract += HideExamineUICanvas;
    }
    private void OnDisable()
    {
        interactable.OnInteract -= ShowExamineUICanvas;
        interactable.OnEndInteract -= HideExamineUICanvas;
    }

    private void ShowExamineUICanvas()
    {
        ExamineUI.Instance.Open(dataSO);
    }
    private void HideExamineUICanvas()
    {
        ExamineUI.Instance.Close();
    }
}
