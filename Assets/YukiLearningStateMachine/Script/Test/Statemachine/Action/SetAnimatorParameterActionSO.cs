using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "SetParametersAnimatorActionSO", 
    menuName = "YUKI Learning State Machine/StateMachine/Actions/SetParametersAnimatorAction")]
public class SetAnimatorParameterActionSO : StateActionSO  
{
    public ParameterSetting ParameterSetting;
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetParameterAnimatorAction(ParameterSetting);
    }
}

public class SetParameterAnimatorAction : StateAction
{
    private Animator _animator;

    private ParameterType _parameterType;
    private string _parameterName;
    private bool _value;

    public SetParameterAnimatorAction(ParameterSetting parameterSetting)
    {
        _parameterType = parameterSetting.ParameterType;
        _parameterName = parameterSetting.ParameterName;
        _value = parameterSetting.Value;
    }
    public override void Awake(StateMachine stateMachine)
    {
        _animator = stateMachine.GetComponent<Animator>();
    }
    public override void OnStateEnter()
    {
        switch (_parameterType)
        {
            case ParameterType.Bool:
                _animator.SetBool(_parameterName,_value);
                break;
            case ParameterType.Trigger:
                _animator.SetTrigger(_parameterName);
                break;
        }
    }
    public override void OnUpdate(){}
}

[System.Serializable]
public struct ParameterSetting
{
    public ParameterType ParameterType;
    public string ParameterName;
    public bool Value;
}

public enum ParameterType
{
    Bool, Trigger
}

