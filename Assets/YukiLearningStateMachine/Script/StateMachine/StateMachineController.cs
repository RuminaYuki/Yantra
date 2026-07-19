using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

public class StateMachineController : MonoBehaviour
{
    [SerializeField]
    private TransitionTableSO[] _transitionTables;

    private StateMachine[] _stateMachines;

    private void Awake()
    {
        _stateMachines =
            new StateMachine[_transitionTables.Length];

        for (int i = 0; i < _transitionTables.Length; i++)
        {
            StateMachine stateMachine = new StateMachine(gameObject);

            State initialState = _transitionTables[i].CreateInitialState(stateMachine);

            stateMachine.SetInitialState(initialState);

            _stateMachines[i] = stateMachine;
        }
    }

    private void Update()
    {
        foreach (StateMachine stateMachine in _stateMachines)
        {
            stateMachine.OnUpdate();
        }
    }

    private void FixedUpdate()
    {
        foreach (StateMachine stateMachine in _stateMachines)
        {
            stateMachine.OnFixedUpdate();
        }
    }
}