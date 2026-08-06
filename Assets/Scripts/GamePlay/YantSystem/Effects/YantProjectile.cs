using UnityEngine;

/// <summary>
/// ลูกยนต์ที่ถูกปาออกไป บินตามความเร็ว/แรงโน้มถ่วงที่ตั้งไว้
/// เมื่อชนวัตถุที่มี Rigidbody จะติด YantGradualPush ให้ดันทีละนิดตาม lifeTime
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class YantProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;

    [Header("On Hit")]
    [Tooltip("VFX ที่จะ spawn ตอนชน (ถ้ามี)")]
    [SerializeField] private GameObject _hitVfxPrefab;
    [Tooltip("ทำลายลูกยนต์ทันทีหลังชนหรือไม่")]
    [SerializeField] private bool _destroyOnHit = true;

    private float _pushAcceleration;
    private float _pushLifeTime;
    private bool _pushAwayFromImpact;
    private GameObject _owner;
    private FlagSO _debuff;
    private float _debuffDuration;

    private bool _hasHit;
    private StateFlags _stateFlags;

    private void OnValidate()
    {
        if (_rb == null) TryGetComponent(out _rb);
    }

    /// <summary>ยิงลูกยนต์ออกไป กำหนดทุกพารามิเตอร์จาก PushYantSO</summary>
    public void Launch(
        Vector3 velocity,
        bool useGravity,
        float lifeTime,
        float pushAcceleration,
        float pushLifeTime,
        bool pushAwayFromImpact,
        GameObject owner,
        FlagSO debuff = null,
        float duration = 0f)
    {
        if (_rb == null) TryGetComponent(out _rb);

        _rb.useGravity = useGravity;
        _rb.linearVelocity = velocity;

        _pushAcceleration = pushAcceleration;
        _pushLifeTime = pushLifeTime;
        _pushAwayFromImpact = pushAwayFromImpact;
        _owner = owner;
        _debuff = debuff;
        _debuffDuration = duration;

        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);

        if (_hasHit) return;

        // ไม่ชนตัวเอง/เจ้าของ
        if (_owner != null && other.transform.root == _owner.transform.root) return;

        _hasHit = true;

        if (_debuff != null
            && other.gameObject.TryGetComponent(out StateFlags stateFlags)
            && stateFlags.Contains(_debuff))
        {
            if (other.gameObject.TryGetComponent(out FlagCountdown flagCountdown))
            {
                float duration = _debuffDuration > 0f ? _debuffDuration : 0f;
                flagCountdown.setFlagCountdown(_debuff, duration);
            }
            else
            {
                stateFlags.Set(_debuff, true);
            }
            //Debug.Log(other.gameObject.name + " has debuff " + _debuff.name);
        }

        if (_hitVfxPrefab != null)
            Instantiate(_hitVfxPrefab, other.ClosestPoint(transform.position), Quaternion.identity);

        if (_destroyOnHit) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);

        if (_hasHit) return;

        // ไม่ชนตัวเอง/เจ้าของ
        if (_owner != null && collision.transform.root == _owner.transform.root) return;

        _hasHit = true;

        if (_debuff != null
            && collision.transform.root.TryGetComponent(out StateFlags stateFlags)
            && stateFlags.Get(_debuff))
        {
            stateFlags.Set(_debuff, true);
        }

        /*Rigidbody targetRb = collision.rigidbody;
        if (targetRb != null && !targetRb.isKinematic)
        {
            Vector3 pushDir = _pushAwayFromImpact
                ? (targetRb.worldCenterOfMass - transform.position)
                : _rb.linearVelocity;

            pushDir.Normalize();

            if (!targetRb.TryGetComponent(out YantGradualPush push))
                push = targetRb.gameObject.AddComponent<YantGradualPush>();

            push.Begin(pushDir, _pushAcceleration, _pushLifeTime);
        }*/

        if (_hitVfxPrefab != null)
            Instantiate(_hitVfxPrefab, collision.GetContact(0).point, Quaternion.identity);

        if (_destroyOnHit) Destroy(gameObject);
    }
}
