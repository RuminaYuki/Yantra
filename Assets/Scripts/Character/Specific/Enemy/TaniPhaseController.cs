using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[RequireComponent(typeof(StateMachineController))]
public class TaniPhaseController : MonoBehaviour
{
    [SerializeField] private TransitionTableSO[] _transitionTables;
    [Header("Event Chanel")]
    [SerializeField] private IntEventChannelSO _sealTaniEvent;
    private StateMachineController _stateMachineController;

    
    void Awake()
    {
        _stateMachineController = GetComponent<StateMachineController>();
    }

    void OnEnable()
    {
        _sealTaniEvent.Raised += HandleSealTaniEvent;
    }
    void OnDisable()
    {
        _sealTaniEvent.Raised -= HandleSealTaniEvent;
    }
    private void HandleSealTaniEvent(int amount)
    {
        if(amount == 0) return;
        _stateMachineController.ChangeTable(0,_transitionTables[amount]);
        Debug.Log("Trigget _sealTaniEvent amount "+ amount);
    }
}
