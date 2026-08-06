using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewExitStateAction",
    menuName = "YUKI Learning State Machine/SubStateMachine/Actions/ExitStateAction")]
public class ExitStateActionSO : StateActionSO
{
    [SerializeField] private string _exitId = "Default";

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new ExitStateAction(_exitId);
    }
}

public class ExitStateAction : StateAction
{
    private readonly string _exitId;
    private StateMachine _stateMachine;

    public ExitStateAction(string exitId)
    {
        _exitId = exitId;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public override void OnStateEnter()
    {
        _stateMachine.RequestExit(_exitId);
    }

    public override void OnUpdate()
    {
    }
}