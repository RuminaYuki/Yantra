using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(fileName = "PlayerLocomotionAnimationAction", 
menuName = "YUKI Learning State Machine/Actions/Player/PlayerLocomotionAnimationAction")]
public class PlayerLocomotionAnimationActionSO : StateActionSO
{
    [Header("Animation Setting")]
    public float Multiply;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new PlayerLocomotionAnimationAction(Multiply);
    }
}

public class PlayerLocomotionAnimationAction : StateAction
{
    private PlayerLocomotion _playerLocomotion;
    private float _multiply;

    public PlayerLocomotionAnimationAction(float multiply)
    {
        _multiply = multiply;
    }

    public override void Awake(StateMachine stateMachine)
    {
         _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();
    }
    public override void OnStateEnter()
    {
        _playerLocomotion.SetMuitply(_multiply);
    }

    public override void OnUpdate(){}
}
