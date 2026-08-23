using UnityEngine;

public class AddGravityFordummy : MonoBehaviour
{
   [Header("Settings")]
    [SerializeField] private float gravity = -9.81f; // ค่าแรงโน้มถ่วง
    [SerializeField] private CharacterController controller; // อ้างอิง CharacterController (ถ้ามี)

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    private void Start()
    {
        // หากไม่ได้ตั้งค่าไว้ใน Inspector ให้ดึงอัตโนมัติ
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. ตรวจสอบว่าอยู่บนพื้นหรือไม่
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // fallback หากไม่มีจุด groundCheck ให้ใช้ CharacterController
            isGrounded = controller != null && controller.isGrounded;
        }

        // 2. รีเซ็ตแรงตกเมื่ออยู่บนพื้น (ใส่ค่าน้อยๆ ติดลบเพื่อบังคับให้ตัวละครแนบติดพื้นเสมอ)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 3. คำนวณความเร็วตกตามกาลเวลา: v = g * t
        velocity.y += gravity * Time.deltaTime;

        // 4. สั่งเคลื่อนที่
        if (controller != null)
        {
            // ใช้ CharacterController (แนะนำสำหรับเคลื่อนที่ตัวละคร)
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            // กรณีไม่มี CharacterController ปรับตำแหน่งผ่าน Transform โดยตรง
            transform.Translate(velocity * Time.deltaTime, Space.World);
        }
    }

    // วาด Gizmo ช่วยให้เห็นขอบเขตการตรวจจับพื้นในหน้า Scene
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
