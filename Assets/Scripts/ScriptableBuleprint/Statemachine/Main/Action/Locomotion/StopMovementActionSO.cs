using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "StopMovementAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Stop Movement")]
public class StopMovementActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new StopMovementAction();
    }
}

public class StopMovementAction : StateAction
{
    private BaseLocomotion _locomotion;

    public override void Awake(StateMachine stateMachine)
    {
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("StopMovementAction cannot find BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        _locomotion?.StopMovement();
    }

    public override void OnStateExit()
    {
        _locomotion?.StopMovement(false);
    }

    public override void OnUpdate() { }
}
