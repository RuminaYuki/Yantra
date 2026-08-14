using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewRecentlyDamagedCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Health/Recently Damaged")]
public class RecentlyDamagedConditionSO : StateConditionSO
{
    [Tooltip("ต้องไม่มีดาเมจเข้ามานานกว่าค่านี้ จึงถือว่าดาเมจหยุดแล้ว")]
    [SerializeField, Min(0f)]
    private float _damageTimeout = 1.2f;

    public override Condition CreateCondition()
    {
        return new RecentlyDamagedCondition(_damageTimeout);
    }
}

public class RecentlyDamagedCondition : Condition
{
    private readonly float _damageTimeout;

    private Health _health;
    private float _lastDamageTime = float.NegativeInfinity;

    public RecentlyDamagedCondition(float damageTimeout)
    {
        _damageTimeout = damageTimeout;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _health = stateMachine.GetComponent<Health>();

        if (_health == null)
        {
            Debug.LogError(
                $"{nameof(RecentlyDamagedCondition)} cannot find Health " +
                $"on {stateMachine.Owner.name}.");

            return;
        }

        _health.Onhit += HandleHit;
    }

    private void HandleHit()
    {
        // ดาเมจแต่ละ tick จะต่ออายุ Hurt state
        _lastDamageTime = Time.time;

        // ไม่จำเป็นต้อง ClearStatementCache ตรงนี้
        // เพราะ State Machine จะล้าง cache ในแต่ละรอบ evaluation
    }

    protected override bool Statement()
    {
        if (_health == null || _health.IsDead)
        {
            return false;
        }

        float timeSinceLastDamage = Time.time - _lastDamageTime;

        return timeSinceLastDamage <= _damageTimeout;
    }

    public override void Dispose()
    {
        if (_health != null)
        {
            _health.Onhit -= HandleHit;
        }

        _health = null;
    }
}