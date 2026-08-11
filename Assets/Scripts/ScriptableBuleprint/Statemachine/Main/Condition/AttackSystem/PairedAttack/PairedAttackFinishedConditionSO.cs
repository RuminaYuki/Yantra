using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "PairedAttackFinishedCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Attack System/Paired Attack/Paired Attack Finished")]
public class PairedAttackFinishedConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new PairedAttackFinishedCondition();
    }
}

public class PairedAttackFinishedCondition : Condition
{
    private PairedAttackController _pairedAttackController;

    public override void Awake(StateMachine stateMachine)
    {
        _pairedAttackController =
            stateMachine.GetComponent<PairedAttackController>();

        if (_pairedAttackController == null)
        {
            Debug.LogError(
                "PairedAttackFinishedCondition cannot find PairedAttackController.");
        }
    }

    protected override bool Statement()
    {
        return _pairedAttackController != null &&
               _pairedAttackController.HasCompletedAttack;
    }
}
