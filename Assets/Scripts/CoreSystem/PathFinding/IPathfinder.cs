using UnityEngine;

public interface IPathfinder
{
    bool CalculatePath(Vector3 start, Vector3 destination);
    Vector3 GetDirection(Vector3 currentPosition);
    bool HasReachedDestination();
}