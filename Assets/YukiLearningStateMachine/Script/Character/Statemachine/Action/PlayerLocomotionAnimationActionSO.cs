using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(fileName = "PlayerLocomotionAnimationActionSO", 
menuName = "YUKI Learning State Machine/Actions/PlayerLocomotionAnimationActionSO")]
public class PlayerLocomotionAnimationActionSO : StateActionSO
{
    public float DampTime;

    public LocomotionAnimationParameter locomotionAnimationParameter;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new PlayerLocomotionAnimationAction(DampTime,locomotionAnimationParameter);
    }
}

public class PlayerLocomotionAnimationAction : StateAction
{
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _transform;

    private float _dampTime;

    private string _nameparameterZ;
    private string _nameparameterx;

    private float _minparameterZ;
    private float _minparameterX;
    private float _maxparameterZ;
    private float _maxparameterX;

    

    public PlayerLocomotionAnimationAction(float dampTime, LocomotionAnimationParameter locomotionAnimationParameter)
    {
        _dampTime = dampTime;

        _nameparameterZ = locomotionAnimationParameter.NameparameterZ;
        _nameparameterx = locomotionAnimationParameter.NameparameterX;

        _minparameterZ = locomotionAnimationParameter.MinparameterZ;
        _minparameterX = locomotionAnimationParameter.MinparameterX;
        _maxparameterZ = locomotionAnimationParameter.MaxparameterZ;
        _maxparameterX = locomotionAnimationParameter.MaxparameterX;
    }

    public override void Awake(StateMachine stateMachine)
    {
         _animator = stateMachine.GetComponent<Animator>();
         _rigidbody = stateMachine.GetComponent<Rigidbody>();
         _transform = stateMachine.GetComponent<Transform>();
    }

    public override void OnStateEnter()
    {
       _animator.Play("Locomotion",0);
    }

    public override void OnUpdate()
    {
        
        Vector3 localVelocity = _transform.InverseTransformDirection(_rigidbody.linearVelocity);
        float velocityZ = Mathf.Clamp(localVelocity.z, _minparameterZ, _maxparameterZ);
        float velocityX = Mathf.Clamp(localVelocity.x, _minparameterX, _maxparameterX);
        _animator.SetFloat(_nameparameterZ, velocityZ, _dampTime, Time.deltaTime);
        _animator.SetFloat(_nameparameterx, velocityX, _dampTime, Time.deltaTime);
    }
    public override void OnStateExit()
    {
    }
}

[System.Serializable]
public struct LocomotionAnimationParameter
{
    public string NameparameterZ;
    public string NameparameterX;

    public float MinparameterZ;
    public float MinparameterX;
    public float MaxparameterZ;
    public float MaxparameterX;
}
