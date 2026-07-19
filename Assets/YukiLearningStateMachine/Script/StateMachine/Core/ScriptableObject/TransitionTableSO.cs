using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yuki.Learning.StateMachine.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "NewTransitionTable",
        menuName = "YUKI Learning State Machine/Transition Table")]
    public class TransitionTableSO : ScriptableObject
    {
        [SerializeField]
        private StateSO _initialState;

        [SerializeField]
        private AnyTransitionItem[] _anyTransitions = Array.Empty<AnyTransitionItem>();

        [SerializeField]
        private TransitionItem[] _transitions = Array.Empty<TransitionItem>();

        public State CreateInitialState(StateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

            if (_initialState == null)
            {
                throw new InvalidOperationException(
                    $"Transition table {name} does not have an initial state.");
            }

            var runtimeStates = new Dictionary<StateSO, State>();
            var transitionLists = new Dictionary<State, List<StateTransition>>();
            var runtimeConditions = new Dictionary<StateConditionSO, Condition>();

            GetOrCreateState(
                _initialState,
                stateMachine,
                runtimeStates,
                transitionLists);

            BuildLocalTransitions(
                stateMachine,
                runtimeStates,
                transitionLists,
                runtimeConditions);

            foreach (KeyValuePair<State, List<StateTransition>> pair in transitionLists)
            {
                pair.Key.SetTransitions(pair.Value.ToArray());
            }

            StateTransition[] runtimeAnyTransitions = BuildAnyTransitions(
                stateMachine,
                runtimeStates,
                transitionLists,
                runtimeConditions);

            stateMachine.SetAnyTransitions(runtimeAnyTransitions);

            return runtimeStates[_initialState];
        }

        private void BuildLocalTransitions(
            StateMachine stateMachine,
            Dictionary<StateSO, State> runtimeStates,
            Dictionary<State, List<StateTransition>> transitionLists,
            Dictionary<StateConditionSO, Condition> runtimeConditions)
        {
            foreach (TransitionItem item in _transitions ?? Array.Empty<TransitionItem>())
            {
                State fromState = GetOrCreateState(
                    item.FromState,
                    stateMachine,
                    runtimeStates,
                    transitionLists);

                State toState = GetOrCreateState(
                    item.ToState,
                    stateMachine,
                    runtimeStates,
                    transitionLists);

                StateCondition[][] conditionGroups = CreateConditionGroups(
                    item.ConditionGroups,
                    runtimeConditions);

                transitionLists[fromState].Add(
                    new StateTransition(toState, conditionGroups));
            }
        }

        private StateTransition[] BuildAnyTransitions(
            StateMachine stateMachine,
            Dictionary<StateSO, State> runtimeStates,
            Dictionary<State, List<StateTransition>> transitionLists,
            Dictionary<StateConditionSO, Condition> runtimeConditions)
        {
            AnyTransitionItem[] usages =
                _anyTransitions ?? Array.Empty<AnyTransitionItem>();

            var runtimeTransitions = new List<StateTransition>(usages.Length);

            foreach (AnyTransitionItem item in usages)
            {
                State toState = GetOrCreateState(
                    item.ToState,
                    stateMachine,
                    runtimeStates,
                    transitionLists);

                StateCondition[][] conditionGroups = CreateConditionGroups(
                    item.ConditionGroups,
                    runtimeConditions);

                runtimeTransitions.Add(
                    new StateTransition(toState, conditionGroups));
            }

            return runtimeTransitions.ToArray();
        }

        private StateCondition[][] CreateConditionGroups(
            ConditionGroupUsage[] groupUsages,
            Dictionary<StateConditionSO, Condition> runtimeConditions)
        {
            ConditionGroupUsage[] usages =
                groupUsages ?? Array.Empty<ConditionGroupUsage>();

            var runtimeGroups = new StateCondition[usages.Length][];

            for (int groupIndex = 0; groupIndex < usages.Length; groupIndex++)
            {
                ConditionUsage[] conditionUsages =
                    usages[groupIndex].Conditions ?? Array.Empty<ConditionUsage>();

                var runtimeGroup = new StateCondition[conditionUsages.Length];

                for (int conditionIndex = 0;
                    conditionIndex < conditionUsages.Length;
                    conditionIndex++)
                {
                    ConditionUsage usage = conditionUsages[conditionIndex];
                    Condition runtimeCondition = GetOrCreateCondition(
                        usage.Condition,
                        runtimeConditions);

                    bool expectedResult = usage.ExpectedResult == Result.True;

                    runtimeGroup[conditionIndex] = new StateCondition(
                        runtimeCondition,
                        expectedResult);
                }

                runtimeGroups[groupIndex] = runtimeGroup;
            }

            return runtimeGroups;
        }

        private State GetOrCreateState(
            StateSO stateSO,
            StateMachine stateMachine,
            Dictionary<StateSO, State> runtimeStates,
            Dictionary<State, List<StateTransition>> transitionLists)
        {
            if (stateSO == null)
            {
                throw new InvalidOperationException(
                    $"Transition table {name} contains a transition with a missing state.");
            }

            if (runtimeStates.TryGetValue(stateSO, out State existingState))
            {
                return existingState;
            }

            State newState = stateSO.CreateState(stateMachine);

            runtimeStates.Add(stateSO, newState);
            transitionLists.Add(newState, new List<StateTransition>());

            return newState;
        }

        private Condition GetOrCreateCondition(
            StateConditionSO conditionSO,
            Dictionary<StateConditionSO, Condition> runtimeConditions)
        {
            if (conditionSO == null)
            {
                throw new InvalidOperationException(
                    $"Transition table {name} contains a missing condition.");
            }

            if (runtimeConditions.TryGetValue(
                conditionSO,
                out Condition existingCondition))
            {
                return existingCondition;
            }

            Condition newCondition = conditionSO.CreateCondition();
            runtimeConditions.Add(conditionSO, newCondition);

            return newCondition;
        }

        [Serializable]
        public struct AnyTransitionItem
        {
            public StateSO ToState;
            public ConditionGroupUsage[] ConditionGroups;
        }

        [Serializable]
        public struct TransitionItem
        {
            public StateSO FromState;
            public ConditionGroupUsage[] ConditionGroups;
            public StateSO ToState;
        }

        public enum Result
        {
            True,
            False
        }

        [Serializable]
        public struct ConditionUsage
        {
            public StateConditionSO Condition;
            public Result ExpectedResult;
        }

        [Serializable]
        public struct ConditionGroupUsage
        {
            public ConditionUsage[] Conditions;
        }
    }
}
