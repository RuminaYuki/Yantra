using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "LockMovementAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Lock Movement")]
public class LockMovementActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new LockMovementAction();
    }
}

public class LockMovementAction : StateAction
{
    private BaseLocomotion _locomotion;

    public override void Awake(StateMachine stateMachine)
    {
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("LockMovementAction cannot find BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        _locomotion?.LockMovement(this);
    }

    public override void OnStateExit()
    {
        _locomotion?.UnlockMovement(this);
    }

    public override void OnUpdate() { }
}
