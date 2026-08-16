using UnityEngine;

public enum ObjectiveType
{
    INTERACTION,
    COLLECT,
    ELIMINATE,
    REACH_POINT
}

[CreateAssetMenu(fileName = "New Objective", menuName = "Objective/Objective")]
public class ObjectiveSO : ScriptableObject
{
    [SerializeField] private string objectiveName = "Unnamed Objective";
    [SerializeField] private ObjectiveType type = ObjectiveType.INTERACTION;
    [SerializeField] private int targetCount = 3;
    [SerializeField] private string description = "";
    [SerializeField] private Vector3 endLocation = Vector3.zero;
    [SerializeField] private float endLocationRadius = 2f;

    public string ObjectiveName => objectiveName;
    public ObjectiveType Type => type;
    public int TargetCount => targetCount;
    public string Description => description;
    public Vector3 EndLocation => endLocation;
    public float EndLocationRadius => endLocationRadius;
}
