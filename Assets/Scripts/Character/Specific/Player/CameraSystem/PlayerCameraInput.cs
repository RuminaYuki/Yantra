using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraInput : MonoBehaviour
{
    [Tooltip("ลากกล้อง PlayerCameraController มาใส่ช่องนี้ ถ้าเว้นว่างไว้ ระบบจะหาจาก Tag 'MainCamera' ให้อัตโนมัติ")]
    public PlayerCameraController cameraController;

    private void Start()
    {
        // ระบบกันเหนียว: ถ้าลืมลากใส่ใน Inspector ให้หาอัตโนมัติ
        if (cameraController == null)
        {
            // Camera.main คือคำสั่งลัดของ Unity ในการหา Object ที่มี Tag ว่า "MainCamera"
            if (Camera.main != null)
            {
                cameraController = Camera.main.GetComponent<PlayerCameraController>();
            }

            // แจ้งเตือนใน Console เผื่อว่าหาไม่เจอจริงๆ
            if (cameraController == null)
            {
                Debug.LogWarning("PlayerCameraInput: หา PlayerCameraController ไม่เจอ กรุณาเช็คว่ากล้องหลักตั้ง Tag เป็น MainCamera หรือยัง");
            }
        }
    }

    private void Update()
    {
        if (cameraController != null && Mouse.current != null)
        {
            // อ่านค่าเมาส์ แล้วส่งคำสั่งไปให้กล้องขยับ
            cameraController.FeedLookInput(Mouse.current.delta.ReadValue());
        }
    }
}