public abstract class KaState
{
    protected readonly KaStateMachine StateMachine;

    protected KaState(KaStateMachine stateMachine)
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
    }

    public virtual void DrawGizmos()
    {
    }
}
