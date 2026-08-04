using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [SerializeField] Transform target;
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

    void Start()
    {
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

    public bool DetectRange()
    {
        float distance = (target.position - transform.position).sqrMagnitude;
        return distance <= detectRange * detectRange;
    }
    public bool FOVAngle()
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);

        return angle <= viewAngle * 0.5f;
    }
    public bool Raycast()
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        if (Physics.Raycast(transform.position, dirToTarget, distance, obstacleLayer))
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

        //DetectRange
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        //FOV
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, left * detectRange);
        Gizmos.DrawRay(transform.position, right * detectRange);

        //Raycast
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}