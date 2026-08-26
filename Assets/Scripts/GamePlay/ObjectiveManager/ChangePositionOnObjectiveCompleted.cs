using UnityEngine;

[RequireComponent(typeof(Objective))]
public class ChangePositionOnObjectiveCompleted : MonoBehaviour
{
    [Header("References")]
    [Tooltip("If null, will use objectiveSO in Objective")]
    [SerializeField] private ObjectiveSO objectiveSO;
    private Objective objive;
    [SerializeField] private StringEventChannelSO eventChannel;

    [Header("Target")]
    [SerializeField] private GameObject targetObj;
    [SerializeField] private Transform targetTranform;
    private bool isSubscribed;

    private void Awake()
    {
        if (objectiveSO == null)
        {
            objive = GetComponent<Objective>();
            objectiveSO = objive != null ? objive.ObjectiveSO : null;
        }

        if (eventChannel == null)
        {
            Debug.LogWarning($"{nameof(ChangePositionOnObjectiveCompleted)} needs an event channel.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (eventChannel == null || isSubscribed)
            return;

        eventChannel.Raised += OnObjectiveCompleted;
        isSubscribed = true;
    }

    private void OnDisable()
    {
        if (eventChannel == null || !isSubscribed)
            return;

        eventChannel.Raised -= OnObjectiveCompleted;
        isSubscribed = false;
    }

    private void OnObjectiveCompleted(string objectiveName)
    {
        if (objectiveSO == null || targetObj == null || targetTranform == null)
            return;

        if (objectiveSO.ObjectiveName == objectiveName)
        {
            targetObj.transform.position = targetTranform.position;
        }
    }
}
