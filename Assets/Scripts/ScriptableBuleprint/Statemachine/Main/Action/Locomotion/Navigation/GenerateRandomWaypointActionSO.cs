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
    private int _maxAttemptsPerPoint = 3;

    [SerializeField, Min(0.01f)]
    private float _radius = 5f;

    [SerializeField, Min(0.01f)]
    private float _minDistance = 1.5f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new GenerateRandomWaypointAction(
            _pointCount,
            _maxAttemptsPerPoint,
            _radius,
            _minDistance);
    }
}

public class GenerateRandomWaypointAction : StateAction
{
    private readonly int _pointCount;
    private readonly int _maxAttemptsPerPoint;
    private readonly float _radius;
    private readonly float _minDistance;

    private RandomWalkPoint _randomWalkPoint;
    private WaypointPath _waypointPath;
    private Transform _owner;

    public GenerateRandomWaypointAction(
        int pointCount,
        int maxAttemptsPerPoint,
        float radius,
        float minDistance)
    {
        _pointCount = Mathf.Max(1, pointCount);
        _maxAttemptsPerPoint = Mathf.Max(1, maxAttemptsPerPoint);
        _radius = Mathf.Max(0.01f, radius);
        _minDistance = Mathf.Clamp(minDistance, 0.01f, _radius);
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
            return;

        _waypointPath.ResetToFirstPoint();

        int count = Mathf.Min(_pointCount, _waypointPath.Count);

        Vector3 origin = _owner.position;

        for (int i = 0; i < count; i++)
        {
            if (!TryGeneratePoint(
                    origin,
                    out Vector3 position))
                return;

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
            if (_randomWalkPoint.TryGetRandomPoint(
                    origin,
                    _radius,
                    _minDistance,
                    out point))
            {
                return true;
            }
        }

        point = default;
        return false;
    }
}
