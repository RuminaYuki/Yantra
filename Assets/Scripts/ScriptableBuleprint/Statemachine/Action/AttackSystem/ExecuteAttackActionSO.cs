using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "ExecuteAttackAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Attack System/Execute Attack")]
public class ExecuteAttackActionSO : StateActionSO
{
    [SerializeField] private GameObject _attackPrefab;
    [SerializeField, Min(0f)]
    private float _damage = 10f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new ExecuteAttackAction(_attackPrefab, _damage);
    }
}

public class ExecuteAttackAction : StateAction
{
    private readonly GameObject _attackPrefab;
    private readonly float _damage;
    private PairedAttackController _pairedAttackController;


    public ExecuteAttackAction(GameObject attackPrefab,float damage)
    {
        _attackPrefab = attackPrefab;
        _damage = damage;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _pairedAttackController = stateMachine.GetComponent<PairedAttackController>();

        if (_pairedAttackController == null)
            Debug.LogError("ExecuteAttackAction cannot find AttackSystem.");
    }

    public override void OnStateEnter()
    {
        if (_pairedAttackController == null || _attackPrefab == null)
        {
            return;
        }

        _pairedAttackController.TryAttack(_attackPrefab, _damage);
    }

    public override void OnUpdate() { }
    public override void OnStateExit() { }
}
