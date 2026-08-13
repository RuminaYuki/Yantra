using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "WalkAroundAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Walk Around")]
public class WalkAroundActionSO : StateActionSO
{
    [SerializeField] private TransformAnchor _targetAnchor;

    [Header("if have FloatDataSO, it will override the value")]
    [SerializeField] private FloatDataSO _radiusData;
    [SerializeField, Min(0.01f)] private float _radius = 2f;

    [SerializeField, Min(0f)] private float _radiusCorrection = 1f;
    [SerializeField, Min(0f)] private float _radiusTolerance = 0.2f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        float radius = _radiusData != null ? _radiusData.Value : _radius;
        return new WalkAroundAction(
            _targetAnchor,
            radius,
            _radiusCorrection,
            _radiusTolerance);
    }
}

public class WalkAroundAction : StateAction
{
    private const float DirectionThreshold = 0.01f;

    private readonly TransformAnchor _targetAnchor;
    private readonly float _radius;
    private readonly float _radiusCorrection;
    private readonly float _radiusTolerance;

    private BaseLocomotion _locomotion;
    private Transform _owner;
    private float _walkSide;

    public WalkAroundAction(
        TransformAnchor targetAnchor,
        float radius,
        float radiusCorrection,
        float radiusTolerance)
    {
        _targetAnchor = targetAnchor;
        _radius = Mathf.Max(0.01f,radius);
        _radiusCorrection = radiusCorrection;
        _radiusTolerance = radiusTolerance;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner.transform;
        _locomotion = stateMachine.GetComponent<BaseLocomotion>();

        if (_locomotion == null)
            Debug.LogError("WalkAroundAction requires BaseLocomotion.");
    }

    public override void OnStateEnter()
    {
        _walkSide = _walkSide == 0f
            ? (Random.value < 0.5f ? -1f : 1f)
            : -_walkSide;
        _locomotion?.ClearMovementDirection();
    }

    public override void OnUpdate()
    {
        if (_locomotion == null)
            return;

        if (!TryGetTarget(out Transform target))
        {
            _locomotion.ClearMovementDirection();
            return;
        }

        Vector3 fromTarget = _owner.position - target.position;
        fromTarget.y = 0f;

        if (fromTarget.sqrMagnitude <= DirectionThreshold)
        {
            fromTarget = -_owner.forward;
            fromTarget.y = 0f;
        }

        float currentRadius = fromTarget.magnitude;
        Vector3 radialDirection = fromTarget / currentRadius;

        Vector3 tangentDirection =
            Vector3.Cross(Vector3.up,radialDirection) * _walkSide;
        float radiusError = currentRadius - _radius;
        Vector3 moveDirection = tangentDirection;

        if (Mathf.Abs(radiusError) > _radiusTolerance)
        {
            float correctedError =
                Mathf.Abs(radiusError) - _radiusTolerance;
            float correctionWeight =
                Mathf.Clamp01(correctedError * _radiusCorrection);
            Vector3 correctionDirection =
                radiusError > 0f ? -radialDirection : radialDirection;

            moveDirection = Vector3.Slerp(
                tangentDirection,
                correctionDirection,
                correctionWeight);
        }

        _locomotion.SetMovementDirection(moveDirection.normalized);
        _locomotion.SetFacingDirection(-radialDirection);
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
