using UnityEngine;

public class TargetPointFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private TransformAnchor anchor;

    [Header("Position")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool offsetFollowsTargetRotation = true;

    public Vector3 Position => transform.position;

    private void LateUpdate()
    {
        // Anchor มีความสำคัญกว่า Target และอาจได้รับค่าหลัง Awake
        if (anchor != null && anchor.Value != null)
        {
            target = anchor.Value;
        }

        if (target == null)
        {
            return;
        }

        transform.position = offsetFollowsTargetRotation
            ? target.TransformPoint(offset)
            : target.position + offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetAnchor(TransformAnchor newAnchor)
    {
        anchor = newAnchor;
    }
}