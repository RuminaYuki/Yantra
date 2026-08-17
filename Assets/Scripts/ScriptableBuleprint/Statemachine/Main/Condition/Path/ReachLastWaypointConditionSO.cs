using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "ReachLastWaypointCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Locomotion/Path/Reach Last Waypoint")]
public class ReachLastWaypointConditionSO : StateConditionSO
{
    [SerializeField, Min(0.01f)]
    private float _arrivalDistance = 0.5f;

    public override Condition CreateCondition()
    {
        return new ReachLastWaypointCondition(
            _arrivalDistance);
    }
}

public class ReachLastWaypointCondition : Condition
{
    private readonly float _arrivalDistance;

    private Transform _owner;
    private WaypointPath _waypointPath;

    public ReachLastWaypointCondition(
        float arrivalDistance)
    {
        _arrivalDistance =
            Mathf.Max(0.01f, arrivalDistance);
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner.transform;
        _waypointPath =
            stateMachine.GetComponent<WaypointPath>();

        if (_waypointPath == null)
        {
            Debug.LogError(
                "ReachLastWaypointCondition requires WaypointPath.",
                stateMachine.Owner);
        }
    }

    protected override bool Statement()
    {
        if (_owner == null ||
            _waypointPath == null ||
            _waypointPath.CurrentPoint == null)
        {
            return false;
        }

        if (!_waypointPath.IsAtLastPoint)
            return false;

        Vector3 offset =
            _waypointPath.CurrentPoint.position -
            _owner.position;

        offset.y = 0f;

        return offset.sqrMagnitude <=
               _arrivalDistance * _arrivalDistance;
    }
}