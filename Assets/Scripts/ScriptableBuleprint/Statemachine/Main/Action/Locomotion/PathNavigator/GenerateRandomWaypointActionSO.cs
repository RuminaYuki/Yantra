using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "GenerateRandomWaypointAction",
    menuName =
        "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Path Navigator/Generate Random Waypoint")]
public class GenerateRandomWaypointActionSO : StateActionSO
{
    public override StateAction CreateAction(
        StateMachine stateMachine)
    {
        return new GenerateRandomWaypointAction();
    }
}

public class GenerateRandomWaypointAction : StateAction
{
    private const int MaxAttempts = 10;

    private RandomWalkPoint _randomWalkPoint;
    private WaypointPath _waypointPath;
    private PathNavigator _pathNavigator;
    private GameObject _owner;

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner;

        _randomWalkPoint =
            stateMachine.GetComponent<RandomWalkPoint>();

        _waypointPath =
            stateMachine.GetComponent<WaypointPath>();

        _pathNavigator =
            stateMachine.GetComponent<PathNavigator>();

        if (_randomWalkPoint == null)
        {
            Debug.LogError(
                "GenerateRandomWaypointAction requires RandomWalkPoint.",
                _owner);
        }

        if (_waypointPath == null)
        {
            Debug.LogError(
                "GenerateRandomWaypointAction requires WaypointPath.",
                _owner);
        }

        if (_pathNavigator == null)
        {
            Debug.LogError(
                "GenerateRandomWaypointAction requires PathNavigator.",
                _owner);
        }
    }

    public override void OnStateEnter()
    {
        if (_randomWalkPoint == null ||
            _waypointPath == null ||
            _pathNavigator == null)
        {
            return;
        }

        Transform currentPoint =
            _waypointPath.CurrentPoint;

        if (currentPoint == null)
        {
            Debug.LogWarning(
                "GenerateRandomWaypointAction cannot find CurrentPoint.",
                _owner);

            return;
        }

        for (int i = 0; i < MaxAttempts; i++)
        {
            if (!_randomWalkPoint.TryGetRandomPoint(
                    out Vector3 randomPosition))
            {
                continue;
            }

            currentPoint.position = randomPosition;

            if (_pathNavigator.TrySetTarget(currentPoint))
                return;
        }

        _pathNavigator.ClearTarget();

        Debug.LogWarning(
            $"GenerateRandomWaypointAction could not find " +
            $"a reachable point after {MaxAttempts} attempts.",
            _owner);
    }

    public override void OnUpdate()
    {
    }
}