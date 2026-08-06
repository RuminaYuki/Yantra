using UnityEngine;

/// <summary>
/// ติดบน Heal Yantra prefab — ฮีลผู้เล่นทันทีตอน spawn แล้วลบตัวเอง
/// </summary>
public class HealYant : MonoBehaviour, IYantEffect
{
    [SerializeField] private float _healAmount = 25f;
    [Tooltip("หน่วงเวลาก่อนลบ prefab (ให้เวลา VFX เล่น)")]
    //[SerializeField] private float _lifetime = 2f;

    private void Start()
    {
        //Destroy(gameObject, _lifetime);
    }

    public bool Initialize(GameObject playerRoot, YantraStatsController stats)
    {
        if (stats != null)
        {
            stats.Heal(_healAmount);
            Debug.Log($"<color=#00FF88>[HealYant]</color> ฮีล +{_healAmount}");
            return true;
        }
        else
        {
            Debug.LogWarning("<color=#00FF88>[HealYant]</color> ไม่พบ YantraStatsController — ฮีลไม่ได้");
            return false;
        }
        //Destroy(gameObject);
    }
}
