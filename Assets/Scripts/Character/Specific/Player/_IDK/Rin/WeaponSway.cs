using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ระบบ Procedural Weapon Sway ทำให้ปืนโยกตามการขยับเมาส์
/// เพิ่มความสมจริงในการเล็ง โดยไม่ต้องพึ่งพา Animator
/// </summary>
public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("ความแรงในการโยกของปืน")]
    [SerializeField] private float _swayMultiplier = 2f;
    [Tooltip("ความสมูทในการดึงปืนกลับตำแหน่งเดิม")]
    [SerializeField] private float _smoothness = 8f;
    [Tooltip("ขีดจำกัดการหมุนสูงสุด ป้องกันปืนบิดเกินไป")]
    [SerializeField] private float _maxSwayAngle = 10f;

    private Quaternion _initialRotation;

    private void Start()
    {
        // บันทึก Rotation เริ่มต้นของปืนเมื่อเทียบกับกระดูกแม่ (มือ)
        _initialRotation = transform.localRotation;
    }

    private void Update()
    {
        CalculateSway();
    }

    private void CalculateSway()
    {
        // 1. รับค่าการขยับเมาส์ผ่าน New Input System
        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        // ถ้าคุณใช้ Input แบบเก่า (Input Manager) ให้คอมเมนต์บรรทัดบน แล้วปลดคอมเมนต์บรรทัดล่าง:
        // float mouseX = Input.GetAxisRaw("Mouse X");
        // float mouseY = Input.GetAxisRaw("Mouse Y");
        // Vector2 mouseDelta = new Vector2(mouseX, mouseY);

        // 2. คำนวณแกนการหมุน (ตรงข้ามกับการขยับเมาส์)
        float swayX = -mouseDelta.x * _swayMultiplier * Time.deltaTime;
        float swayY = mouseDelta.y * _swayMultiplier * Time.deltaTime;

        // 3. จำกัดขอบเขตไม่ให้ปืนหมุนทะลุแขน (Clamp)
        swayX = Mathf.Clamp(swayX, -_maxSwayAngle, _maxSwayAngle);
        swayY = Mathf.Clamp(swayY, -_maxSwayAngle, _maxSwayAngle);

        // 4. สร้าง Rotation เป้าหมาย
        // หมุนแกน X (ขึ้น/ลง) ตามเมาส์ Y และ หมุนแกน Y (ซ้าย/ขวา) ตามเมาส์ X
        Quaternion targetRotation = Quaternion.Euler(swayY, swayX, 0f) * _initialRotation;

        // 5. ค่อยๆ หมุน (Slerp) ตำแหน่งปืนปัจจุบันไปหาเป้าหมายเพื่อความสมูท
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, _smoothness * Time.deltaTime);
    }
}