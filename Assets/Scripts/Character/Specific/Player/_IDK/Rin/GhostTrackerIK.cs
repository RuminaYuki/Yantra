using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GhostTrackerIK : MonoBehaviour
{
    [Header("เป้าหมายที่ต้องการให้มอง (ผี หรือ ไอเทม)")]
    public Transform targetToLookAt;

    [Header("ข้อจำกัดการมองเห็น (Limits)")]
    [Tooltip("ระยะไกลสุดที่ตัวละครจะเริ่มมองเป้าหมาย")]
    public float maxLookDistance = 10f;

    [Header("ระบบกันมองทะลุกำแพง (Line of Sight)")]
    [Tooltip("เลเยอร์ของกำแพงหรือสิ่งกีดขวาง")]
    public LayerMask obstacleMask;
    [Tooltip("จุดยิงแสงระดับสายตา (แกน Y แนะนำที่ 1.5)")]
    public Vector3 eyeOffset = new Vector3(0, 1.5f, 0);

    [Header("ตั้งค่าความสมูท")]
    [Tooltip("น้ำหนักการมองสูงสุด (1 = มองเต็มที่)")]
    [Range(0f, 1f)] public float maxLookWeight = 1f;
    public float smoothSpeed = 5f; // ความไวในการหันคอ

    private Animator _animator;
    private float _currentWeight = 0f;
    private float _targetWeight = 0f; // เอาไว้เก็บค่าเป้าหมายว่าควรมองหรือไม่

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (targetToLookAt != null)
        {
            // 1. เช็คระยะทาง (Distance) อย่างเดียว
            float distance = Vector3.Distance(transform.position, targetToLookAt.position);

            // 2. เช็คกำแพงบัง (Linecast)
            bool isBlocked = false;
            Vector3 startPos = transform.position + eyeOffset; // ยิงเส้นจากระดับสายตาตัวละคร

            // ถ้าเส้นที่ยิงไปหาผี ไปชนเข้ากับเลเยอร์กำแพง แปลว่าโดนบัง
            if (Physics.Linecast(startPos, targetToLookAt.position, out RaycastHit hit, obstacleMask))
            {
                isBlocked = true;
            }

            // ถ้าอยู่ในระยะ + ไม่มีกำแพงบัง -> สั่งให้มอง! (ตัดเช็คมุมออกไปแล้ว)
            if (distance <= maxLookDistance && !isBlocked)
            {
                _targetWeight = maxLookWeight;
            }
            else
            {
                _targetWeight = 0f; // ถ้าไกลไป หรือมีอะไรบัง ให้เลิกมอง
            }
        }
        else
        {
            _targetWeight = 0f;
        }
    }

    // ฟังก์ชันนี้เป็นระบบของ Unity จะทำงานก็ต่อเมื่อเปิด IK Pass ใน Animator เท่านั้น
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null) return;

        // ค่อยๆ ปรับน้ำหนักการหันคอให้สมูท
        _currentWeight = Mathf.Lerp(_currentWeight, _targetWeight, Time.deltaTime * smoothSpeed);

        if (_currentWeight > 0.01f && targetToLookAt != null)
        {
            // ตั้งค่าน้ำหนัก (น้ำหนักรวม, น้ำหนักตัว, น้ำหนักหัว, น้ำหนักตา, ล็อกไม่ให้คอหัก)
            _animator.SetLookAtWeight(_currentWeight, 0.1f, 0.8f, 1.0f, 0.5f);

            // สั่งให้หน้าหันไปหาตำแหน่งเป้าหมาย
            _animator.SetLookAtPosition(targetToLookAt.position);
        }
        else
        {
            _animator.SetLookAtWeight(_currentWeight);
        }
    }

    // วาดเส้นในหน้า Scene เพื่อให้ตั้งค่าง่ายขึ้น
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxLookDistance); // เส้นบอกระยะการมอง

        if (targetToLookAt != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + eyeOffset, targetToLookAt.position); // เส้นบอก Line of Sight
        }
    }
}