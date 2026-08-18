using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewVoidEventChannelCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Void Event Channel")]
public class VoidEventChannelConditionSO : StateConditionSO
{
    [SerializeField] private VoidEventChannelSO _eventChannel;

    public override Condition CreateCondition()
    {
        return new VoidEventChannelCondition(_eventChannel);
    }
}

public class VoidEventChannelCondition : Condition
{
    private readonly VoidEventChannelSO _eventChannel;

    private GameObject _owner;
    private bool _wasRaised;

    public VoidEventChannelCondition(VoidEventChannelSO eventChannel)
    {
        _eventChannel = eventChannel;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner;

        if (_eventChannel == null)
        {
            Debug.LogError(
                "VoidEventChannelCondition has no Event Channel.",
                _owner);
            return;
        }

        _eventChannel.Raised += OnEventRaised;
    }

    protected override bool Statement()
    {
        return _wasRaised;
    }

    public override void Dispose()
    {
        if (_eventChannel != null)
        {
            _eventChannel.Raised -= OnEventRaised;
        }
    }

    private void OnEventRaised()
    {
        _wasRaised = true;
    }
}
