using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

public class StateMachineController : MonoBehaviour
{
    [SerializeField] private TransitionTableSO[] _transitionTables;

    private StateMachine[] _stateMachines;

    private void Start()
    {
        _stateMachines = new StateMachine[_transitionTables.Length];

        for (int i = 0; i < _transitionTables.Length; i++)
        {
            _stateMachines[i] = CreateStateMachine(_transitionTables[i]);
        }
    }

    private void Update()
    {
        if (_stateMachines == null)
        {
            return;
        }

        foreach (StateMachine stateMachine in _stateMachines)
        {
            stateMachine?.OnUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (_stateMachines == null)
        {
            return;
        }

        foreach (StateMachine stateMachine in _stateMachines)
        {
            stateMachine?.OnFixedUpdate();
        }
    }

    public void ChangeTable(int tableIndex, TransitionTableSO newTable)
    {
        if (_stateMachines == null)
        {
            Debug.LogWarning("StateMachineController has not started yet.", this);
            return;
        }

        if (tableIndex < 0 || tableIndex >= _stateMachines.Length)
        {
            Debug.LogError($"Invalid state machine index: {tableIndex}.", this);
            return;
        }

        if (newTable == null)
        {
            Debug.LogError("Cannot change to a null TransitionTableSO.", this);
            return;
        }

        // ออกจาก State และยกเลิก Condition ของ Table เก่า
        _stateMachines[tableIndex]?.Dispose();

        _transitionTables[tableIndex] = newTable;
        _stateMachines[tableIndex] = CreateStateMachine(newTable);
    }

    private StateMachine CreateStateMachine(TransitionTableSO table)
    {
        if (table == null)
        {
            return null;
        }

        StateMachine stateMachine = new StateMachine(gameObject);
        State initialState = table.CreateInitialState(stateMachine);

        stateMachine.SetInitialState(initialState);

        return stateMachine;
    }
    public void RestartTable(int tableIndex)
    {
        if (_stateMachines == null)
        {
            Debug.LogWarning(
                "StateMachineController has not started yet.",
                this);

            return;
        }

        if (tableIndex < 0 || tableIndex >= _stateMachines.Length)
        {
            Debug.LogError(
                $"Invalid state machine index: {tableIndex}.",
                this);

            return;
        }

        TransitionTableSO currentTable = _transitionTables[tableIndex];

        if (currentTable == null)
        {
            Debug.LogError(
                $"Transition table at index {tableIndex} is null.",
                this);

            return;
        }

        _stateMachines[tableIndex]?.Dispose();
        _stateMachines[tableIndex] =
            CreateStateMachine(currentTable);
    }

    private void OnDestroy()
    {
        if (_stateMachines == null)
        {
            return;
        }

        foreach (StateMachine stateMachine in _stateMachines)
        {
            stateMachine?.Dispose();
        }
    }
}