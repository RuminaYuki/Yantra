using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "PlayerBookUpperAction", 
    menuName = "YUKI Learning State Machine/Actions/Player/PlayerBookUpperAction")]
public class PlayerBookUpperAnimationActionSO : StateActionSO
{   
    public string AnimationStateName;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new PlayerBookUpperAnimationAction(AnimationStateName);
    }
}

public class PlayerBookUpperAnimationAction : StateAction
{
    private Animator _animator;
    private string _animationStateName;

    public PlayerBookUpperAnimationAction(string StateName)
    {
        _animationStateName = StateName;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _animator = stateMachine.GetComponent<Animator>();
    }

    public override void OnStateEnter()
    {
        _animator.Play(_animationStateName, 3);
    }

    public override void OnUpdate()
    {
        // Implement the logic for the PlayerBookUpperAnimationAction here
    }
}
