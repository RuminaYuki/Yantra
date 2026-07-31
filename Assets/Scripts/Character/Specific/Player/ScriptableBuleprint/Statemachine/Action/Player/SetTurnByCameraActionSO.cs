using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetTurnByCameraAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/PlayerLocomotion/Set Turn By Camera")]
public class SetTurnByCameraActionSO : StateActionSO
{
    [SerializeField] private bool _enabled;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetTurnByCameraAction(_enabled);
    }
}

public class SetTurnByCameraAction : StateAction
{
    private readonly bool _enabled;
    private PlayerLocomotion _playerLocomotion;

    public SetTurnByCameraAction(bool enabled)
    {
        _enabled = enabled;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();
    }

    public override void OnStateEnter()
    {
        if (_playerLocomotion == null) return;

        _playerLocomotion.SetTurnByCamera(_enabled);
    }

    public override void OnStateExit()
    {
        if (_playerLocomotion == null) return;

        _playerLocomotion.SetTurnByCamera(false);
    }

    public override void OnUpdate() { }
}
