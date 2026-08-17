using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetRootMotionEnabledAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Set Root Motion Enabled")]
public class SetRootMotionEnabledActionSO : StateActionSO
{
    [SerializeField] private bool _enabled = true;
    [SerializeField] private bool _resetValue = true;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetRootMotionEnabledAction(_enabled, _resetValue);
    }
}

public class SetRootMotionEnabledAction : StateAction
{
    private readonly bool _enabled;
    private readonly bool _resetValue;
    private IRootMotionControl _rootMotionControl;

    public SetRootMotionEnabledAction(bool enabled, bool resetValue)
    {
        _enabled = enabled;
        _resetValue = resetValue;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _rootMotionControl = stateMachine.Owner.GetComponent(
            typeof(IRootMotionControl)) as IRootMotionControl;

        if (_rootMotionControl == null)
        {
            Debug.LogError(
                "SetRootMotionEnabledAction cannot find IRootMotionControl.",
                stateMachine.Owner);
        }
    }

    public override void OnStateEnter()
    {
        _rootMotionControl?.SetRootMotionEnabled(_enabled);
    }

    public override void OnUpdate() { }

    public override void OnStateExit()
    {
        if (_resetValue)
            _rootMotionControl?.SetRootMotionEnabled(!_enabled);
    }
}
