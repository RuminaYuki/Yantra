using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;


[CreateAssetMenu(fileName = "IsBookInputCondition",
    menuName = "YUKI Learning State Machine/Conditions/Player/IsBookInputCondition")]
public class IsBookInputConditionSO : StateConditionSO
{
    [SerializeField] private YantraInputObserverSO _playerInput;

    public override Condition CreateCondition()
    {
        return new IsBookInputCondition(_playerInput);
    }
}

public class IsBookInputCondition : Condition
{
    private bool _isBook;
    private YantraInputObserverSO _playerInput;

    public override void Awake(StateMachine stateMachine)
    {
        
    }
    public IsBookInputCondition(YantraInputObserverSO playerInput)
    {
        if (playerInput == null)
        {
            Debug.LogError("IsCrouchCondition requires " + "YantraInputObserverSO.");
            return;
        }

        _playerInput = playerInput;
        _playerInput.OnPressQ_ButtonChannel += HandleBookInput;
    }
    public override void Dispose()
    {
        if (_playerInput == null) return;
        _playerInput.OnPressQ_ButtonChannel -= HandleBookInput;
    }

    private void HandleBookInput()
    {
        _isBook = true;
    }

    protected override bool Statement()
    {
        bool result = _isBook;

        _isBook = false;

        return result;
    }
}
