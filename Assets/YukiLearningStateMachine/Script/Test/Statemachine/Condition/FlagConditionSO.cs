using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewFlagCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Check Flag")]
public class FlagConditionSO : StateConditionSO
{
    [SerializeField] private FlagSO flag;

    public override Condition CreateCondition()
    {
        return new FlagCondition(flag);
    }
}

public class FlagCondition : Condition
{
    private readonly FlagSO flag;
    private StateFlagsAccess stateFlags;

    public FlagCondition(FlagSO flag)
    {
        this.flag = flag;
    }

    public override void Awake(StateMachine stateMachine)
    {
        stateFlags = stateMachine.GetComponent<StateFlagsAccess>();

        if (stateFlags == null)
            Debug.LogError("FlagCondition requires StateFlags or StateFlagReader on the StateMachine GameObject.");
    }

    protected override bool Statement()
    {
        if (stateFlags == null || flag == null)
        {
            return false;
        }

        return stateFlags.Get(flag);
    }
}
