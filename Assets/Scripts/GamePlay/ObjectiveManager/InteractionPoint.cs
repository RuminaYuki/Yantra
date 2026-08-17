using UnityEngine;

public class InteractionPoint : MonoBehaviour, IObjectiveTarget
{
    private Iinteractable interactable;
    [Tooltip("For debug")]
    [SerializeField] private string pointName = "Interaction Point";
    private Objective objective;
    private bool isCompleted = false;

    public bool IsComplete => isCompleted;

    private void Awake()
    {
        interactable = GetComponent<Iinteractable>();

        if (interactable != null) interactable.OnInteract += TyInteract;
    }

    public void Setup(Objective obj)
    {
        objective = obj;
    }

    private void TyInteract(GameObject rootplayer)
    {
        Complete();
    }

    public void Complete()
    {
        if (isCompleted)
            return;

        isCompleted = true;
        Debug.Log($"✅ Completed: {pointName}");
        
        // Notify objective
        if (objective != null)
        {
            objective.OnTargetCompleted();
        }
        
        this.gameObject.SetActive(false);
        // Visual feedback (optional)
        //GetComponent<Renderer>().material.color = Color.green;
    }
}
