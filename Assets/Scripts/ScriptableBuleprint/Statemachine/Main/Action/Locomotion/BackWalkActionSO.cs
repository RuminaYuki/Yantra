using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "BackWalkAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Back Walk")]
public class BackWalkActionSO : StateActionSO
{
    [SerializeField] private TransformAnchor _targetAnchor;

    [SerializeField, Range(0f,45f)]
    private float _facingTolerance = 5f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new BackWalkAction(_targetAnchor,_facingTolerance);
    }
}

public class BackWalkAction : StateAction
{
    private readonly TransformAnchor _targetAnchor;
    private readonly float _facingTolerance;

    private BaseLocomotion _locomotion;
    private Transform _owner;
    private bool _startedMoving;

    public BackWalkAction(
        TransformAnchor targetAnchor,
        float facingTolerance)
    {
        _targetAnchor = targetAnchor;
        _facingTolerance = facingTolerance;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner.transform;
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("BackWalkAction requires BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        _startedMoving = false;
        _locomotion?.ClearMovementDirection();
    }

    public override void OnUpdate()
    {
        if (_locomotion == null)
            return;

        if (!_startedMoving)
        {
            if (!TryGetTarget(out Transform target))
                return;

            Vector3 facingDirection = target.position - _owner.position;
            facingDirection.y = 0f;
            _locomotion.SetFacingDirection(facingDirection);

            float angle = Vector3.Angle(_owner.forward,facingDirection);
            if (angle > _facingTolerance)
                return;

            _startedMoving = true;
        }

        _locomotion.SetMovementDirection(-_owner.forward);
    }

    public override void OnStateExit()
    {
        _locomotion?.ClearMovementDirection();
    }

    private bool TryGetTarget(out Transform target)
    {
        target = null;

        if (_targetAnchor == null || !_targetAnchor.IsSet)
            return false;

        target = _targetAnchor.Value;
        return target != null;
    }
}
