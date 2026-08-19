using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// ติดบน Heal Yantra prefab — ฮีลผู้เล่นทันทีตอน spawn แล้วลบตัวเอง
/// </summary>
public class HealYant : MonoBehaviour, IYantEffect, IYantAnimationTiming
{
    [ReadOnly][SerializeField] private IHeal health;
    [SerializeField] private float _healAmount = 25f;
    [SerializeField] private StatSO statSO;

    private GameObject _playerRoot;

    public bool Initialize(GameObject playerRoot, bool holdLMB)
    {
        _playerRoot = playerRoot;
        health = playerRoot.GetComponentInParent<IHeal>();
        if (health != null)
        {
            Debug.Log($"<color=#00FF88>[HealYant]</color> ฮีล +{_healAmount}");
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=#00FF88>[HealYant]</color> ไม่พบ IHeal — ฮีลไม่ได้ {playerRoot.name}");
            return false;
        }
        //Destroy(gameObject);
    }

    public void TriggerAnimationTiming()
    {
        if (_playerRoot == null || statSO == null)
        {
            return;
        }

        health.Heal(_healAmount);

        StatCountdown statCountdown =
            _playerRoot.GetComponentInChildren<StatCountdown>();

        if (statCountdown != null)
        {
            statCountdown.SetStatCountdown(statSO, 0.01f);
        }
    }
}
