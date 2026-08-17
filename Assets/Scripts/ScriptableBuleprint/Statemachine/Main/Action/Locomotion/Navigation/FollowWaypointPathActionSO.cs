using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[CreateAssetMenu(fileName = "FollowWaypointPathAction", 
menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Navigation/Follow Waypoint Path")]
public class FollowWaypointPathActionSO : StateActionSO
{
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new FollowWaypointPathAction();
    }
}
public class FollowWaypointPathAction : StateAction
{
    private PathNavigator _pathNavigator;
    private WaypointPath _waypointPath;
    public override void Awake(StateMachine stateMachine)
    {
        _pathNavigator = stateMachine.GetComponent<PathNavigator>();
        _waypointPath = stateMachine.GetComponent<WaypointPath>();
    }
    public override void OnStateEnter()
    {
        _pathNavigator.Target = _waypointPath.CurrentPoint;
    }
    public override void OnUpdate(){}
    public override void OnStateExit()
    {
        _waypointPath.MoveToNextPoint();
    }
}
