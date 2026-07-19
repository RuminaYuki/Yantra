using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "IsRunningInputCondition",
    menuName = "YUKI Learning State Machine/Conditions/Is Running Input")]
public class IsRunningConditionSO : StateConditionSO
{
    [SerializeField] private YantraInputObserverSO _playerInput;

    public override Condition CreateCondition()
    {
        return new IsRunningInputCondition(_playerInput);
    }
}

public class IsRunningInputCondition : Condition
{
    private bool _isRunning;

    public IsRunningInputCondition(YantraInputObserverSO playerInput)
    {
        if (playerInput == null)
        {
            Debug.LogError("IsRunningCondition requires " + "YantraInputObserverSO.");
            return;
        }
        playerInput.OnRunChannel += HandleRunInput;
    }
    
    private void HandleRunInput(bool isRunning)
    {
        _isRunning = isRunning;
    }

    protected override bool Statement()
    {
        return _isRunning;
    }
}