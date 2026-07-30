using UnityEngine;

public class KaChase : KaState
{
    private static readonly Color StateColor = Color.yellow;

    public KaChase(KaStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        StateMachine.SetAgentSpeed(StateMachine.ChaseSpeed);
        StateMachine.PlayChaseAnimation();
    }

    public override void Tick()
    {
        if (StateMachine.IsPlayerInAttackRange())
        {
            StateMachine.ChangeState(StateMachine.AttackState);
            return;
        }

        if (!StateMachine.CanSeePlayer())
        {
            StateMachine.ChangeState(StateMachine.SearchState);
            return;
        }

        Transform player = StateMachine.Player;
        if (player == null)
        {
            StateMachine.ChangeState(StateMachine.SearchState);
            return;
        }

        if (!StateMachine.TrySetReachableDestination(player.position))
        {
            StateMachine.ChangeToUnreachablePlayerSearch();
            return;
        }

        StateMachine.RecordLastSeenPlayerPosition();
    }

    public override void DrawGizmos()
    {
        StateMachine.DrawSightCone(StateColor);

        StateMachine.DrawAttackArea(StateColor);

        Transform player = StateMachine.Player;
        if (player != null && StateMachine.CanSeePlayer())
        {
            Gizmos.color = StateMachine.GetDebugColor(StateColor);
            Gizmos.DrawLine(
                StateMachine.transform.position + StateMachine.DebugGizmoOffset,
                player.position + StateMachine.DebugGizmoOffset);
        }
    }
}
