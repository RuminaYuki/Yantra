using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetTurnSmoothSpeedAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/PlayerLocomotion/Set Turn Smooth Speed")]
public class SetTurnSmoothSpeedActionSO : StateActionSO
{
    [SerializeField, Min(0f)] private float _turnSmoothSpeed = 1f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetTurnSmoothSpeedAction(_turnSmoothSpeed);
    }
}

public class SetTurnSmoothSpeedAction : StateAction
{
    private readonly float _turnSmoothSpeed;
    private PlayerLocomotion _playerLocomotion;

    public SetTurnSmoothSpeedAction(float turnSmoothSpeed)
    {
        _turnSmoothSpeed = turnSmoothSpeed;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();

        if (_playerLocomotion == null)
            Debug.LogError("SetTurnSmoothSpeedAction cannot find PlayerLocomotion.");
    }

    public override void OnStateEnter()
    {
        if (_playerLocomotion == null) return;
        _playerLocomotion.SetTurnSmoothSpeed(_turnSmoothSpeed);
    }

    public override void OnStateExit()
    {
        if (_playerLocomotion == null) return;
        _playerLocomotion.SetTurnSmoothSpeed(1f);
    }

    public override void OnUpdate() { }
}