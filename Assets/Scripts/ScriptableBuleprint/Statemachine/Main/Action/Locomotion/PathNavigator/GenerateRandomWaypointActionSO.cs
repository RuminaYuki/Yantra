using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "GenerateRandomWaypointAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Path Navigator/Generate Random Waypoints")]
public class GenerateRandomWaypointActionSO : StateActionSO
{
    [SerializeField, Min(1)]
    private int _pointCount = 3;

    [SerializeField, Min(1)]
    private int _maxAttemptsPerPoint = 10;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new GenerateRandomWaypointAction(_pointCount,_maxAttemptsPerPoint);
    }
}

public class GenerateRandomWaypointAction : StateAction
{
    private readonly int _pointCount;
    private readonly int _maxAttemptsPerPoint;

    private RandomWalkPoint _randomWalkPoint;
    private WaypointPath _waypointPath;
    private Transform _owner;

    public GenerateRandomWaypointAction(
        int pointCount,
        int maxAttemptsPerPoint)
    {
        _pointCount = Mathf.Max(1, pointCount);
        _maxAttemptsPerPoint =
            Mathf.Max(1, maxAttemptsPerPoint);
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner.transform;
        _randomWalkPoint = stateMachine.GetComponent<RandomWalkPoint>();
        _waypointPath = stateMachine.GetComponent<WaypointPath>();

        if (_randomWalkPoint == null)
        {
            Debug.LogError(
                "GenerateRandomWaypointAction requires RandomWalkPoint.",
                stateMachine.Owner);
        }

        if (_waypointPath == null)
        {
            Debug.LogError(
                "GenerateRandomWaypointAction requires WaypointPath.",
                stateMachine.Owner);
        }
    }

    public override void OnStateEnter()
    {
        if (_owner == null ||
            _randomWalkPoint == null ||
            _waypointPath == null ||
            _waypointPath.PathRoot == null)
        {
            return;
        }

        int count = Mathf.Min(
            _pointCount,
            _waypointPath.Count);

        if (count < _pointCount)
        {
            Debug.LogWarning(
                $"Path Root requires at least {_pointCount} waypoint children.",
                _owner);
        }

        Vector3 origin = _owner.position;

        for (int i = 0; i < count; i++)
        {
            if (!TryGeneratePoint(origin, out Vector3 position))
            {
                Debug.LogWarning(
                    $"Could not generate waypoint {i + 1}.",
                    _owner);

                return;
            }

            Transform waypoint =
                _waypointPath.PathRoot.GetChild(i);

            waypoint.position = position;
            origin = position;
        }
    }

    public override void OnUpdate()
    {
    }

    private bool TryGeneratePoint(Vector3 origin, out Vector3 point)
    {
        for (int i = 0; i < _maxAttemptsPerPoint; i++)
        {
            if (_randomWalkPoint.TryGetRandomPoint(origin, out point))
            {
                return true;
            }
        }

        point = default;
        return false;
    }
}