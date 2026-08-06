using UnityEngine;
using UnityEngine.AI;

public class UnityNavMeshPathfinder : IPathfinder
{
    private readonly NavMeshPath _path = new NavMeshPath();

    private int _cornerIndex = 1;

    public bool CalculatePath(Vector3 start, Vector3 destination)
    {
        bool success = NavMesh.CalculatePath(
            start,
            destination,
            NavMesh.AllAreas,
            _path);

        _cornerIndex = 1;

        return success &&
               _path.status == NavMeshPathStatus.PathComplete &&
               _path.corners.Length > 1;
    }

    public Vector3 GetDirection(Vector3 currentPosition)
    {
        if (_path.corners == null ||
            _cornerIndex >= _path.corners.Length)
            return Vector3.zero;

        Vector3 targetCorner = _path.corners[_cornerIndex];

        Vector3 direction = targetCorner - currentPosition;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.1f)
        {
            _cornerIndex++;

            if (_cornerIndex >= _path.corners.Length)
                return Vector3.zero;

            targetCorner = _path.corners[_cornerIndex];
            direction = targetCorner - currentPosition;
            direction.y = 0;
        }

        return direction.normalized;
    }

    public bool HasReachedDestination()
    {
        return _cornerIndex >= _path.corners.Length;
    }
}