using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetEnableYantControllerAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Set Enable Yant Controller")]
public class SetEnableYantControllerActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetEnableYantControllerAction();
    }
}

public class SetEnableYantControllerAction : StateAction
{
    private YantController _yantController;

    public override void Awake(StateMachine stateMachine)
    {
        _yantController = stateMachine.GetComponent<YantController>();
    }

    public override void OnStateEnter()
    {
        _yantController.SetEnableUseInputObserverSO(true);
    }

    public override void OnStateExit()
    {
        _yantController.SetEnableUseInputObserverSO(false);
    }

    public override void OnUpdate() { }
}
