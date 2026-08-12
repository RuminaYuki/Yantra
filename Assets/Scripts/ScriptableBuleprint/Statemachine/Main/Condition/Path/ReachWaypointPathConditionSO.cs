using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[CreateAssetMenu(
    fileName = "ReachWaypointPathCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Locomotion/Path/Reach Waypoint Path")]
public class ReachWaypointPathConditionSO : StateConditionSO
{
    [Min(0.01f)] public float ArrivalDistance = 0.5f;
    public override Condition CreateCondition()
    {
        return new ReachWaypointPathCondition(ArrivalDistance);
    }
}
public class ReachWaypointPathCondition : Condition
{
    private Transform _transform;
    private WaypointPath _waypointPath;

    private readonly float _arrivalDistance;
    public ReachWaypointPathCondition(float arrivalDistance)
    {
        _arrivalDistance = arrivalDistance;
    }
    public override void Awake(StateMachine stateMachine)
    {
        _transform = stateMachine.GetComponent<Transform>();
        _waypointPath = stateMachine.GetComponent<WaypointPath>();
    }
    protected override bool Statement()
    {
        if (_transform == null || _waypointPath == null ||
        _waypointPath.CurrentPoint == null)
        {
            Debug.LogError("ReachWaypointPathCondition cannot find Transform, WaypointPath, or CurrentPoint.");
            return false;
        }

        Vector3 offset = _waypointPath.CurrentPoint.position - _transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= _arrivalDistance * _arrivalDistance;
    }
}
