using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "ExecuteAttackAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Attack System/Execute Attack")]
public class ExecuteAttackActionSO : StateActionSO
{
    [SerializeField] private GameObject _attackPrefab;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new ExecuteAttackAction(_attackPrefab);
    }
}

public class ExecuteAttackAction : StateAction
{
    private readonly GameObject _attackPrefab;
    private AttackSystem _attackSystem;

    public ExecuteAttackAction(GameObject attackPrefab)
    {
        _attackPrefab = attackPrefab;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _attackSystem = stateMachine.GetComponent<AttackSystem>();

        if (_attackSystem == null)
            Debug.LogError("ExecuteAttackAction cannot find AttackSystem.");
    }

    public override void OnStateEnter()
    {
        if (_attackSystem == null ||
            _attackPrefab == null)
        {
            return;
        }

        _attackSystem.TryAttack(_attackPrefab);
    }

    public override void OnUpdate() { }
    public override void OnStateExit() { }
}
