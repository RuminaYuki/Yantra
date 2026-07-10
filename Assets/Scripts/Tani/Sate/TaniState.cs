public abstract class TaniState
{
    protected readonly TaniStateMachine StateMachine;

    protected TaniState(TaniStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void Enter()
    {
    }

    public virtual void Tick()
    {
    }

    public virtual void Exit()
    {
        StateMachine.StopCurrentAudio();
    }

    public virtual void DrawGizmos()
    {
    }
}
