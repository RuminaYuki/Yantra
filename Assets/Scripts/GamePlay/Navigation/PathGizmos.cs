using UnityEngine;

public class PathGizmos : MonoBehaviour
{
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private float pointRadius = 0.15f;
    [SerializeField] private float arrowSize = 0.4f;

    private void OnDrawGizmos()
    {
        Gizmos.color = pathColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform point = transform.GetChild(i);

            Gizmos.DrawSphere(point.position, pointRadius);

            if (i >= transform.childCount - 1)
                continue;

            Transform nextPoint = transform.GetChild(i + 1);

            Gizmos.DrawLine(point.position, nextPoint.position);
            DrawArrow(point.position, nextPoint.position);
        }
    }

    private void DrawArrow(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        direction.Normalize();
        Vector3 arrowPosition = Vector3.Lerp(start, end, 0.6f);
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        Vector3 right = lookRotation *
                        Quaternion.Euler(0f, 160f, 0f) *
                        Vector3.forward;

        Vector3 left = lookRotation *
                       Quaternion.Euler(0f, 200f, 0f) *
                       Vector3.forward;

        Gizmos.DrawLine(
            arrowPosition,
            arrowPosition + right * arrowSize);

        Gizmos.DrawLine(
            arrowPosition,
            arrowPosition + left * arrowSize);
    }
}
