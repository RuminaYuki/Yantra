using UnityEngine;

[ExecuteAlways]
public class Follow3D : MonoBehaviour
{
    [SerializeField] protected Transform FollowTarget;

    [Header("Position Settings")]
    [SerializeField] protected bool FollowPosition = true;
    [SerializeField] protected bool UsePositionSmoothing = true;
    [Range(0.1f, 100f)]
    [SerializeField] protected float PositionSmooth = 10f;
    [SerializeField] protected Vector3 OffsetPosition = new(0, 0, 0);

    [Header("Rotation Settings")]
    [SerializeField] protected bool FollowRotation = true;
    [SerializeField] protected bool UseRotationSmoothing = true;
    [Range(0.1f, 100f)]
    [SerializeField] protected float RotationSmooth = 10f;
    [SerializeField] protected Vector3 OffsetRotation = new(0, 0, 0);

    protected virtual void LateUpdate()
    {
        if (FollowTarget == null) return;

        if (FollowPosition)
        {
            Vector3 targetPos = FollowTarget.transform.position + OffsetPosition;
            if (UsePositionSmoothing)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, PositionSmooth * Time.deltaTime);
            }
            else
            {
                transform.position = targetPos;
            }
        }

        if (FollowRotation)
        {
            Quaternion targetRot = FollowTarget.transform.rotation * Quaternion.Euler(OffsetRotation);
            if (UseRotationSmoothing)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSmooth * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }
}