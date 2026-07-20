using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float detectionRadius = 10f;
    [Range(0, 180)] public float viewAngle = 60f;

    [Header("Cooldown Settings")]
    public float chaseDuration = 5f;
    public float cooldownDuration = 3f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool isOnCooldown = false;
    private Coroutine chaseCoroutine;    // เก็บอ้างอิง Coroutine เพื่อสั่งหยุดได้แม่นยำ

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        // ถ้าติดคูลดาวน์/สตัน หรือกำลังไล่อยู่ จะไม่เริ่มตรวจจับใหม่
        if (isOnCooldown || isChasing || player == null) return;

        if (IsPlayerInFront())
        {
            chaseCoroutine = StartCoroutine(ChasePlayerRoutine());
        }
    }

    // ==========================================
    // ฟังก์ชันสำหรับให้สคริปต์อื่นเรียกใช้ (Public Function)
    // ==========================================
    public void ApplyStunCooldown(float customDuration = -1f)
    {
        // 1. ถ้ากำลังวิ่งไล่อยู่ ให้หยุดการทำงานของ Coroutine ไล่ตามทันที
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            isChasing = false;
        }

        // 2. สั่ง NavMeshAgent ให้หยุดเดินและล้างเส้นทางเดิม
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        // 3. เริ่มนับเวลาคูลดาวน์/สตันใหม่
        // (ถ้าใส่ค่า customDuration มาจะใช้ค่านั้น ถ้าไม่ใส่จะใช้ค่าเริ่มต้น cooldownDuration)
        float finalDuration = (customDuration > 0f) ? customDuration : cooldownDuration;

        // เคลียร์ Coroutine คูลดาวน์เก่า (ถ้ามี) แล้วเริ่มอันใหม่ เพื่อไม่ให้เวลาทับซ้อนกัน
        StopAllCoroutines();
        StartCoroutine(CooldownRoutine(finalDuration));
    }

    IEnumerator CooldownRoutine(float duration)
    {
        isOnCooldown = true;
        isChasing = false;
        Debug.Log($"มอนสเตอร์ติดสตัน/คูลดาวน์ เป็นเวลา {duration} วินาที");

        yield return new WaitForSeconds(duration);

        isOnCooldown = false;
        Debug.Log("หมดระยะคูลดาวน์ มอนสเตอร์กลับมาทำงานปกติ");
    }

    // ==========================================
    // ฟังก์ชันตรวจจับและการวิ่งไล่ตามปกติ
    // ==========================================
    bool IsPlayerInFront()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRadius)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
            if (angle <= viewAngle)
            {
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, detectionRadius))
                {
                    if (hit.transform.CompareTag("Player")) return true;
                }
            }
        }
        return false;
    }

    IEnumerator ChasePlayerRoutine()
    {
        isChasing = true;
        Debug.Log("เจอผู้เล่น! กำลังไล่ตาม...");


        while (player != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
            
            yield return null;
        }

        // เมื่อไล่ตามครบกำหนดเวลา ย้ายไปเข้าสถานะคูลดาวน์ปกติ
        ApplyStunCooldown(cooldownDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle, 0) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRadius);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRadius);
    }
}
