using UnityEngine;

public class KaMoveController3D : MonoBehaviour
{
    private Rigidbody rb;

    [Header("References Transform")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform StartTransform;


    private Transform endTransform;
    private float moveSpeed;
    private bool isMoving;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        endTransform = rb.gameObject.transform;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        MoveToTarget();

        if (Vector3.Distance(rb.position, targetTransform.position) < 0.1f)
        {
            endTransform.position = StartTransform.position;
            isMoving = false;
            Debug.Log("Warp To Start");
        }
    }

    public void StartMove(float speed)
    {
        moveSpeed = speed;
        isMoving = true;
    }

    private void MoveToTarget()
    {
        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            targetTransform.position,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }
}
