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
    private void Awake()
    {
        if (objectiveSO == null)
        {
            objive = GetComponent<Objective>();
            objectiveSO = objive.ObjectiveSO;
        }
        if (eventChannel == null ) gameObject.SetActive(false);

        eventChannel.Raised += OnObjtiveCompleted;
    }

    private void OnObjtiveCompleted(string NameObjive)
    {
        if (objectiveSO.ObjectiveName == NameObjive)
        {
            targetObj.transform.position = targetTranform.transform.position;
        }
    }
}
