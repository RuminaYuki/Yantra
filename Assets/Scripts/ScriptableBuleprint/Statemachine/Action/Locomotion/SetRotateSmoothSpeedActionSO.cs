using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetTurnSmoothSpeedAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Set Turn Smooth Speed")]
public class SetRotateSmoothSpeedActionSO : StateActionSO
{
    [SerializeField, Min(0f)] private float _rotateSmoothSpeed = 1f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetRotateSmoothSpeedAction(_rotateSmoothSpeed);
    }
}

public class SetRotateSmoothSpeedAction : StateAction
{
    private readonly float _rotateSmoothSpeed;
    private BaseLocomotion _locomotion;

    public SetRotateSmoothSpeedAction(float rotateSmoothSpeed)
    {
        _rotateSmoothSpeed = rotateSmoothSpeed;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("SetRotateSmoothSpeedAction cannot find BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        if (_locomotion == null) return;
        _locomotion.SetRotateSmoothSpeed(_rotateSmoothSpeed);
    }

    public override void OnStateExit()
    {
        if (_locomotion == null) return;
        _locomotion.SetRotateSmoothSpeed(1f);
    }

    public override void OnUpdate() { }
}
