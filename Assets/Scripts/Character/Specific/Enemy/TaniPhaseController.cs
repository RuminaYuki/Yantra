using UnityEngine;
using Yuki.Learning.StateMachine.ScriptableObjects;
[RequireComponent(typeof(StateMachineController))]
public class TaniPhaseController : MonoBehaviour
{
    [SerializeField] private TransitionTableSO[] _transitionTables;
    [Header("Event Chanel")]
    [SerializeField] private IntEventChannelSO _sealTaniEvent;
    private StateMachineController _stateMachineController;
    public int SealCount{get; private set;}

    
    void Awake()
    {
        _stateMachineController = GetComponent<StateMachineController>();
        _sealTaniEvent.Raised += HandleSealTaniEvent;
    }

    void OnEnable()
    {
        if (_sealTaniEvent == null)
        {
            Debug.LogError("Seal Tani event channel is not assigned.", this);
            return;
        }

        _sealTaniEvent.Raised += HandleSealTaniEvent;
    }
    void OnDisable()
    {
        if (_sealTaniEvent != null)
            _sealTaniEvent.Raised -= HandleSealTaniEvent;
    }
    private void HandleSealTaniEvent(int amount)
    {
        Debug.Log(amount);
        if (amount <= 0)
            return;

        if (_transitionTables == null || amount - 1 >= _transitionTables.Length)
        {
            Debug.LogWarning(
                $"No Tani transition table is configured for objective progress {amount - 1}.",
                this);
            return;
        }

        _stateMachineController.ChangeTable(0, _transitionTables[amount - 1]);
        SealCount++;
        Debug.Log($"Triggered seal Tani event with amount {amount}.", this);
    }
}
