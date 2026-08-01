using UnityEngine;

/// <summary>
/// ติดบน Push Yantra prefab — ปา YantProjectile ไปในทิศที่เล็ง
/// </summary>
public class BlastYant : MonoBehaviour, IYantEffect
{
    [Header("Projectile")]
    [SerializeField] private YantProjectile _projectilePrefab;
    [SerializeField] private float _throwSpeed = 18f;
    [Tooltip("เงยปาขึ้นเล็กน้อย (องศา)")]
    [SerializeField] private float _initialAngleDeg = 8f;
    [SerializeField] private bool _useGravity = true;
    [SerializeField] private float _projectileLifetime = 6f;

    [Header("Push On Hit")]
    [SerializeField] private float _pushAcceleration = 6f;
    [SerializeField] private float _pushLifetime = 1.5f;
    [SerializeField] private bool _pushAwayFromImpact = true;

    public void Initialize(GameObject playerRoot, YantraStatsController stats, Vector3 aimDirection)
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("<color=#FFAA00>[BlastYant]</color> ยังไม่ได้ใส่ Projectile Prefab");
            Destroy(gameObject);
            return;
        }

        // เงยทิศขึ้นตามมุมที่ตั้ง
        Vector3 right = Vector3.Cross(Vector3.up, aimDirection);
        if (right.sqrMagnitude < 1e-4f) right = Vector3.right;
        Vector3 launchDir = Quaternion.AngleAxis(-_initialAngleDeg, right.normalized) * aimDirection;

        YantProjectile proj = Instantiate(
            _projectilePrefab,
            transform.position,
            Quaternion.LookRotation(launchDir));

        proj.Launch(
            launchDir * _throwSpeed,
            _useGravity,
            _projectileLifetime,
            _pushAcceleration,
            _pushLifetime,
            _pushAwayFromImpact,
            playerRoot);

        Debug.Log($"<color=#FFAA00>[PushYant]</color> ปา projectile ไปทาง {launchDir:F2}");
    }
}
