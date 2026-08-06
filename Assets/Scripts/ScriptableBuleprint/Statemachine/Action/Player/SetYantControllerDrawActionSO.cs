using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetEnableYantControllerAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Set Yant Controller Draw")]
public class SetYantControllerDrawActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetYantControllerDrawAction();
    }
}

public class SetYantControllerDrawAction : StateAction
{
    private YantController _yantController;
    private DrawOn3DMesh _drawOn3DMesh;

    public override void Awake(StateMachine stateMachine)
    {
        _yantController = stateMachine.GetComponent<YantController>();
    }

    public override void OnStateEnter()
    {
        _yantController.SetDrawInputObserverSO(true);
    }

    public override void OnStateExit()
    {
        _yantController.SetDrawInputObserverSO(false);
    }

    public override void OnUpdate() { }
}
