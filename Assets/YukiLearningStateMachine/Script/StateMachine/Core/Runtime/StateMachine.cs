using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yuki.Learning.StateMachine
{
    public class StateMachine
    {
        private readonly GameObject _owner;

        private readonly HashSet<Condition> _conditions =
            new HashSet<Condition>();

        private State _currentState;

        private StateTransition[] _anyTransitions =
            Array.Empty<StateTransition>();

        private bool _isDisposed;

        public StateMachine(GameObject owner)
        {
            _owner = owner != null
                ? owner
                : throw new ArgumentNullException(nameof(owner));
        }

        public T GetComponent<T>() where T : Component
        {
            return _owner.GetComponent<T>();
        }

        public void RegisterCondition(Condition condition)
        {
            if (condition == null)
            {
                Debug.LogError(
                    $"StateMachine on {_owner.name} cannot register a null condition.",
                    _owner);
                return;
            }

            if (_isDisposed)
            {
                Debug.LogWarning(
                    $"StateMachine on {_owner.name} is already disposed.",
                    _owner);
                return;
            }

            _conditions.Add(condition);
        }

        public void OnUpdate()
        {
            if (_isDisposed || _currentState == null)
            {
                return;
            }

            if (TryGetNextState(out State nextState))
            {
                ChangeState(nextState);
            }

            _currentState.OnUpdate();
        }

        public void OnFixedUpdate()
        {
            if (_isDisposed || _currentState == null)
            {
                return;
            }

            _currentState.OnFixedUpdate();
        }

        public void SetInitialState(State initialState)
        {
            if (_isDisposed)
            {
                Debug.LogWarning(
                    $"StateMachine on {_owner.name} is already disposed.",
                    _owner);
                return;
            }

            if (initialState == null)
            {
                Debug.LogError(
                    $"StateMachine on {_owner.name} cannot use a null initial state.",
                    _owner);
                return;
            }

            _currentState = initialState;
            _currentState.OnStateEnter();
        }

        public void ChangeState(State nextState)
        {
            if (_isDisposed ||
                nextState == null ||
                ReferenceEquals(_currentState, nextState))
            {
                return;
            }

            _currentState?.OnStateExit();

            _currentState = nextState;
            _currentState.OnStateEnter();
        }

        public void SetAnyTransitions(
            StateTransition[] transitions)
        {
            if (_isDisposed)
            {
                return;
            }

            _anyTransitions =
                transitions ?? Array.Empty<StateTransition>();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _currentState?.OnStateExit();

            foreach (Condition condition in _conditions)
            {
                condition.Dispose();
            }

            _conditions.Clear();

            _currentState = null;
            _anyTransitions =
                Array.Empty<StateTransition>();
        }

        private bool TryGetNextState(
            out State nextState)
        {
            if (TryGetAnyTransition(out nextState))
            {
                return true;
            }

            return _currentState.TryGetTransition(
                out nextState);
        }

        private bool TryGetAnyTransition(
            out State nextState)
        {
            nextState = null;

            foreach (
                StateTransition transition
                in _anyTransitions)
            {
                if (transition == null ||
                    !transition.TryGetNextState(
                        out State candidate))
                {
                    continue;
                }

                // Prevent re-entering the active state
                // while its condition remains true.
                if (ReferenceEquals(
                    candidate,
                    _currentState))
                {
                    continue;
                }

                nextState = candidate;
                break;
            }

            foreach (
                StateTransition transition
                in _anyTransitions)
            {
                transition?.ClearConditionsCache();
            }

            return nextState != null;
        }
    }
}