using UnityEngine;

public class EmergencyAimFix : MonoBehaviour
{
    [Header("ลากกระดูกหน้าอก และ แขนขวา มาใส่")]
    public Transform chestBone;
    public Transform rightArmBone;

    [Header("ลากเป้าสีขาว (AimTarget_Point) มาใส่")]
    public Transform aimTarget;

    [Header("ลากสคริปต์กล้องมาใส่ เพื่อเช็คการเล็ง")]
    public FatalFrameCameraController cameraController;

    [Header("ปรับแกนถ้ามันหันผิดทิศ (หมุนจนกว่าจะตรงเป้า)")]
    public Vector3 chestOffset = new Vector3(0, -90, -90);
    public Vector3 armOffset = new Vector3(0, -90, -90);

    void LateUpdate()
    {
        // ถ้าไม่ได้เล็ง หรือไม่มีเป้า ให้หยุดทำงาน (ปล่อยเล่นอนิเมชั่นปกติ)
        if (cameraController == null || !cameraController.IsGunAiming || aimTarget == null) return;

        // บังคับหน้าอกหันหาเป้าแบบทะลุทะลวง
        if (chestBone != null)
        {
            chestBone.LookAt(aimTarget);
            chestBone.Rotate(chestOffset);
        }

        // บังคับแขนหันหาเป้าแบบทะลุทะลวง
        if (rightArmBone != null)
        {
            rightArmBone.LookAt(aimTarget);
            rightArmBone.Rotate(armOffset);
        }
    }
}