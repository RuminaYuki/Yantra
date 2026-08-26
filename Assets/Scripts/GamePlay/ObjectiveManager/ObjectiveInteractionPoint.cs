using UnityEngine;

public class ObjectiveInteractionPoint : MonoBehaviour, IObjectiveTarget
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
    }

    private void OnEnable()
    {
        if (interactable != null)
            interactable.OnInteract += TryInteract;
    }

    private void OnDisable()
    {
        if (interactable != null)
            interactable.OnInteract -= TryInteract;
    }

    public void Setup(Objective obj)
    {
        objective = obj;
    }

    private void TryInteract(GameObject rootplayer)
    {
        Complete();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Complete();
        }
    }

    public void Complete()
    {
        if (isCompleted)
        {
            Debug.Log("here");
            return;
        }

        isCompleted = true;
        Debug.Log($"✅ Completed: {pointName}");
        
        // Notify objective
        if (objective != null && objective.gameObject != null)
        {
            objective.OnTargetCompleted();
        }
        
        this.gameObject.SetActive(false);
        // Visual feedback (optional)
        //GetComponent<Renderer>().material.color = Color.green;
    }
}
