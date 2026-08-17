using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[RequireComponent(typeof(StateMachineController))]
public class TaniPhaseController : MonoBehaviour
{
    [SerializeField] private TransitionTableSO[] _transitionTableSOs;
    private StateMachineController _stateMachineController;
    void Awake()
    {
        _stateMachineController.GetComponent<StateMachineController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
