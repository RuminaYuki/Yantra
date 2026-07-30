using UnityEngine;

public class Patrol : TaniState
{
    private static readonly Color StateColor = Color.green;

    private enum WaypointLookStep
    {
        None,
        TurningOut,
        HoldingLook,
        TurningBack
    }

    private int currentWaypointIndex;
    private bool warnedMissingWaypoints;
    private WaypointLookStep lookStep;
    private float turnElapsedTime;
    private float lookHoldEndTime;
    private Quaternion originalRotation;
    private Quaternion turnStartRotation;
    private Quaternion turnTargetRotation;

    public Patrol(TaniStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        lookStep = WaypointLookStep.None;
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

        if (lookStep != WaypointLookStep.None)
        {
            LookAroundAtWaypoint();
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

        StartWaypointTurn();
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
            Debug.LogWarning($"{nameof(Patrol)} on {StateMachine.name} has an empty waypoint at index {currentWaypointIndex}.");
            return;
        }

        StateMachine.TrySetDestination(waypoint.position);
    }

    private void StartWaypointTurn()
    {
        if (!StateMachine.EnablePatrolTurnAtWaypoint || StateMachine.PatrolTurnDuration <= 0f)
        {
            MoveToNextWaypoint();
            return;
        }

        StateMachine.StopAgent();
        StateMachine.PlayIdleAnimation();

        float turnAngle = GetRandomTurnAngle();
        lookStep = WaypointLookStep.TurningOut;
        turnElapsedTime = 0f;
        originalRotation = StateMachine.transform.rotation;
        turnStartRotation = StateMachine.transform.rotation;
        turnTargetRotation = Quaternion.Euler(0f, turnAngle, 0f) * turnStartRotation;
    }

    private void LookAroundAtWaypoint()
    {
        if (lookStep == WaypointLookStep.HoldingLook)
        {
            if (Time.time < lookHoldEndTime)
            {
                return;
            }

            StartTurnBack();
            return;
        }

        TurnAtWaypoint();
    }

    private void TurnAtWaypoint()
    {
        turnElapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(turnElapsedTime / StateMachine.PatrolTurnDuration);

        StateMachine.transform.rotation = Quaternion.Slerp(
            turnStartRotation,
            turnTargetRotation,
            progress);

        if (progress < 1f)
        {
            return;
        }

        if (lookStep == WaypointLookStep.TurningOut)
        {
            lookStep = WaypointLookStep.HoldingLook;
            lookHoldEndTime = Time.time + Mathf.Max(0f, StateMachine.PatrolLookHoldDuration);
            return;
        }

        lookStep = WaypointLookStep.None;
        StateMachine.PlayPatrolAnimation();
        MoveToNextWaypoint();
    }

    private void StartTurnBack()
    {
        lookStep = WaypointLookStep.TurningBack;
        turnElapsedTime = 0f;
        turnStartRotation = StateMachine.transform.rotation;
        turnTargetRotation = originalRotation;
    }

    private float GetRandomTurnAngle()
    {
        int randomIndex = Random.Range(0, 2);
        float turnDirection = randomIndex == 0 ? -1f : 1f;
        return StateMachine.PatrolLookBackAngle * turnDirection;
    }

    private void MoveToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % StateMachine.PatrolWaypoints.Length;
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

            Gizmos.color = StateColor;
            Gizmos.DrawWireSphere(waypoint.position, StateMachine.WaypointGizmoRadius);

            Transform nextWaypoint = FindNextWaypoint(i);
            if (nextWaypoint != null)
            {
                Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
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
        Debug.LogWarning($"{nameof(Patrol)} on {StateMachine.name} needs patrol waypoints.");
    }

    private Transform FindNextWaypoint(int currentIndex)
    {
        Transform[] waypoints = StateMachine.PatrolWaypoints;
        for (int offset = 1; offset < waypoints.Length; offset++)
        {
            int nextIndex = (currentIndex + offset) % waypoints.Length;
            Transform waypoint = waypoints[nextIndex];
            if (waypoint != null)
            {
                return waypoint;
            }
        }

        return null;
    }
}
