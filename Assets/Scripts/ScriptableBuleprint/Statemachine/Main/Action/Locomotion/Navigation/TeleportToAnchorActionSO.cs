using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "TeleportToAnchorAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Navigation/Teleport To Anchor")]
public class TeleportToAnchorActionSO : StateActionSO
{
    [SerializeField] private TransformAnchor _destinationAnchor;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new TeleportToAnchorAction(_destinationAnchor);
    }
}

public class TeleportToAnchorAction : StateAction
{
    private readonly TransformAnchor _destinationAnchor;
    private CharacterTeleporter _teleporter;
    private GameObject _owner;

    public TeleportToAnchorAction(TransformAnchor destinationAnchor)
    {
        _destinationAnchor = destinationAnchor;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner;
        _teleporter = stateMachine.GetComponent<CharacterTeleporter>();

        if (_teleporter == null)
            Debug.LogError("TeleportToAnchorAction requires CharacterTeleporter.", _owner);
    }

    public override void OnStateEnter()
    {
        if (_teleporter == null)
            return;

        if (_destinationAnchor == null || !_destinationAnchor.IsSet)
        {
            Debug.LogWarning("TeleportToAnchorAction destination anchor is not set.", _owner);
            return;
        }

        _teleporter.Teleport(_destinationAnchor.Value.position);
    }

    public override void OnUpdate() { }
}
