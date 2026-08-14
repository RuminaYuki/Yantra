using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewRestoreFullHealthAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Health/Restore Full Health")]
public class RestoreFullHealthActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new RestoreFullHealthAction();
    }
}

public class RestoreFullHealthAction : StateAction
{
    private Health _health;

    public override void Awake(StateMachine stateMachine)
    {
        _health = stateMachine.GetComponent<Health>();

        if (_health == null)
        {
            Debug.LogError(
                $"{nameof(RestoreFullHealthAction)} cannot find Health on " +
                $"{stateMachine.Owner.name}.",
                stateMachine.Owner);
        }
    }

    public override void OnStateEnter()
    {
        if (_health == null)
        {
            return;
        }

        _health.RestoreFullHealth();
    }

    public override void OnUpdate()
    {
    }
}
