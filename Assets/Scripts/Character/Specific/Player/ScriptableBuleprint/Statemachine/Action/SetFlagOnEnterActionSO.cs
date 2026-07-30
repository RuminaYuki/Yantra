using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewSetFlagOnEnterAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Set Flag On Enter")]
public class SetFlagOnEnterActionSO : StateActionSO
{
    [SerializeField] private FlagSO flag;
    [SerializeField] private bool value = true;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetFlagOnEnterAction(flag, value);
    }
}

public class SetFlagOnEnterAction : StateAction
{
    private readonly FlagSO flag;
    private readonly bool value;
    private StateFlagsAccess stateFlags;

    public SetFlagOnEnterAction(FlagSO flag, bool value)
    {
        this.flag = flag;
        this.value = value;
    }

    public override void Awake(StateMachine stateMachine)
    {
        stateFlags = stateMachine.GetComponent<StateFlagsAccess>();

        if (stateFlags == null)
            Debug.LogError("SetFlagOnEnterAction requires StateFlags or StateFlagReader on the StateMachine GameObject.");
    }

    public override void OnStateEnter()
    {
        if (stateFlags == null || flag == null) return;
        stateFlags.Set(flag, value);
    }

    public override void OnUpdate(){}
}
