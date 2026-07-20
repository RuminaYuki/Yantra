using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "PlayerLocomotionAction", 
    menuName = "YUKI Learning State Machine/Actions/Player/PlayerLocomotionAction")]
public class PlayerLocomotionActionSO : StateActionSO
{

    [Header("RotateSystem")]
    public float RotateSpeed;

    [Header("Optional")]
    public bool FaceMoveDirection = false;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        
        return new PlayerLocomotionAction(RotateSpeed,FaceMoveDirection);
    }
}

public class PlayerLocomotionAction : StateAction
{
    private PlayerLocomotion _playerLocomotion;

    private float _rotationspeed;
    private bool _faceMoveDirection;
    public PlayerLocomotionAction(float rotationspeed,bool faceMoveDirection)
    {
        _rotationspeed = rotationspeed;
        _faceMoveDirection = faceMoveDirection;
    }
    public override void Awake(StateMachine stateMachine)
    {
         _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();
    }

    public override void OnStateEnter()
    {
        _playerLocomotion.SetSpeedRotation(_rotationspeed);
        _playerLocomotion.SetEnableFaceMoveDirection(_faceMoveDirection);
    }

    public override void OnUpdate(){}

    public override void OnStateExit()
    {
    }
}
