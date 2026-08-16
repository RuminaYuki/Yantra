using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// จับการเข้า-ออกจาก collision พื้นฐาน
/// เคลื่อน object ตามตำแหน่ง origin transform + offset
/// เก็บ list ของ IDamageable ที่ยังคงอยู่ใน collision
/// </summary>
public class CollisionDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask _targetMask = ~0;

    [Tooltip("ไม่ตรวจจับ object ที่มี root นี้")]
    [SerializeField] private GameObject _ownerToIgnore;

    [Header("Position Tracking")]
    [Tooltip("Transform ที่ใช้ตำแหน่งอ้างอิง")]
    [SerializeField] private Transform _originTransform;

    [Tooltip("เลื่อนตำแหน่งออกจาก origin")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Tooltip("หมุนเพิ่มเติม")]
    [SerializeField] private Quaternion _rotationOffset = Quaternion.identity;

    // เก็บ targets ที่ยังอยู่ใน collision
    private HashSet<IDamageable> _activeTargets = new HashSet<IDamageable>();
    private HashSet<Collider> _activeColliders = new HashSet<Collider>();

    private void Awake()
    {
        if (_originTransform == null)
        {
            _originTransform = Camera.main != null
                ? Camera.main.transform
                : null;
        }
    }

    private void Update()
    {
        // ติดตามตำแหน่ง origin + offset ตลอดเวลา
        if (_originTransform != null)
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// อัปเดตตำแหน่ง object ตามตำแหน่ง origin
    /// </summary>
    private void UpdatePosition()
    {
        Vector3 targetPos = _originTransform.position +
                           _originTransform.TransformDirection(_positionOffset);
        transform.position = targetPos;
        transform.rotation = _originTransform.rotation * _rotationOffset;
    }

    /// <summary>
    /// OnCollisionStay ถูกเรียกทุก frame ที่ object ยังคงอยู่ใน collision
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Collider otherCollider = collision.collider;

        if (otherCollider == null)
        {
            return;
        }

        // ถ้ารู้ collider ไป แล้ว ข้าม
        if (_activeColliders.Contains(otherCollider))
        {
            return;
        }

        if (!TryAddTarget(otherCollider))
        {
            return;
        }

        _activeColliders.Add(otherCollider);
    }

    /// <summary>
    /// OnCollisionExit ถูกเรียกเมื่อ object ออกจาก collision
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        Collider otherCollider = collision.collider;

        if (otherCollider == null)
        {
            return;
        }

        RemoveTarget(otherCollider);
    }

    /// <summary>
    /// ลองเพิ่ม target หากผ่านการตรวจสอบทั้งหมด
    /// </summary>
    private bool TryAddTarget(Collider collider)
    {
        // ตรวจสอบ layer mask
        if (!IsLayerInMask(collider.gameObject.layer, _targetMask))
        {
            return false;
        }

        // ไม่ตรวจจับ owner
        if (_ownerToIgnore != null &&
            collider.transform.root == _ownerToIgnore.transform.root)
        {
            return false;
        }

        IDamageable damageable =
            collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return false;
        }

        _activeTargets.Add(damageable);
        return true;
    }

    /// <summary>
    /// ลบ target ออกจาก active list
    /// </summary>
    private void RemoveTarget(Collider collider)
    {
        _activeColliders.Remove(collider);

        IDamageable damageable =
            collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            _activeTargets.Remove(damageable);
        }
    }

    /// <summary>
    /// ตรวจสอบว่า layer อยู่ใน LayerMask หรือไม่
    /// </summary>
    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return ((mask.value & (1 << layer)) > 0);
    }

    /// <summary>
    /// ดึง list ของ targets ที่กำลัง stay อยู่
    /// </summary>
    public List<IDamageable> GetActiveTargets()
    {
        return new List<IDamageable>(_activeTargets);
    }

    /// <summary>
    /// ดึงจำนวน targets ที่กำลัง stay อยู่
    /// </summary>
    public int GetActiveTargetCount()
    {
        return _activeTargets.Count;
    }

    /// <summary>
    /// ตั้งค่า owner ที่ต้องการข้าม
    /// </summary>
    public void SetOwnerToIgnore(GameObject owner)
    {
        _ownerToIgnore = owner;
    }

    /// <summary>
    /// Clear all active targets
    /// </summary>
    public void ClearTargets()
    {
        _activeTargets.Clear();
        _activeColliders.Clear();
    }
}
