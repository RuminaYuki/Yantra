using UnityEngine;

public class KaSearch : KaState
{
    private static readonly Color StateColor = new Color(1f, 0.45f, 0f);

    private float searchEndTime;
    private bool reachedLastSeenPosition;

    public KaSearch(KaStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        StateMachine.SetAgentSpeed(StateMachine.SearchSpeed);
        reachedLastSeenPosition = false;
        searchEndTime = 0f;

        if (StateMachine.HasLastSeenPlayerPosition)
        {
            StateMachine.TrySetDestination(StateMachine.LastSeenPlayerPosition);
            return;
        }

        StartScanning();
    }

    public override void Tick()
    {
        float rangeBonus = reachedLastSeenPosition ? StateMachine.SearchSightRangeBonus : 0f;
        float angleBonus = reachedLastSeenPosition ? StateMachine.SearchSightAngleBonus : 0f;

        if (StateMachine.CanSeePlayer(rangeBonus, angleBonus))
        {
            Transform player = StateMachine.Player;
            if (player == null || !StateMachine.TrySetReachableDestination(player.position))
            {
                if (!reachedLastSeenPosition)
                {
                    StartScanning();
                }
            }
            else
            {
                StateMachine.RecordLastSeenPlayerPosition();
                StateMachine.ChangeState(StateMachine.ChaseState);
                return;
            }
        }

        if (!reachedLastSeenPosition)
        {
            if (!StateMachine.HasReachedDestination())
            {
                return;
            }

            StartScanning();
        }

        if (Time.time >= searchEndTime)
        {
            StateMachine.ChangeState(StateMachine.PatrolState);
        }
    }

    public override void DrawGizmos()
    {
        float rangeBonus = reachedLastSeenPosition ? StateMachine.SearchSightRangeBonus : 0f;
        float angleBonus = reachedLastSeenPosition ? StateMachine.SearchSightAngleBonus : 0f;

        StateMachine.DrawSightCone(
            StateColor,
            rangeBonus,
            angleBonus);

        StateMachine.DrawAttackArea(StateColor);

        if (StateMachine.HasLastSeenPlayerPosition)
        {
            Vector3 offset = StateMachine.DebugGizmoOffset;
            Gizmos.color = StateMachine.GetDebugColor(StateColor);
            Gizmos.DrawWireSphere(StateMachine.LastSeenPlayerPosition + offset, StateMachine.WaypointGizmoRadius);
            Gizmos.DrawLine(StateMachine.transform.position + offset, StateMachine.LastSeenPlayerPosition + offset);
        }
    }

    private void StartScanning()
    {
        reachedLastSeenPosition = true;
        StateMachine.StopAgent();
        StateMachine.PlaySearchAnimation();
        searchEndTime = Time.time + StateMachine.SearchDuration;
    }
}
