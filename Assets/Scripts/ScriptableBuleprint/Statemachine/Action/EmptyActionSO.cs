using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "EmptyAction", 
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Empty Action")]
public class EmptyActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new EmptyAction();
    }
}

public class EmptyAction : StateAction
{
    public override void OnStateEnter()
    {
        
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        
    }
}

