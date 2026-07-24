using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetLocomotionMultiplierAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/PlayerLocomotion/Set Locomotion Multiplier")]
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
    private PlayerLocomotion _playerLocomotion;

    public SetLocomotionMultiplierAction(float multiplier)
    {
        _multiplier = multiplier;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();
    }

    public override void OnStateEnter()
    {
        if (_playerLocomotion == null) return;

        _playerLocomotion.SetMuitply(_multiplier);
    }

    public override void OnUpdate() { }
}