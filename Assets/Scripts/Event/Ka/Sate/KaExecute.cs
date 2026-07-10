using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public class KaExecute : KaState
{
    public KaExecute(KaStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        StateMachine.StopAgent();
        StateMachine.FacePlayer();
        StateMachine.PlayAttackJumpscare();

        if (EventBus.Instance)
            EventBus.Instance.Publish(new GhostExecuteEvent(StateMachine.transform));
    }

    public override void Tick() { }

    public override void DrawGizmos() { }
}
