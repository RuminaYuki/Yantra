using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetYantControllerCastAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Set Yant Controller Cast")]
public class SetYantControllerCastActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetYantControllerCastAction();
    }
}

public class SetYantControllerCastAction : StateAction
{
    private YantController _yantController;

    public override void Awake(StateMachine stateMachine)
    {
        _yantController = stateMachine.GetComponent<YantController>();
    }

    public override void OnStateEnter()
    {
        _yantController.SetCastInputObserverSO(true);
    }

    public override void OnStateExit()
    {
        _yantController.SetCastInputObserverSO(false);
    }

    public override void OnUpdate() { }
}
