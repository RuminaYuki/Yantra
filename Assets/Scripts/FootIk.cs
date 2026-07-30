using UnityEngine;

public class FootIk : MonoBehaviour
{
    private Animator animator;

    [Header("IK Settings")]
    [Range(0, 1)] public float ikWeight = 1.0f; // น้ำหนักการดึง IK (1 คือแนบพื้นเต็มที่)
    public LayerMask groundLayer;               // เลเยอร์ของพื้น/บันได
    public float raycastDistance = 1.0f;        // ระยะยิง Raycast ลงมาจากเท้า
    public float footOffsetY = 0.1f;            // ค่า Offset ชดเชยความหนาของพื้นรองเท้า

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Unity จะเรียกใช้ Event นี้อัตโนมัติเมื่อติ๊ก IK Pass ใน Animator
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // --- เท้าซ้าย ---
        SetFootIKPosition(AvatarIKGoal.LeftFoot);

        // --- เท้าขวา ---
        SetFootIKPosition(AvatarIKGoal.RightFoot);
    }

    private void SetFootIKPosition(AvatarIKGoal foot)
    {
        // 1. กำหนดน้ำหนักการทำงานของ IK ให้เท้าชิ้นนั้น
        animator.SetIKPositionWeight(foot, ikWeight);
        animator.SetIKRotationWeight(foot, ikWeight);

        // 2. ดึงตำแหน่งเท้าปัจจุบันจาก Animation
        Vector3 footPos = animator.GetIKPosition(foot);

        // 3. ยิง Raycast จากตำแหน่งเหนือเท้าเล็กน้อยลงมาที่พื้น
        RaycastHit hit;
        Ray ray = new Ray(footPos + Vector3.up * 0.5f, Vector3.down);

        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            // ปรับตำแหน่ง Y ของเท้าให้แตะจุดที่ Raycast ชนพื้นพอดี + ค่า Offset รองเท้า
            Vector3 targetFootPos = hit.point;
            targetFootPos.y += footOffsetY;

            // สั่งให้ IK ดึงเท้าไปที่ตำแหน่งใหม่
            animator.SetIKPosition(foot, targetFootPos);

            // (Optional) หมุนฝ่าเท้าให้เอียงราบไปตามความชันของพื้น/ขั้นบันได
            Quaternion targetFootRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
            animator.SetIKRotation(foot, targetFootRot);
        }
    }
}
