using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewIsDeadCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Health/Is Dead")]
public class IsDeadConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new IsDeadCondition();
    }
}

public class IsDeadCondition : Condition
{
    private Health health;

    public override void Awake(StateMachine stateMachine)
    {
        health = stateMachine.GetComponent<Health>();

        if (health == null)
            Debug.LogError($"IsDeadCondition Not found Health!");
    }

    protected override bool Statement()
    {
        if (health == null) return false;

        //will return false if the character has more than 0 health, otherwise True
        return health.IsDead;
    }
}
