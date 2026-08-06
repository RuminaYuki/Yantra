using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraInput : MonoBehaviour
{
    [Tooltip("ลากกล้อง FatalFrameCameraController มาใส่ช่องนี้")]
    public FatalFrameCameraController cameraController;

    private void Update()
    {
        if (cameraController != null && Mouse.current != null)
        {
            // อ่านค่าเมาส์ แล้วส่งคำสั่งไปให้กล้องขยับ 
            cameraController.FeedLookInput(Mouse.current.delta.ReadValue());
        }
    }
}