using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "StartFlagCooldownAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Timer/Start Flag Cooldown")]
public class StartFlagCooldownActionSO : StateActionSO
{
    [SerializeField] private FlagSO _flag;
    [SerializeField, Min(0f)] private float _duration = 1f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new StartFlagCooldownAction(_flag, _duration);
    }
}

public class StartFlagCooldownAction : StateAction
{
    private readonly FlagSO _flag;
    private readonly float _duration;

    private FlagCountdown _flagCountdown;

    public StartFlagCooldownAction(FlagSO flag, float duration)
    {
        _flag = flag;
        _duration = duration;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _flagCountdown = stateMachine.GetComponent<FlagCountdown>();

        if (_flagCountdown == null)
        {
            Debug.LogError(
                "StartFlagCooldownAction requires FlagCountdown on the StateMachine GameObject.");
        }
    }

    public override void OnStateEnter()
    {
        if (_flagCountdown == null || _flag == null)
            return;

        _flagCountdown.SetFlagCountdown(
            _flag,
            _duration,
            false);
    }

    public override void OnUpdate() { }
    public override void OnStateExit() { }
}
