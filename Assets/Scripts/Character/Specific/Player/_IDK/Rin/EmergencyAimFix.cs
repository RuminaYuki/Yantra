using UnityEngine;

public class EmergencyAimFix : MonoBehaviour
{
    [Header("ลากกระดูกหน้าอก และ แขนขวา มาใส่")]
    public Transform chestBone;
    public Transform rightArmBone;

    [Header("ลากเป้าสีขาว (AimTarget_Point) มาใส่")]
    public Transform aimTarget;

    [Header("ลากสคริปต์กล้องมาใส่ เพื่อเช็คการเล็ง")]
    public PlayerCameraController cameraController;

    [Header("ปรับแกนถ้ามันหันผิดทิศ (หมุนจนกว่าจะตรงเป้า)")]
    public Vector3 chestOffset = new Vector3(0, -90, -90);
    public Vector3 armOffset = new Vector3(0, -90, -90);

    // === ตัวแปรที่เพิ่มเข้ามาใหม่ เพื่อทำความสมูท ===
    private float _smoothWeight = 0f;

    void LateUpdate()
    {
        // ถ้าไม่มีของครบ ให้ข้ามไปเลย
        if (cameraController == null || aimTarget == null) return;

        // 1. คำนวณน้ำหนักการเล็ง (วิ่งจาก 0 ไป 1 แบบสมูทๆ)
        // เลข 8f คือความไวในการยกแขน ปรับให้สมูทกำลังดีโดยไม่ต้องไปแก้ใน Inspector
        float targetWeight = cameraController.IsGunAiming ? 1f : 0f;
        _smoothWeight = Mathf.Lerp(_smoothWeight, targetWeight, Time.deltaTime * 8f);

        // ถ้าไม่ได้เล็ง และน้ำหนักกลับเป็น 0 แล้ว ให้ปล่อยเป็นหน้าที่ของ Animator ตามปกติ
        if (_smoothWeight < 0.01f) return;

        // 2. จัดการกระดูกหน้าอกแบบสมูท
        if (chestBone != null)
        {
            // จำมุมเดิมของ Animator (ท่ายืน/เดินปกติ) ไว้ก่อน
            Quaternion animRotation = chestBone.rotation;

            // จำลองหันไปหาเป้า เพื่อหา "มุมที่อยากจะเล็ง"
            chestBone.LookAt(aimTarget);
            chestBone.Rotate(chestOffset);
            Quaternion aimRotation = chestBone.rotation;

            // ผสมมุมเดิม กับ มุมเล็ง ตามน้ำหนักความสมูท
            chestBone.rotation = Quaternion.Slerp(animRotation, aimRotation, _smoothWeight);
        }

        // 3. จัดการกระดูกแขนขวาแบบสมูท
        if (rightArmBone != null)
        {
            Quaternion animRotation = rightArmBone.rotation;

            rightArmBone.LookAt(aimTarget);
            rightArmBone.Rotate(armOffset);
            Quaternion aimRotation = rightArmBone.rotation;

            rightArmBone.rotation = Quaternion.Slerp(animRotation, aimRotation, _smoothWeight);
        }
    }
}