using UnityEngine;

public class KaAttack : KaState
{
    private static readonly Color StateColor = Color.red;

    private float attackEndTime;
    private bool hasDealtDamage;

    public KaAttack(KaStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        if (StateMachine.StopAgentDuringAttack)
        {
            StateMachine.StopAgent();
        }

        StateMachine.FacePlayer();
        StateMachine.PlayAttackAnimation();
        attackEndTime = Time.time + StateMachine.AttackDuration;
        hasDealtDamage = false;
    }

    public override void Tick()
    {
        if (Time.time < attackEndTime)
        {
            StateMachine.FacePlayer();

            if (!hasDealtDamage && StateMachine.IsPlayerInAttackRange())
            {
                hasDealtDamage = true;
                Transform player = StateMachine.Player;
                if (player != null)
                {
                    var stats = player.GetComponentInParent<YantraStatsController>()
                             ?? player.GetComponentInChildren<YantraStatsController>();
                    if (stats != null)
                    {
                        stats.TakeDamage(StateMachine.AttackDamage);
                        if (stats.GetCurrentHp() <= stats.GetMinHp())
                        {
                            StateMachine.ChangeState(StateMachine.ExecuteState);
                            return;
                        }
                    }
                }
            }
            return;
        }

        StateMachine.ChangeState(StateMachine.PatrolState);
    }

    public override void DrawGizmos()
    {
        StateMachine.DrawAttackArea(StateColor);
    }
}
