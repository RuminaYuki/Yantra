using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "QuickTurnCheckCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/QuickTurnCheck")]
public class QuickTurnCheckConditionSO : StateConditionSO
{
    [SerializeField, Range(0.1f, 1f)] private float _inputThreshold = 0.5f;
    [SerializeField, Min(0f)] private float _reversalWindow = 0.25f;
    [SerializeField, Min(0.01f)] private float _conditionBuffer = 0.1f;

    public override Condition CreateCondition()
    {
        return new QuickTurnCheckCondition(
            _inputThreshold,
            _reversalWindow,
            _conditionBuffer);
    }
}

public class QuickTurnCheckCondition : Condition
{
    private readonly float _inputThreshold;
    private readonly float _reversalWindow;
    private readonly float _conditionBuffer;
    private PlayerLocomotion _playerLocomotion;
    private int _previousDirection;
    private float _lastDirectionTime = float.NegativeInfinity;
    private float _triggerUntil = float.NegativeInfinity;

    public QuickTurnCheckCondition(
        float inputThreshold,
        float reversalWindow,
        float conditionBuffer)
    {
        _inputThreshold = inputThreshold;
        _reversalWindow = reversalWindow;
        _conditionBuffer = conditionBuffer;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _playerLocomotion = stateMachine.GetComponent<PlayerLocomotion>();

        if (_playerLocomotion == null)
            Debug.LogError("QuickTurnCheckCondition cannot find PlayerLocomotion.");
    }

    protected override bool Statement()
    {
        if (_playerLocomotion == null)
            return false;

        float currentTime = Time.time;
        Vector3 input = _playerLocomotion.GetDirection();

        if (Mathf.Abs(input.x) >= _inputThreshold)
        {
            ResetDirection();
            return currentTime <= _triggerUntil;
        }

        int currentDirection = GetDirectionSign(input.z);

        if (currentDirection != 0)
        {
            bool changedDirection =
                _previousDirection != 0 &&
                currentDirection != _previousDirection;

            bool changedRecently =
                currentTime - _lastDirectionTime <= _reversalWindow;

            if (changedDirection && changedRecently)
                _triggerUntil = currentTime + _conditionBuffer;

            _previousDirection = currentDirection;
            _lastDirectionTime = currentTime;
        }
        else if (currentTime - _lastDirectionTime > _reversalWindow)
        {
            ResetDirection();
        }

        return currentTime <= _triggerUntil;
    }

    private int GetDirectionSign(float inputZ)
    {
        if (inputZ >= _inputThreshold)
            return 1;

        if (inputZ <= -_inputThreshold)
            return -1;

        return 0;
    }

    private void ResetDirection()
    {
        _previousDirection = 0;
        _lastDirectionTime = float.NegativeInfinity;
    }
}