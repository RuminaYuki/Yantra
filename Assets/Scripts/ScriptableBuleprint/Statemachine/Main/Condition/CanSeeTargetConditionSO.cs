using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewCanSeeTargetCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Can See Target")]
public class CanSeeTargetConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new CanSeeTargetCondition();
    }
}

public class CanSeeTargetCondition : Condition
{
    private LineOfSight lineOfSight;

    public override void Awake(StateMachine stateMachine)
    {
        Transform owner = stateMachine.GetComponent<Transform>();

        if (owner != null)
        {
            lineOfSight = owner.GetComponentInChildren<LineOfSight>(true);
        }

        if (lineOfSight == null)
        {
            Debug.LogError(
                "CanSeeTargetCondition cannot find LineOfSight on the StateMachine GameObject or its children.",
                owner);
        }
    }

    protected override bool Statement()
    {
        if (lineOfSight == null || lineOfSight.Target == null)
        {
            return false;
        }

        return lineOfSight.CanSeeTarget();
    }
}
