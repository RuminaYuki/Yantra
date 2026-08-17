using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [Header("If targetAnchor is set, It will override Target value")]
    [SerializeField] TransformAnchor targetAnchor;
    [SerializeField] Transform target;
    [SerializeField] Transform viewpoint;
    [SerializeField] bool showGizmos = true;

    [Header("DetectRange")]
    [SerializeField] float detectRange;

    [Header("FieldOfView")]
    [SerializeField] float viewAngle = 90f;
    [SerializeField] bool enableFOV = true;
 
    [Header("Raycast")]
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] bool enableRayCast = true;

    public Transform Target => target;

    private Transform Viewpoint => viewpoint != null ? viewpoint : transform;

    void Start()
    {
        if(targetAnchor != null)
        {
            target = targetAnchor.Value;
            if (target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    Debug.LogError("Dont have Player in Scene");
                }   
            }
        }
    }

    public bool DetectRange()
    {
        float distance = (target.position - Viewpoint.position).sqrMagnitude;
        return distance <= detectRange * detectRange;
    }
    public bool FOVAngle()
    {
        Vector3 dirToTarget = (target.position - Viewpoint.position).normalized;
        float angle = Vector3.Angle(Viewpoint.forward, dirToTarget);

        return angle <= viewAngle * 0.5f;
    }
    public bool Raycast()
    {
        Vector3 origin = Viewpoint.position;
        Vector3 dirToTarget = (target.position - origin).normalized;
        float distance = Vector3.Distance(origin, target.position);

        if (Physics.Raycast(origin, dirToTarget, distance, obstacleLayer))
        {
            return false;
        }
        return true;
    }
    public bool CanSeeTarget()
    {
        if (!DetectRange()) return false;
        if (enableFOV && !FOVAngle()) return false;
        if (enableRayCast &&!Raycast()) return false;

        return true;
    }
    private void OnDrawGizmosSelected()
    {
        if(!showGizmos) return;
        if (target == null) return;

        Transform viewOrigin = Viewpoint;

        //DetectRange
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(viewOrigin.position, detectRange);

        //FOV
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * viewOrigin.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * viewOrigin.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(viewOrigin.position, left * detectRange);
        Gizmos.DrawRay(viewOrigin.position, right * detectRange);

        //Raycast
        Gizmos.color = Color.red;
        Gizmos.DrawLine(viewOrigin.position, target.position);
    }
}