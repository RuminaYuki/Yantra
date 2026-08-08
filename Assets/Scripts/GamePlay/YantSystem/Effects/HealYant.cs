using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// ติดบน Heal Yantra prefab — ฮีลผู้เล่นทันทีตอน spawn แล้วลบตัวเอง
/// </summary>
public class HealYant : MonoBehaviour, IYantEffect
{
    [ReadOnly][SerializeField] private IDamageable health;
    [SerializeField] private float _healAmount = 25f;
    [Tooltip("หน่วงเวลาก่อนลบ prefab (ให้เวลา VFX เล่น)")]
    //[SerializeField] private float _lifetime = 2f;

    public bool Initialize(GameObject playerRoot)
    {
        health = playerRoot.GetComponentInParent<IDamageable>();
        if (health != null)
        {
            //health.Heal(_healAmount);
            //Debug.Log($"<color=#00FF88>[HealYant]</color> ฮีล +{_healAmount}");
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=#00FF88>[HealYant]</color> ไม่พบ IDamageable — ฮีลไม่ได้ {playerRoot.name}");
            return false;
        }
        //Destroy(gameObject);
    }
}
