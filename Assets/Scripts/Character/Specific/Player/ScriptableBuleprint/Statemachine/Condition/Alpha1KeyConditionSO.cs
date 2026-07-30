using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "Alpha1KeyCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Alpha1 Key")]
public class Alpha1KeyConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new Alpha1KeyCondition();
    }
}
public class Alpha1KeyCondition : Condition
{
    protected override bool Statement()
    {
        return Input.GetKeyDown(KeyCode.Alpha1);
    }
}

