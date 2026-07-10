using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public class Execute : TaniState
{
    public Execute(TaniStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        StateMachine.StopAgent();
        StateMachine.FacePlayer();
        StateMachine.PlayExecuteAnimation();

        if (EventBus.Instance)
            EventBus.Instance.Publish(new GhostExecuteEvent(StateMachine.transform));
    }

    public override void Tick() { }

    public override void DrawGizmos() { }
}
