using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[CreateAssetMenu(
    fileName = "ReachPathCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Locomotion/Path/ReachPath")]
public class ReachPathConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return null;
    }
}
public class ReachPathCondition : Condition
{
    private WaypointPath _waypointPath;
    public override void Awake(StateMachine stateMachine)
    {
        _waypointPath = stateMachine.GetComponent<WaypointPath>();
    }
    protected override bool Statement()
    {
        // Implementation for reach path condition
        return false;
    }
}
