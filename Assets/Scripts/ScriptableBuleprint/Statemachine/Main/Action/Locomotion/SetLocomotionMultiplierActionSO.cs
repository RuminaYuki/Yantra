using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetLocomotionMultiplierAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Set Locomotion Multiplier")]
public class SetLocomotionMultiplierActionSO : StateActionSO
{
    [SerializeField] private float _multiplier = 1f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetLocomotionMultiplierAction(_multiplier);
    }
}

public class SetLocomotionMultiplierAction : StateAction
{
    private readonly float _multiplier;
    private BaseLocomotion _locomotion;

    public SetLocomotionMultiplierAction(float multiplier)
    {
        _multiplier = multiplier;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("SetLocomotionMultiplierAction cannot find BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        if (_locomotion == null) return;

        _locomotion.SetMoveMultiply(_multiplier);
    }

    public override void OnUpdate() { }

    public override void OnStateExit()
    {
        _locomotion?.SetMoveMultiply(1f);
    }
}
