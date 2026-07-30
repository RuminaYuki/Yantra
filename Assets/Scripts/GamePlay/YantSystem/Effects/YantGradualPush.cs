using UnityEngine;

/// <summary>
/// ดันวัตถุ "ทีละนิดๆ ไปเรื่อยๆ" ตามทิศที่กำหนด เป็นเวลาหนึ่ง (lifeTime)
/// ติดอยู่บนวัตถุที่โดน projectile ชน แล้วลบตัวเองทิ้งเมื่อหมดเวลา
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class YantGradualPush : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _direction;
    private float _accelerationPerSecond;
    private float _remaining;

    /// <summary>เริ่มดัน ถ้ามีตัวเดิมอยู่แล้วให้เรียกซ้ำเพื่อรีเฟรชทิศ/เวลา</summary>
    public void Begin(Vector3 direction, float accelerationPerSecond, float lifeTime)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        _direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : transform.forward;
        _accelerationPerSecond = accelerationPerSecond;
        _remaining = lifeTime;
    }

    private void FixedUpdate()
    {
        if (_remaining <= 0f)
        {
            Destroy(this);
            return;
        }

        _remaining -= Time.fixedDeltaTime;

        if (_rb != null && !_rb.isKinematic)
        {
            // ForceMode.Acceleration → ไม่ขึ้นกับมวล ดันนุ่มๆ สม่ำเสมอทุกเฟรม
            _rb.AddForce(_direction * _accelerationPerSecond, ForceMode.Acceleration);
        }
    }
}
