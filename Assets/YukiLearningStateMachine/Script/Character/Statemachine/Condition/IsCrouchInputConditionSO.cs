using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "IsCrouchInputCondition",
    menuName = "YUKI Learning State Machine/Conditions/Is Crouch Input")]
public class IsCrouchInputConditionSO : StateConditionSO
{
    [SerializeField] private YantraInputObserverSO _playerInput;
    public override Condition CreateCondition()
    {
        return new IsCrouchInputCondition(_playerInput);
    }
}

public class IsCrouchInputCondition : Condition
{
    private bool _isCrouch;
    private YantraInputObserverSO _playerInput;


    public IsCrouchInputCondition(YantraInputObserverSO playerInput)
    {
        if (playerInput == null)
        {
            Debug.LogError("IsCrouchCondition requires " + "YantraInputObserverSO.");
            return;
        }
        _playerInput = playerInput;
        _playerInput.OnCrouchChannel += HandleCrouchInput;
    }
    public override void Dispose()
    {
        if (_playerInput == null) return;
        _playerInput.OnCrouchChannel -= HandleCrouchInput;
    }
    
    private void HandleCrouchInput(bool isCrouch)
    {
        _isCrouch = isCrouch;
    }

    protected override bool Statement()
    {
        return _isCrouch;
    }
}
