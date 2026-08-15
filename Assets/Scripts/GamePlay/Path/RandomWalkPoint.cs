using System.Collections.Generic;
using UnityEngine;

public class RandomWalkPoint : MonoBehaviour
{
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _bodyRadius = 0.4f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private int _directionCount = 16;

    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private LayerMask _groundLayer;

    public bool TryGetRandomPoint(out Vector3 point)
    {
        List<Vector3> validPoints = new();

        for (int i = 0; i < _directionCount; i++)
        {
            float angle = (360f / _directionCount) * i;

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            float availableDistance = _radius;

            if (Physics.SphereCast(
                transform.position,
                _bodyRadius,
                direction,
                out RaycastHit hit,
                _radius,
                _obstacleLayer))
            {
                availableDistance = hit.distance;
            }

            availableDistance -= _bodyRadius;

            if (availableDistance < _minDistance)
                continue;

            float distance =
                Random.Range(_minDistance, availableDistance);

            Vector3 candidate =
                transform.position + direction * distance;

            if (TryGetGround(candidate, out Vector3 groundPoint))
            {
                validPoints.Add(groundPoint);
            }
        }

        if (validPoints.Count == 0)
        {
            point = default;
            return false;
        }

        point = validPoints[
            Random.Range(0, validPoints.Count)
        ];

        return true;
    }

    private bool TryGetGround(
        Vector3 position,
        out Vector3 groundPoint)
    {
        Vector3 origin = position + Vector3.up * 2f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            4f,
            _groundLayer))
        {
            groundPoint = hit.point;
            return true;
        }

        groundPoint = default;
        return false;
    }
}