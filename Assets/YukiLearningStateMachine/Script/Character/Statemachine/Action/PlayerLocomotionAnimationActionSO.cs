using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(fileName = "PlayerLocomotionAnimationActionSO", 
menuName = "YUKI Learning State Machine/Actions/Player/PlayerLocomotionAnimationActionSO")]
public class PlayerLocomotionAnimationActionSO : StateActionSO
{
    [Header("Player Input")]
    public YantraInputObserverSO PlayerInput;

    [Header("Animation Setting")]
    public float DampTime;
    public float Multiply;

    public LocomotionAnimationParameter locomotionAnimationParameter;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new PlayerLocomotionAnimationAction(PlayerInput,DampTime,Multiply ,locomotionAnimationParameter);
    }
}

public class PlayerLocomotionAnimationAction : StateAction
{
    //PlayerInput
    private YantraInputObserverSO _playerInput;
    private Vector3 _direction;


    private Animator _animator; 
    
    private Transform _transform;
    
    private float _dampTime;
    private float _multiply;

    private string _animationStateName;

    private string _nameparameterZ;
    private string _nameparameterx;

    

    public PlayerLocomotionAnimationAction(YantraInputObserverSO playerInput, float dampTime, float multiply, LocomotionAnimationParameter locomotionAnimationParameter)
    {
        _playerInput = playerInput;

        _dampTime = dampTime;
        _multiply = multiply;

        _animationStateName = locomotionAnimationParameter.AnimationStateName;

        _nameparameterZ = locomotionAnimationParameter.NameparameterZ;
        _nameparameterx = locomotionAnimationParameter.NameparameterX;
    }

    public override void Awake(StateMachine stateMachine)
    {
         _animator = stateMachine.GetComponent<Animator>();
         _transform = stateMachine.GetComponent<Transform>();
    }

    public override void OnStateEnter()
    {
        _playerInput.OnMoveChannel += HandleMoveInput;

       _animator.Play(_animationStateName,0);
    }

     private void HandleMoveInput(Vector3 moveInput)
    {
        if(!_playerInput) return;
        _direction = moveInput;
    }

    public override void OnUpdate()
    {
        
        Vector3 localVelocity = _transform.InverseTransformDirection(_direction);
        float velocityZ = Mathf.Clamp(localVelocity.z, -1, 1);
        float velocityX = Mathf.Clamp(localVelocity.x, -1, 1);
        _animator.SetFloat(_nameparameterZ, velocityZ * _multiply, _dampTime, Time.deltaTime);
        _animator.SetFloat(_nameparameterx, velocityX * _multiply, _dampTime, Time.deltaTime);
    }
    public override void OnStateExit()
    {
        _playerInput.OnMoveChannel -= HandleMoveInput;
    }
}

[System.Serializable]
public struct LocomotionAnimationParameter
{
    public string AnimationStateName;

    public string NameparameterZ;
    public string NameparameterX;
}
