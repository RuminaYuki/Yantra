using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "IdleAction", 
    menuName = "YUKI Learning State Machine/Actions/IdleActionSO")]
public class IdleActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new IdleAction();
    }
}

public class IdleAction : StateAction
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

