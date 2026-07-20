using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "LeftShiftKeyCondition",
    menuName = "YUKI Learning State Machine/Conditions/LeftShiftKey")]
public class LeftShiftKeyConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new LeftShiftKeyCondition();
    }
}
public class LeftShiftKeyCondition : Condition
{
    protected override bool Statement()
    {
        return Input.GetKeyDown(KeyCode.LeftShift);
    }
}