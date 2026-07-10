using UnityEngine;

public class Idle : TaniState
{
    private static readonly Color StateColor = Color.white;

    public Idle(TaniStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        StateMachine.StopAgent();
        StateMachine.PlayIdleAnimation();
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
        }
    }

    public override void DrawGizmos()
    {
        StateMachine.DrawSightCone(StateColor);
        StateMachine.DrawAttackArea(StateColor);
    }
}
