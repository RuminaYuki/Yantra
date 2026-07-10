using UnityEngine;

public class KaPatrol : KaState
{
    private static readonly Color StateColor = Color.green;

    private int currentWaypointIndex;
    private int waypointDirection = 1;
    private bool warnedMissingWaypoints;

    public KaPatrol(KaStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        StateMachine.SetAgentSpeed(StateMachine.PatrolSpeed);
        StateMachine.PlayPatrolAnimation();
        MoveToCurrentWaypoint();
    }

    public override void Tick()
    {
        if (StateMachine.IsPlayerInAttackRange())
        {
            StateMachine.ChangeState(StateMachine.AttackState);
            return;
        }

        if (StateMachine.CanSeePlayer())
        {
            Transform player = StateMachine.Player;
            if (player == null || !StateMachine.TrySetReachableDestination(player.position))
            {
                StateMachine.ChangeToUnreachablePlayerSearch();
                return;
            }

            StateMachine.RecordLastSeenPlayerPosition();
            StateMachine.ChangeState(StateMachine.ChaseState);
            return;
        }

        if (!HasWaypoints())
        {
            WarnMissingWaypoints();
            return;
        }

        if (!StateMachine.HasReachedDestination())
        {
            return;
        }

        MoveToNextWaypoint();
    }

    private void MoveToCurrentWaypoint()
    {
        if (!HasWaypoints())
        {
            WarnMissingWaypoints();
            return;
        }

        Transform[] waypoints = StateMachine.PatrolWaypoints;
        Transform waypoint = waypoints[currentWaypointIndex];
        if (waypoint == null)
        {
            Debug.LogWarning($"{nameof(KaPatrol)} on {StateMachine.name} has an empty waypoint at index {currentWaypointIndex}.");
            return;
        }

        StateMachine.TrySetDestination(waypoint.position);
    }

    private void MoveToNextWaypoint()
    {
        Transform[] waypoints = StateMachine.PatrolWaypoints;
        if (waypoints.Length <= 1)
        {
            MoveToCurrentWaypoint();
            return;
        }

        int nextWaypointIndex = currentWaypointIndex + waypointDirection;
        if (nextWaypointIndex >= waypoints.Length || nextWaypointIndex < 0)
        {
            waypointDirection *= -1;
            nextWaypointIndex = currentWaypointIndex + waypointDirection;
        }

        currentWaypointIndex = nextWaypointIndex;
        MoveToCurrentWaypoint();
    }

    public override void DrawGizmos()
    {
        StateMachine.DrawSightCone(StateColor);

        StateMachine.DrawAttackArea(StateColor);

        Transform[] waypoints = StateMachine.PatrolWaypoints;
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null)
            {
                continue;
            }

            Vector3 waypointPosition = waypoint.position + StateMachine.DebugGizmoOffset;
            Gizmos.color = StateMachine.GetDebugColor(StateColor);
            Gizmos.DrawWireSphere(waypointPosition, StateMachine.WaypointGizmoRadius);

            Transform nextWaypoint = FindConnectedWaypoint(i, 1);
            if (nextWaypoint != null)
            {
                Gizmos.DrawLine(waypointPosition, nextWaypoint.position + StateMachine.DebugGizmoOffset);
            }

            Transform previousWaypoint = FindConnectedWaypoint(i, -1);
            if (previousWaypoint != null)
            {
                Gizmos.DrawLine(waypointPosition, previousWaypoint.position + StateMachine.DebugGizmoOffset);
            }
        }
    }

    private bool HasWaypoints()
    {
        Transform[] waypoints = StateMachine.PatrolWaypoints;
        return waypoints != null && waypoints.Length > 0;
    }

    private void WarnMissingWaypoints()
    {
        if (warnedMissingWaypoints)
        {
            return;
        }

        warnedMissingWaypoints = true;
        StateMachine.ChangeState(StateMachine.IdleState);
        Debug.LogWarning($"{nameof(KaPatrol)} on {StateMachine.name} needs patrol waypoints.");
    }

    private Transform FindConnectedWaypoint(int currentIndex, int direction)
    {
        Transform[] waypoints = StateMachine.PatrolWaypoints;
        int nextIndex = currentIndex + direction;
        while (nextIndex >= 0 && nextIndex < waypoints.Length)
        {
            Transform waypoint = waypoints[nextIndex];
            if (waypoint != null)
            {
                return waypoint;
            }

            nextIndex += direction;
        }

        return null;
    }
}
