using UnityEngine;

[RequireComponent(typeof(PathNavigator))]
public class WaypointPatrolController : MonoBehaviour
{
    [SerializeField] private WaypointPath _waypointPath;
    [SerializeField, Min(0.01f)] private float _arrivalDistance = 0.5f;

    private PathNavigator _pathNavigator;

    private void Awake()
    {
        _pathNavigator = GetComponent<PathNavigator>();
    }

    private void Start()
    {
        if (_waypointPath == null)
        {
            Debug.LogWarning(
                $"{nameof(WaypointPatrolController)} on {name} has no waypoint path assigned.",
                this);
            return;
        }

        SetCurrentWaypointAsTarget();
    }

    private void Update()
    {
        Transform currentPoint = _waypointPath != null
            ? _waypointPath.CurrentPoint
            : null;

        if (currentPoint == null)
            return;

        Vector3 offset = currentPoint.position - transform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude > _arrivalDistance * _arrivalDistance)
            return;

        _waypointPath.MoveToNextPoint();
        SetCurrentWaypointAsTarget();
    }

    private void SetCurrentWaypointAsTarget()
    {
        _pathNavigator.Target = _waypointPath.CurrentPoint;
    }
}
